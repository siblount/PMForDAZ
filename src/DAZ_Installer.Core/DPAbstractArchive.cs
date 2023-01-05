// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using IOPath = System.IO.Path;
using System.IO;
using System;
using DAZ_Installer.Core.Utilities;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// Defines the archive type of an archive.
    /// </summary>
    // TODO: Add a type for Multi-Product and Multi-Bundle to help determine whether
    // an archive should be added to the database/library
    public enum ArchiveType
    {
        Product, Bundle, Unknown
    }

    /// <summary>
    /// Defines the archive format of an archive.
    /// </summary>
    public enum ArchiveFormat {
        SevenZ, WinZip, RAR, Unknown
    }
    /// <summary>
    /// Abstract class for all supported archive files. 
    /// Currently the supported archive files are RAR, WinZip, and 7z (partially).
    /// </summary>
    public abstract class DPAbstractArchive : DPAbstractFile {
        /// <summary>
        /// The current mode of the archive file; describes whether the archie is 
        /// peeking (seeking files) or extracting (seeking and extracting files).
        /// </summary>
        protected enum Mode {
            Peek, Extract
        }
        /// <summary>
        /// The name that will be used for the hierachy. Equivalent to Path.GetFileName(Path);
        /// </summary>
        /// <value>The file name of the <c>Path</c> of this archive with the extension.</value>
        public string HierachyName { get; set; }
        /// <summary>
        /// The name that will be used for the list view. The list name is the working archive's <c>FileName</c> + $"\\{Path}".
        /// </summary>
        /// <value>The working archive's FileName + $"\\{Path}".</value>
        public string ListName { get; set; }
        /// <summary>
        /// A list of archives that are children of this archive.
        /// </summary>
        public List<DPAbstractArchive> publicArchives { get; init; } = new List<DPAbstractArchive>();
        /// <summary>
        /// The archive that is the parent of this archive. This can be null.
        /// </summary>
        public DPAbstractArchive? ParentArchive { get; set; }
        /// <summary>
        /// A file that has been detected as a manifest file. This can be null.
        /// </summary>
        public DPDSXFile? ManifestFile { get; set; }
        /// <summary>
        /// A file that has been detected as a supplement file. This can be null.
        /// </summary>
        public DPDSXFile? SupplementFile { get; set; }
        /// <summary>
        /// A boolean value to describe if this archive is a child of another archive. Default is false.
        /// </summary>
        public bool IsInnerArchive { get; set; } = false;
        /// <summary>
        /// The type of this archive. Default is <c>ArchiveType.Unknown</c>.
        /// </summary>
        public ArchiveType Type { get; set; } = ArchiveType.Unknown;
        /// <summary>
        /// A list of files that errored during the extraction stage.
        /// </summary>
        public LinkedList<string> ErroredFiles { get; set; } = new LinkedList<string>();
        /// <summary>
        /// The product info connected to this archive.
        /// </summary>
        public DPProductInfo ProductInfo = new DPProductInfo();

        /// <summary>
        /// A map of all of the folders parented to this archive.
        /// </summary>
        /// <typeparam name="string">The `Path` of the Folder.</typeparam>
        /// <typeparam name="DPFolder">The folder.</typeparam>

        public Dictionary<string, DPFolder> Folders { get; } = new Dictionary<string, DPFolder>();

        /// <summary>
        /// A list of folders at the root level of this archive.
        /// </summary>
        public List<DPFolder> RootFolders { get; } = new List<DPFolder>();
        /// <summary>
        /// A list of all of the contents (DPAbstractFiles) in this archive.
        /// </summary>
        /// <typeparam name="DPAbstractFile">The file content in this archive.</typeparam>
        public List<DPAbstractFile> Contents { get; } = new List<DPAbstractFile>();
        
        /// <summary>
        /// A list of the root contents/ the contents at root level (DPAbstractFiles) of this archive.
        /// </summary>
        /// <typeparam name="DPAbstractFile">The file content in this archive.</typeparam>
        public List<DPAbstractFile> RootContents { get; } = new List<DPAbstractFile>();
        /// <summary>
        /// A list of all .dsx files in this archive.
        /// </summary>
        /// <typeparam name="DPDSXFile">A file that is either a manifest, supplementary, or support file (.dsx).</typeparam>
        public List<DPDSXFile> DSXFiles { get; } = new List<DPDSXFile>();
        /// <summary>
        /// A list of all readable daz files in this archive. This consists of types with extension: .duf, .dsf.
        /// </summary>
        /// <typeparam name="DPDazFile">A file with the extension .duf OR .dsf.</typeparam>
        /// <returns></returns>
        public List<DPDazFile> DazFiles { get; } = new List<DPDazFile>();

        /// <summary>
        /// A boolean to determine if the processor can read the contents of the archive without extracting to disk.
        /// </summary>
        public virtual bool CanReadWithoutExtracting { get; init; } = false;
        /// <summary>
        /// The true uncompressed size of the archive contents in bytes.
        /// </summary>
        public ulong TrueArchiveSize { get; set; } = 0;
        /// <summary>
        /// The expected tag count for this archive. This value is updated when an applicable file has discovered new tags.
        /// </summary>
        public uint ExpectedTagCount { get; set; } = 0;

        /// <summary>
        /// The progress combo that is visible on the extraction page. This is typically null when the file is firsted discovered
        /// inside another archive (and therefore before it is processed) and/or after the extraction has completed.
        /// </summary>
        public DPProgressCombo? ProgressCombo { get; set; }

        /// <summary>
        /// Identifies what mode the archive is currently in. The default is Mode.Extract.
        /// </summary>
        protected Mode mode { get; set; } = Mode.Extract;

        /// <summary>
        /// The regex expression used for creating a product name.
        /// </summary>
        /// <returns></returns>
        public static Regex ProductNameRegex = new Regex(@"([^+|\-|_|\s]+)", RegexOptions.Compiled);
        public DPAbstractArchive(string _path, bool innerArchive = false, string? relativePathBase = null) : base(_path)
        {
            IsInnerArchive = innerArchive; // Order matters.
            // Make a file but we don't want to check anything.
            if (IsInnerArchive) Parent = null;
            else _parent = null;
            FileName = IOPath.GetFileName(_path);
            if (relativePathBase != null)
            {
                RelativePath = IOPath.GetRelativePath(relativePathBase, Path);
                RelativePath = RelativePath.Replace(PathHelper.GetSeperator(RelativePath), PathHelper.GetSeperator(Path));
            }
            else RelativePath = FileName;
            if (DPProcessor.workingArchive != this && DPProcessor.workingArchive != null)
            {
                ListName = DPProcessor.workingArchive.FileName + '\\' + Path;
            }
            Ext = GetExtension(Path);
            HierachyName = IOPath.GetFileName(Path);
            ProductInfo = new DPProductInfo(IOPath.GetFileNameWithoutExtension(Path));

            if (IsInnerArchive)
                DPProcessor.workingArchive.Contents.Add(this);
        }
        #region Abstract methods
        /// <summary>
        /// Peeks the archive contents if possible and will extract the archive contents to the destination path. 
        /// </summary>
        public abstract void Extract();

        /// <summary>
        /// Previews the archive by discovering files in this archive.
        /// </summary>
        public abstract void Peek();
        
        /// <summary>
        /// Reads the files listed in <c>DSXFiles</c>. If <c>CanReadWithoutExtracting</c> is true, the file won't be extracted.
        /// Otherwise, the file will be extracted to the <c>TEMP_LOCATION</c> of <c>DPProcessor</c>. 
        /// </summary>
        public abstract void ReadMetaFiles();
        
        /// <summary>
        /// Reads files that have the extension .dsf and .duf after it has been extracted. 
        /// </summary>
        public abstract void ReadContentFiles();
        /// <summary>
        /// Calls the derived archive class to dispose of the file handle.
        /// </summary>
        public abstract void ReleaseArchiveHandles();
        #endregion
        #region public Methods
        /// <summary>
        ///  Checks whether or not the given ext is what is expected. Checks file headers.
        /// </summary>
        /// <returns>Returns an extension of the appropriate archive extraction method. Otherwise, null.</returns>

        public static ArchiveFormat DetermineArchiveFormatPrecise(string location) {
            try
            {
                using FileStream stream = File.OpenRead(location);
                var bytes = new byte[8];
                stream.Read(bytes, 0, 8);
                stream.Close();
                // ZIP File Header
                // 	50 4B OR 	57 69
                if ((bytes[0] == 80 || bytes[0] == 87) && (bytes[1] == 75 || bytes[2] == 105))
                {
                    return ArchiveFormat.WinZip;
                }
                // RAR 5 consists of 8 bytes.  0x52 0x61 0x72 0x21 0x1A 0x07 0x01 0x00
                // RAR 4.x consists of 7. 0x52 0x61 0x72 0x21 0x1A 0x07 0x00
                // Rar!
                if (bytes[0] == 82 && bytes[1] == 97 && bytes[2] == 114 && bytes[3] == 33)
                {
                    return ArchiveFormat.RAR;
                }

                if (bytes[0] == 55 && bytes[1] == 122 && bytes[2] == 188 && bytes[3] == 175)
                {
                    return ArchiveFormat.SevenZ;
                }
                return ArchiveFormat.Unknown;
            } catch { return ArchiveFormat.Unknown; }
        }

        /// <summary>
        /// Returns an enum describing the archive's format based on the file extension.
        /// </summary>
        /// <param name="path">The path of the archive.</param>
        /// <returns>A ArchiveFormat enum determining the archive format.</returns>
        public static ArchiveFormat DetermineArchiveFormat(string ext) {
            // ADDITONAL NOTE: This is called for determing archive files inside of an
            // archive file.
            ext = ext.ToLower();
            switch (ext) {
                case "7z":
                    return ArchiveFormat.SevenZ;
                case "rar":
                    return ArchiveFormat.RAR;
                case "zip":
                    return ArchiveFormat.WinZip;
                default:
                    if (uint.TryParse(ext, out uint _)) return ArchiveFormat.SevenZ;
                    return ArchiveFormat.Unknown;
            }
        }

        public static DPAbstractArchive CreateNewArchive(string fileName, bool innerArchive = false, string? relativePathBase = null) {
            string ext = GetExtension(fileName);
            switch (DetermineArchiveFormat(ext)) {
                case ArchiveFormat.RAR:
                    return new DPRARArchive(fileName, innerArchive, relativePathBase);
                case ArchiveFormat.SevenZ:
                    return new DP7zArchive(fileName, innerArchive, relativePathBase);
                case ArchiveFormat.WinZip:
                    return new DPZipArchive(fileName, innerArchive, relativePathBase);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Finds files that were supposedly extracted to disk.
        /// </summary>
        /// <returns>The file paths of successful extracted files.</returns>
        private string[] GetSuccessfulFiles()
        {
            List<string> foundFiles = new List<string>(Contents.Count);
            foreach (var file in Contents) {
                if (file.WasExtracted) foundFiles.Add(file.Path);
            }
            return foundFiles.ToArray();
        }

        public DPProductRecord CreateRecords()
        {
            string imageLocation = string.Empty;

            // Extraction Record successful folder/file paths will now be relative to their content folder (if any).
            var successfulFiles = new List<string>(Contents.Count);
            // Folders where a file was extracted underneath it.
            // Ex: Content/Documents/a.txt was extracted, therefore "Documents" is added.
            var foldersExtracted = new HashSet<string>(Contents.Count);

            foreach (var file in Contents.Where(f => f.WasExtracted))
            {
                successfulFiles.Add(file.RelativePath ?? file.Path);
                if (file.RelativePath != null)
                {
                    foldersExtracted.Add(IOPath.GetDirectoryName(file.RelativePath));
                }
            }
            var workingExtractionRecord = 
                new DPExtractionRecord(IOPath.GetFileName(FileName), DPProcessor.settingsToUse.destinationPath, successfulFiles.ToArray(), ErroredFiles.ToArray(), 
                null, foldersExtracted.ToArray(), 0);

            if (Type != ArchiveType.Bundle)
            {
                if (DPProcessor.settingsToUse.downloadImages == SettingOptions.Yes)
                {
                    imageLocation = DPNetwork.DownloadImage(workingExtractionRecord.ArchiveFileName);
                }
                else if (DPProcessor.settingsToUse.downloadImages == SettingOptions.Prompt)
                {
                    // TODO: Use more reliable method! Support files!
                    // Pre-check if the archive file name starts with "IM"
                    if (workingExtractionRecord.ArchiveFileName.StartsWith("IM"))
                    {
                        var result = DAZ_Installer.Extract.ExtractPage.DoPromptMessage("Do you wish to download the thumbnail for this product?", "Download Thumbnail Prompt", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes) imageLocation = DPNetwork.DownloadImage(workingExtractionRecord.ArchiveFileName);
                    }
                }
                var author = ProductInfo.Authors.Count != 0 ? ProductInfo.Authors.First() : null;
                var workingProductRecord = new DPProductRecord(ProductInfo.ProductName, ProductInfo.Tags.ToArray(), author, 
                                            null, DateTime.Now, imageLocation, 0, 0);
                DPDatabase.AddNewRecordEntry(workingProductRecord, workingExtractionRecord);
                return workingProductRecord;
            }
            return null;
        }

        public DPAbstractFile? FindFileViaNameContains(string name)
        {
            foreach (var file in Contents)
            {
                if (file.Path.Contains(name)) return file;
            }
            return null;
        }

        private static string[] ConvertDPFoldersToStringArr(Dictionary<string, DPFolder> folders)
        {
            string[] strFolders = new string[folders.Count];
            string[] keys = folders.Keys.ToArray();
            for (var i = 0; i < strFolders.Length; i++)
            {
                strFolders[i] = folders[keys[i]].Path;
            }
            return strFolders;
        }

        /// <summary>
        /// This function should be called after all the files have been extracted. If no content folders have been found, this is a bundle.
        /// </summary>
        public ArchiveType DetermineArchiveType()
        {
            foreach (var folder in Folders.Values)
            {
                if (folder.isContentFolder)
                {
                    return ArchiveType.Product;
                }
            }
            foreach (var content in Contents)
            {
                if (content is DPAbstractArchive) return ArchiveType.Bundle;
            }
            return ArchiveType.Unknown;
        }

        public void GetTags()
        {
            // First is always author.
            // Next is folder names.
            var productNameTokens = SplitProductName();
            ReadContentFiles();
            ReadMetaFiles();
            var tagsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            tagsSet.EnsureCapacity(GetEstimateTagCount() + productNameTokens.Length +
                (Folders.Count * 2) + ((Contents.Count - publicArchives.Count) * 2));
            foreach (var file in DazFiles)
            {
                var contentInfo = file.ContentInfo;
                if (contentInfo.Website.Length != 0) tagsSet.Add(contentInfo.Website);
                if (contentInfo.Email.Length != 0) tagsSet.Add(contentInfo.Email);
                tagsSet.UnionWith(contentInfo.Authors);
            }
            foreach (var content in Contents)
            {
                if (content is DPAbstractArchive) continue;
                tagsSet.UnionWith(IOPath.GetFileNameWithoutExtension(content.Path).Split(' '));
            }
            foreach (var folder in Folders)
            {
                tagsSet.UnionWith(PathHelper.GetFileName(folder.Key).Split(' '));
            }
            tagsSet.UnionWith(ProductInfo.Authors);
            tagsSet.UnionWith(productNameTokens);
            if (ProductInfo.SKU.Length != 0) tagsSet.Add(ProductInfo.SKU);
            if (ProductInfo.ProductName.Length != 0) tagsSet.Add(ProductInfo.ProductName);
            ProductInfo.Tags = tagsSet;
        
        }

        public int GetEstimateTagCount() {
            int count = 0;
            foreach (var content in Contents) {
                if (content is DPFile) {
                    count += ((DPFile) content).Tags.Count;
                }
            }
            count += ProductInfo.Authors.Count;
            return count;
        }

        public DPFolder FindParent(DPAbstractFile obj)
        {
            var fileName = PathHelper.GetFileName(obj.Path);
            if (fileName == string.Empty) fileName = IOPath.GetFileName(obj.Path.TrimEnd(PathHelper.GetSeperator(obj.Path)));
            string relativePathOnly = "";
            try
            {
                relativePathOnly = PathHelper.GetAbsoluteUpPath(obj.Path.Remove(obj.Path.LastIndexOf(fileName)));
            }
            catch { }
            if (RecursivelyFindFolder(relativePathOnly, out DPFolder folder))
            {
                return folder;
            }
            return null;
        }

        public bool FolderExists(string fPath) => Folders.ContainsKey(fPath);

        public bool RecursivelyFindFolder(string relativePath, out DPFolder folder)
        {

            foreach (var _folder in Folders.Values)
            {
                if (_folder.Path == relativePath || _folder.Path == PathHelper.SwitchSeperators(relativePath))
                {
                    folder = _folder;
                    return true;
                }
            }
            folder = null;
            return false;
        }


        public string[] SplitProductName() {
            var matches = ProductNameRegex.Matches(ProductInfo.ProductName);
            List<string> tokens = new List<string>(matches.Count);
            foreach (Match match in matches) {
                tokens.Add(match.Value);
            }
            return tokens.ToArray();
        }
        #endregion
        
    }
}