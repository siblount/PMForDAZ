// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace DAZ_Installer.DP
{
    internal class DPFile : DPAbstractFile
    {

        // Public static members
        private static Dictionary<string, ContentType> enumPairs { get; } = new Dictionary<string, ContentType>(Enum.GetValues(typeof(ContentType)).Length);
        public static readonly HashSet<string> DAZFormats = new HashSet<string>() { "duf", "dsa", "dse", "daz", "dsf", "duf", "dsb", "dson", "ds", "dsb", "djl", "dsx", "dsi", "dcb", "dbm", "dbc", "dbl", "dso", "dsd", "dsv" };
        public static readonly HashSet<string> GeometryFormats = new HashSet<string>() { "dae", "bvh", "fbx", "obj", "dso", "abc", "mdd", "mi", "u3d" };
        public static readonly HashSet<string> MediaFormats = new HashSet<string>() { "png", "jpg", "hdr", "hdri", "bmp", "gif", "webp", "eps", "raw", "tiff", "tif", "psd", "xcf", "jpeg", "cr2", "svg", "apng", "avif" };
        public static readonly HashSet<string> DocumentFormats = new HashSet<string>() { "txt", "pdf", "doc", "docx", "odt", "html", "ppt", "pptx", "xlsx", "xlsm", "xlsb", "rtf" };
        public static readonly HashSet<string> OtherFormats = new HashSet<string>() { "exe", "lib", "dll", "bat", "cmd" };
        public static readonly HashSet<string> AcceptableImportFormats = new HashSet<string>() { "rar", "zip", "7z" };
        public static Dictionary<string, DPFile> DPFiles = new Dictionary<string, DPFile>();

        // Used for identification
        // TO DO: Add struct for metadata.
        public ContentType contentType;
        public string author;
        public string website;
        public string email;
        public string id;
        /// <summary>
        /// Parent of current file. When setting parent to a folder, property will call addChild() and handle contents appropriately.
        /// </summary>
        public string ListName { get; set; }

        // TO DO : Add get tags func.
        // TO DO: Add static function to search for a property.
        static DPFile() {
            foreach (var eName in Enum.GetNames(typeof(ContentType)))
            {
                var lowercasedName = eName.ToLower();
                enumPairs[lowercasedName] = (ContentType)Enum.Parse(typeof(ContentType), eName);
            }
        }



        public static ContentType GetContentType(string type, DPFile dP)
        {
            if (!string.IsNullOrEmpty(type) && enumPairs.TryGetValue(type, out ContentType contentType))
            {
                return contentType;
            }
            if (dP is null) return ContentType.DAZ_File;
            if (GeometryFormats.Contains(dP.Ext))
            {
                return ContentType.Geometry;
            }
            else if (MediaFormats.Contains(dP.Ext))
            {
                return ContentType.Media;
            }
            else if (DocumentFormats.Contains(dP.Ext))
            {
                return ContentType.Document;
            }
            else if (OtherFormats.Contains(dP.Ext))
            {
                return ContentType.Program;
            }
            else if (DAZFormats.Contains(dP.Ext))
            {
                return ContentType.DAZ_File;
            }

            // The most obvious comment ever - implied else :\
            return ContentType.Unknown;
        }

        public static string ParseJsonValue(string jsonString, string propertyName)
        {
            // Substring via propertyName length + 6.
            var startSearchIndex = jsonString.IndexOf('"', jsonString.IndexOf(":"));
            var lastQuoteIndex = jsonString.LastIndexOf('"');
            if (startSearchIndex == -1 || lastQuoteIndex == startSearchIndex) return string.Empty;
            var propertyValue = jsonString.Substring(startSearchIndex + 1, lastQuoteIndex - startSearchIndex - 1);

            return propertyValue;

        }

        // Not accurate but it's okay.
        public static string GetPropertyName(string msg) => msg.Remove(msg.IndexOf(':')).Trim('"').TrimStart();
        public void UpdateContentInfo(string[] contents)
        {
            foreach (var line in contents[1..9])
            {
                if (line is null) continue;
                try
                {
                    var propertyName = GetPropertyName(line);
                    if (propertyName.Contains("id")) id = ParseJsonValue(line, "id");
                    else if (propertyName.Contains("type")) contentType = GetContentType(ParseJsonValue(line, "type"), this);
                    else if (propertyName.Contains("author")) author = ParseJsonValue(line, "author");
                    else if (propertyName.Contains("email")) email = ParseJsonValue(line, "email");
                    else if (propertyName.Contains("website")) website = ParseJsonValue(line, "website");
                }
                catch (Exception e)
                {
                    DPCommon.WriteToLog($"Failed to add metadata for file. REASON: {e}");
                }
            }
        }

        public static bool ValidImportExtension(string ext)
        {
            return ArrayHelper.Contains(AcceptableImportFormats, ext);
        }
        public DPFile()
        {

        }

        public DPFile(string _path, DPFolder __parent)
        {
            UID = DPIDManager.GetNewID();
            WillExtract = true;
            Path = _path;
            Parent = __parent;
            if (Path != null | Path != "")
            {
                // _ext can have length of 0, ex: LICENSE
                var _ext = System.IO.Path.GetExtension(Path);
                Ext = _ext.Length != 0 ? _ext.Substring(1) : string.Empty;
            }
            ListName = DPProcessor.workingArchive.FileName + '\\' + Path;
            DPFiles.TryAdd(Path, this);
            DPProcessor.workingArchive.Contents.Add(this);
        }
        ~DPFile()
        {
            DPFiles.Remove(Path);
        }

        public async void QuickReadFileAsync()
        {
            // TO DO: Use GZIP file header check.
            var workingPath = DestinationPath != null ? DestinationPath : ExtractedPath;
            try
            {
                using (StreamReader inputFile = new StreamReader(workingPath))
                {
                    // Get first 10 lines.
                    var tenLines = new string[10];
                    for (var i = 0; i < 10; i++)
                    {
                        tenLines[i] = await inputFile.ReadLineAsync();
                    }
                    UpdateContentInfo(tenLines);
                    inputFile.Close();
                    inputFile.Dispose();
                }
            }
            catch (Exception e)
            {
                DPCommon.WriteToLog(e);
                // Try GZIP method.

                try
                {
                    using (GZipStream stream = new GZipStream(new FileStream(workingPath, FileMode.Open), CompressionMode.Decompress))
                    {

                        using (StreamReader gInputFile = new StreamReader(stream))
                        {
                            // Get first 10 lines.
                            var tenLines = new string[10];
                            for (var i = 0; i < 10; i++)
                            {
                                tenLines[i] = await gInputFile.ReadLineAsync();
                            }
                            gInputFile.Dispose();
                            UpdateContentInfo(tenLines);
                        }
                        stream.Close();
                        stream.Dispose();
                    }
                    // Parse data to JSON.

                }
                catch (Exception f)
                {
                    DPCommon.WriteToLog("GZip method failed.");
                    DPCommon.WriteToLog(f);
                    ///errored = true;
                }
            }
        }

        public bool IsReadable()
        {
            var extractPathExists = !WasExtracted && File.Exists(ExtractedPath);
            var destinationPathExists = WasExtracted && File.Exists(DestinationPath);
            var isDazFile = DAZFormats.Contains(Ext);
            if (!isDazFile) return false;
            var canRead = false;
            try
            {
                if (extractPathExists)
                    File.Open(ExtractedPath, FileMode.Open, FileAccess.Read).Dispose();
                else if (destinationPathExists)
                    File.Open(DestinationPath, FileMode.Open, FileAccess.Read).Dispose();
                else return false;
                canRead = true;
            }
            catch
            {
                DPCommon.WriteToLog("not readable");
                return false;
            }
            return canRead;
        }

        public static bool FindFileInDPFiles(string path, out DPFile file)
        {
            if (DPFiles.TryGetValue(path, out file)) return true;

            file = null;
            return false;
        }

    }

}
