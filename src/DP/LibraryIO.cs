// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
namespace DAZ_Installer
{
    internal enum DPUserProperty
    {
        ProductName, Tags
    }
    internal static class LibraryIO
    {
        internal static List<DPProductRecord> ProductRecords;
        internal static bool initalized = false;
        private const string pathToExtractionRecords = "Records/ExtractionRecords";
        private const string pathToProductRecords = "Records/ProductRecords";
        private static List<uint> ids;
        private static uint previousID = 1;
        public static HashSet<string> previouslyInstalledArchives { get; set; } = new HashSet<string>();
        private const string piaLocation = "previously_installed_archive_names.txt";
        /// <summary>
        /// Initalization attempts to parse all product files into memory (ProductRecords property).
        /// </summary>
        static internal void Initalize()
        {
            string[] precs = new string[0];
            try
            {
                precs = Directory.GetFiles(pathToProductRecords).Where(file => Path.GetExtension(file) == ".prec").ToArray();
            } catch (Exception e)
            {
                DPCommon.WriteToLog($"Unable to get product records. Reason: {e}");
            }
            ids = new List<uint>(precs.Length);
            ProductRecords = new List<DPProductRecord>(precs.Length);
            // Load Extraction Records first.
            foreach (var file in precs)
            {
                ProcessPREC(file);
            }
            if (GetPreviouslyInstalledArchives(out HashSet<string> hashset))
            {
                previouslyInstalledArchives = hashset;
            }

        }

        // PREC format
        // Product Name: Product Name from Supplement, overwritten, or filename \n
        // Tags: string[0] or ... | seperated by |.| \n
        // Directories: string[0] or ... | seperated by |.| \n
        // Time extracted : datetime
        // File Extracted: string[0] or ... | seperated by |.| \n
        // Expected Extraction Record: 

        
        /// <summary>
        /// Creates a DPProductRecord and an associated DPExtractionRecord
        /// </summary>
        /// <param name="archive"></param>
        /// <returns></returns>
        internal static DPProductRecord CreateNewRecord(ref DPArchive archive, string productName, string[] tags, DateTime timeExtracted, bool createOnlyExtraction )
        {
            var uid = GetUniqueID();
            var objs = ConfirmFilesExtraction(ref archive);
            string[] foundFiles = (string[]) objs[0];
            string[] missingFiles = (string[]) objs[1];
            var workingExtractionRecord = new DPExtractionRecord(Path.GetFileName(archive.fileName), foundFiles, DateTime.Now, archive.erroredFiles.Count, foundFiles.Length, missingFiles, archive.erroredFiles.ToArray(), archive.type, uid);
            WriteRecordToDisk(erecord: workingExtractionRecord);
            var expectedExtractionRecordLocation = Path.Combine(pathToExtractionRecords,GetExtractRecordName(workingExtractionRecord));
            if (!createOnlyExtraction)
            {
                string imageLocation = "";
                if (DPSettings.downloadImages == SettingOptions.Yes)
                {
                    imageLocation = DPNetwork.DownloadImage(workingExtractionRecord.ArchiveFileName);
                }
                else if (DPSettings.downloadImages == SettingOptions.Prompt)
                {
                    // Pre-check if the archive file name starts with "IM"
                    if (workingExtractionRecord.ArchiveFileName.StartsWith("IM"))
                    {
                        var result = extractControl.extractPage.DoPromptMessage("Do you wish to download the thumbnail for this product?", "Download Thumbnail Prompt", System.Windows.Forms.MessageBoxButtons.YesNo);
                        if (result == System.Windows.Forms.DialogResult.Yes) imageLocation = DPNetwork.DownloadImage(workingExtractionRecord.ArchiveFileName);
                    }
                }

                var workingProductRecord = new DPProductRecord(productName, tags, ConvertDPFoldersToStringArr(archive.folders), timeExtracted, foundFiles, expectedExtractionRecordLocation, imageLocation, uid);
                WriteRecordToDisk(precord: workingProductRecord);
                return workingProductRecord;
            }
            return null;
           
        }

        private static string[] ConvertDPFoldersToStringArr(Dictionary<string, DPFolder> folders)
        {
            string[] strFolders = new string[folders.Count];
            string[] keys = folders.Keys.ToArray();
            for (var i = 0; i < strFolders.Length; i++)
            {
                strFolders[i] = folders[keys[i]].path;
            }
            return strFolders;
        }

        /// <summary>
        /// Finds files that were supposedly extracted in explorer.
        /// </summary>
        /// <param name="archive"></param>
        /// <returns>An object array with length of 2. [0] - Found Files | [1]  - Missing Files</returns>
        private static object[] ConfirmFilesExtraction(ref DPArchive archive)
        {
            List<string> foundFiles = new List<string>((int) archive.fileCount);
            List<string> missingFiles = new List<string>();
            foreach(var file in archive.contents)
            {
                if (!file.extract) missingFiles.Add(file.path);
                else
                {
                    var dest = file.destinationPath;
                    if (dest == null) continue;
                    if (File.Exists(dest)) foundFiles.Add(file.path);
                    else missingFiles.Add(file.path);
                }
            }

            // Remove any occurances of errored files in missing files.
            for (int i = missingFiles.Count - 1; i >= 0; i--)
            {
                if (archive.erroredFiles.Contains(missingFiles[i]))
                {
                    missingFiles.RemoveAt(i);
                }
            }

            return new object[] { foundFiles.ToArray(), missingFiles.ToArray() };
        }
        // Used for reading.
        static private void ProcessPREC(string filePath)
        {
            // We know filePath exists so no need to check twice.
            // Open file as texteditor, get all text since it should most definitely be less than 64KB. Otherwise, the universe is fucking with me.
            try
            {
                var text = File.ReadAllText(filePath);
                var lines = text.Split('\n');
                var productName = lines[0].Trim();
                var tags = HandleSeperators(lines[1].Trim());
                var directories = HandleSeperators(lines[2].Trim());
                var filesExtracted = HandleSeperators(lines[3].Trim());
                DateTime timeExtracted = DateTime.Parse(lines[4].Trim());
                var expectedExtractionRecordLocation = lines[5].Trim();
                if (expectedExtractionRecordLocation == "NULL") expectedExtractionRecordLocation = null;
                var expectedImageLocation = lines[6].Trim();
                if (expectedImageLocation == "NULL") expectedImageLocation = null;
                var id = uint.Parse(lines[7].Trim());

                var workingPREC = new DPProductRecord(productName, tags, directories, timeExtracted, filesExtracted, expectedExtractionRecordLocation, expectedImageLocation, id);
                ProductRecords.Add(workingPREC);
            } catch (Exception e)
            {
                DPCommon.WriteToLog($"Unable to successfully process product record. REASON: {e}");
            }
        }

        public static void DeleteRecord(object record)
        {
            if (record.GetType() == typeof(DPExtractionRecord))
            {
                var dpe = (DPExtractionRecord) record;
                var location = Path.Combine(pathToExtractionRecords, GetExtractRecordName(dpe));

                // Check if it still exists.
                try
                {
                    if (File.Exists(location)) File.Delete(location);
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Unable to delete extraction record {dpe.ArchiveFileName}. Reason: {e}");
                }

            }
            else if (record.GetType() == typeof(DPProductRecord))
            {
                var dpp = (DPProductRecord)record;
                var location = Path.Combine(pathToProductRecords, GetProductRecordName(dpp));
                // Check if it still exists.

                try
                {
                    if (File.Exists(location)) File.Delete(location);
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Unable to delete product record {dpp.ProductName}. Reason: {e}");
                }

            }
            else DPCommon.WriteToLog("Tried to delete record but got unexpected OBJECT.");
        }

        // EREC Format
        // Archive File Name: string
        // Files Location: string[0] or ... | seperated by |.| \n
        // Time extracted: datetime
        // Errors : int
        // Files Extracted : int
        // Missing Files: string[0] or .. | seperated by |.| \n
        // Errored Files: string[0] or .. | seperated by |.| \n
        // Archive Type : ArchiveType as int.
        // ID

        // Used for reading.
        static private DPExtractionRecord ProcessEREC(string filePath)
        {
            try
            {
                var text = File.ReadAllText(filePath);
                var lines = text.Split('\n');
                var archiveName = lines[0].Trim();
                var extractedFilesLocations = HandleSeperators(lines[1].Trim());
                DateTime timeExtracted = DateTime.Parse(lines[2].Trim());
                var errorCount = int.Parse(lines[3].Trim());
                var successCount = int.Parse(lines[4].Trim());
                var missingFiles = HandleSeperators(lines[5].Trim());
                var erroredFiles = HandleSeperators(lines[6].Trim());
                var archiveType = (ArchiveType)int.Parse(lines[7].Trim());
                var id = uint.Parse(lines[8].Trim());
                
                return new DPExtractionRecord(archiveName, extractedFilesLocations, timeExtracted, errorCount, successCount, missingFiles, erroredFiles, archiveType, id);
                
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog($"Unable to successfully process extraction record. REASON: {e}");
            }
            return null;
        }

        static internal DPExtractionRecord LoadAssociatedExtractionRecord(string expectedPath)
        {
            // Check if file exists.
            if (File.Exists(expectedPath)) return ProcessEREC(expectedPath);
            else DPCommon.WriteToLog($"Did not find extraction record at expected path: {expectedPath}");
            return null;
        }

        static internal DPProductRecord UpdateProductRecord(DPProductRecord record, Dictionary<DPUserProperty, object> updates)
        {
            string newProductName = null;
            string[] newTags = null;

            // Check if we have a new product name.
            if (updates.ContainsKey(DPUserProperty.ProductName)) newProductName = (string)updates[DPUserProperty.ProductName];
            if (updates.ContainsKey(DPUserProperty.Tags)) newTags = (string[])updates[DPUserProperty.Tags];

            // If only new product name
            if (newTags == null && !string.IsNullOrEmpty(newProductName))
            {
                var newRecord = new DPProductRecord(newProductName, record.Tags, record.Directories, record.Time, record.FilesExtracted, record.ExpectedExtractionRecordLocation, record.ExpectedImageLocation, record.ID);
                WriteRecordToDisk(newRecord);
                return newRecord;
            } else if (newTags != null && !string.IsNullOrEmpty(newProductName))
            {
                var newRecord = new DPProductRecord(record.ProductName, newTags, record.Directories, record.Time, record.FilesExtracted, record.ExpectedExtractionRecordLocation, record.ExpectedImageLocation, record.ID);
                WriteRecordToDisk(newRecord);
                return newRecord;
            } else
            {
                var newRecord = new DPProductRecord(newProductName, newTags, record.Directories, record.Time, record.FilesExtracted, record.ExpectedExtractionRecordLocation, record.ExpectedImageLocation, record.ID);
                WriteRecordToDisk(newRecord);
                return newRecord;

            }
        }

        // PREC format
        // Product Name: Product Name from Supplement, overwritten, or filename \n
        // Tags: string[0] or ... | seperated by |.| \n
        // Directories: string[0] or ... | seperated by |.| \n
        // Time extracted : datetime
        // File Extracted: string[0] or ... | seperated by |.| \n
        // Expected Extraction Record:  "NULL" or ...
        // Expected Image Location: "NULL" or ...
        // ID
        static private string ConvertPRecordToText(ref DPProductRecord record)
        {
            const string seperator = "|.|";
            const int stringcapacity = 7;
            StringBuilder builder = new StringBuilder(stringcapacity);
            builder.AppendLine(record.ProductName);
            // Cleanse tags from "" tags.
            builder.AppendLine(string.Join(seperator, record.Tags));
            builder.AppendLine(string.Join(seperator, record.Directories));
            builder.AppendLine(string.Join(seperator, record.FilesExtracted));
            builder.AppendLine(record.Time.ToString());
            if (string.IsNullOrEmpty(record.ExpectedExtractionRecordLocation)) builder.AppendLine("NULL");
            else builder.AppendLine(record.ExpectedExtractionRecordLocation);
            if (string.IsNullOrEmpty(record.ExpectedImageLocation)) builder.AppendLine("NULL");
            else builder.AppendLine(record.ExpectedImageLocation);
            builder.Append(record.ID);
            return builder.ToString();
        }

        // EREC Format
        // Archive File Name: string
        // Files Location: string[0] or ... | seperated by |.| \n
        // Time extracted: datetime
        // Errors : int
        // Files Extracted : int
        // Missing Files: string[0] or .. | seperated by |.| \n
        // Errored Files: string[0] or .. | seperated by |.| \n
        // Archive Type : ArchiveType as int.
        // ID

        static private string ConvertERecordToText(ref DPExtractionRecord record)
        {
            const string seperator = "|.|";
            const int stringcapacity = 7;
            StringBuilder builder = new StringBuilder(stringcapacity);
            builder.AppendLine(record.ArchiveFileName);
            builder.AppendLine(string.Join(seperator, record.FilesLocation));
            builder.AppendLine(record.Time.ToString());
            builder.AppendLine(record.Errors.ToString());
            builder.AppendLine(record.FilesExtracted.ToString());
            builder.AppendLine(string.Join(seperator, record.MissingFiles));
            builder.AppendLine(string.Join(seperator, record.ErroredFiles));
            builder.AppendLine(((int)record.Type).ToString());
            builder.Append(record.ID);
            return builder.ToString();
        }


        // 0 - ERRORED UINT
        static private void WriteRecordToDisk(DPProductRecord precord = null, DPExtractionRecord erecord = null)
        {
            if (precord != null)
            {
                File.WriteAllText(
                    Path.Combine(pathToProductRecords, GetProductRecordName(precord)),
                    ConvertPRecordToText(ref precord));
            } 
            if (erecord != null)
            {
                File.WriteAllText(
                    Path.Combine(pathToExtractionRecords, GetExtractRecordName(erecord)),
                    ConvertERecordToText(ref erecord));
            }
        }

        // File Name = [IDUpTo8Chars]-[ProductNameMaxUpTo20Char].erec || .prec
        static private string GetProductRecordName(DPProductRecord record)
        {
            return record.ID.ToString() + "-" + GetNameUpTo20Chars(record.ProductName) + ".prec";
        }

        static private string GetExtractRecordName(DPExtractionRecord record)
        {
            return record.ID.ToString() + "-" + GetNameUpTo20Chars(record.ArchiveFileName) + ".erec";
        }

        static private string GetNameUpTo20Chars(string name)
        {
            if(name.Length < 20) return name;
                else return name[..20];
        }

        static private string[] HandleSeperators(string msg)
        {
            var msgLines = msg.Split("|.|");
            for (var i = 0; i < msgLines.Length; i++)
            {
                msgLines[i] = msgLines[i].Trim();
            }
            return msgLines;
        }

        internal static uint GetUniqueID()
        {
            if (previousID + 1 == uint.MaxValue || previousID == uint.MaxValue)
            {
                // We need to search for an opening.
                uint previousQueryID = 0;
                foreach (var x in ids)
                {
                    if (Math.Abs(previousID - x) < 1)
                    {
                        if (previousID < x)
                        {
                            previousID = previousID + 1;
                            return previousID;
                        } else
                        {
                            previousID = x + 1;
                            return previousID;
                        }
                    }
                    previousQueryID = x;
                }
                // I swear...
                DPCommon.WriteToLog("Unable to generate unique ID due to next ID reaching max value (4,294,967,295) and there was no squeeze room.");
                return 0;
            } else
            {
                previousID = previousID + 1;
                return previousID;
            }
        }

        internal static bool WritePreviouslyInstalledArchives()
        {
            try
            {
                File.WriteAllLines(piaLocation, previouslyInstalledArchives);
                return true;
            }
            catch
            {
                return false;
            }
        }


        internal static bool GetPreviouslyInstalledArchives(out HashSet<string> archives)
        {
            try
            {
                archives = File.ReadAllLines(piaLocation).ToHashSet();
                return true;
            }
            catch
            {
                archives = null;
                return false;
            }

        }


    }
}
