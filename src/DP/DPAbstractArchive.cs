using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using IOPath = System.IO.Path;
using System.IO;
using System;


namespace DAZ_Installer.DP {

    internal enum ArchiveType
    {
        Product, Bundle, Unknown
    }

    internal enum ArchiveFormat {
        SevenZ, WinZip, RAR, Unknown
    }
    
    internal abstract class DPAbstractArchive : DPAbstractFile {

        public static string TEMP_LOCATION = IOPath.Combine(DPSettings.tempPath, @"DazProductInstaller\");

        protected enum Mode {
            Peek, Extract
        }
        /// <summary>
        /// The name that will be used for the hierachy. Equivalent to Path.GetFileName(Path);
        /// </summary>
        /// <value>The file name of the <c>Path</c> of this archive with the extension.</value>
        internal string HierachyName { get; set; }
        /// <summary>
        /// The file name of this archive. Equivalent to Path.GetFileName(Path);
        /// </summary>
        /// <value>The file name of the <c>Path</c> of this archive with the extension.</value>
        internal string FileName { get; set; }
        /// <summary>
        /// The name that will be used for the list view. The list name is the working archive's <c>FileName</c> + $"\\{Path}".
        /// </summary>
        /// <value>The working archive's FileName + $"\\{Path}".</value>
        internal string ListName { get; set; }
        /// <summary>
        /// A global static dictionary of available archives
        /// </summary>
        /// <typeparam name="string">The name of the archive.</typeparam>
        /// <typeparam name="DPAbstractArchive">The archive.</typeparam>
        internal static Dictionary<string, DPAbstractArchive> Archives { get; } = new Dictionary<string, DPAbstractArchive>(); 
        /// <summary>
        /// A list of archives that are children of this archive.
        /// </summary>
        internal List<DPAbstractArchive> InternalArchives { get; init; } = new List<DPAbstractArchive>();
        /// <summary>
        /// The archive that is the parent of this archive. This can be null.
        /// </summary>
        internal DPAbstractArchive? ParentArchive { get; set; }
        /// <summary>
        /// A file that has been detected as a manifest file. This can be null.
        /// </summary>
        internal DPFile? ManifestFile { get; set; }
        /// <summary>
        /// A file that has been detected as a supplement file. This can be null.
        /// </summary>
        internal DPFile? SupplementFile { get; set; }
        /// <summary>
        /// A boolean value to describe if this archive is a child of another archive. Default is false.
        /// </summary>
        internal bool IsInnerArchive { get; set; } = false;
        /// <summary>
        /// The type of this archive. Default is <c>ArchiveType.Unknown</c>.
        /// </summary>
        internal ArchiveType Type { get; set; } = ArchiveType.Unknown;
        /// <summary>
        /// A list of files that errored during the extraction stage.
        /// </summary>
        internal LinkedList<string> ErroredFiles { get; set; } = new LinkedList<string>();
        /// <summary>
        /// The product info connected to this archive.
        /// </summary>
        internal DPProductInfo ProductInfo { get; init; } = new DPProductInfo();

        /// <summary>
        /// A list of all of the folders parented to this archive.
        /// </summary>

        public Dictionary<string, DPFolder> Folders { get; } = new Dictionary<string, DPFolder>();

        /// <summary>
        /// A list of folders at the root level of this archive.
        /// </summary>
        public List<DPFolder> RootFolders { get; } = new List<DPFolder>();
        /// <summary>
        /// A map of all of the contents (DPAbstractFiles) in this archive.
        /// </summary>
        /// <typeparam name="string">The file name from the extract method.</typeparam>
        /// <typeparam name="DPAbstractFile">The file content in this archive.</typeparam>
        /// <returns></returns>
        public Dictionary<string, DPAbstractFile> Contents { get; } = new Dictionary<string, DPAbstractFile>();
        
        /// <summary>
        /// A map of the root contents/ the contents at root level (DPAbstractFiles) of this archive.
        /// </summary>
        /// <typeparam name="string">The file name from the extract method.</typeparam>
        /// <typeparam name="DPAbstractFile">The file content in this archive.</typeparam>
        /// <returns></returns>
        public Dictionary<string, DPAbstractFile> RootContents { get; } = new Dictionary<string, DPAbstractFile>();
        /// <summary>
        /// A boolean to determine if the processor can read the contents of the archive without extracting to disk.
        /// </summary>
        internal virtual bool CanPeek { get; init; } = false;
        internal uint FileCount { get; set; } = 0;

        /// <summary>
        /// The progress combo that is visible on the extraction page. This is typically null when the file is firsted discovered
        /// inside another archive (and therefore before it is processed) and/or after the extraction has completed.
        /// </summary>
        internal DPProgressCombo? ProgressCombo { get; set; }

        protected Mode mode { get; set; } = Mode.Extract;

        private static Regex productNameRegex = new Regex(@"(\w+)", RegexOptions.Compiled);

        /// <summary>
        /// Peeks the archive contents if possible and will extract the archive contents to the destination path. 
        /// </summary>
        internal abstract void Extract();

        /// <summary>
        /// If the archive is able to be peeked without extracting to disk, this function will update the archive's
        /// properties. Otherwise, no operation will be done.
        /// </summary>
        internal abstract void Peek();
        
        /// <summary>
        /// This function updates the 
        /// </summary>
        internal abstract void UpdateData();
        
        /// <summary>
        ///  Checks whether or not the given ext is what is expected. Checks file headers.
        /// </summary>
        /// <returns>Returns an extension of the appropriate archive extraction method. Otherwise, null.</returns>
        
        internal static ArchiveFormat CheckArchiveLegitmacy(DPAbstractArchive archive) {
            FileStream stream;
            // Open file.
            if (archive.IsInnerArchive) stream = File.OpenRead(archive.ExtractedPath);
            else stream = File.OpenRead(archive.Path);
            
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
        }

        /// <summary>
        /// Returns an enum describing the archive's format based on the file extension.
        /// </summary>
        /// <param name="path">The path of the archive.</param>
        /// <returns>A ArchiveFormat enum determining the archive format.</returns>
        internal static ArchiveFormat DetermineArchiveFormat(string ext) {
            ext = ext.ToLower();
            switch (ext) {
                case "7z":
                    return ArchiveFormat.SevenZ;
                case "rar":
                    return ArchiveFormat.RAR;
                case "zip":
                    return ArchiveFormat.WinZip;
                default:
                    return ArchiveFormat.Unknown;
            }
        }

        internal static DPAbstractArchive CreateNewArchive(string fileName, bool innerArchive = false, string? relativePathBase = null) {
            string ext = IOPath.GetExtension(fileName);
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
            foreach (var file in Contents.Values) {
                if (file.WasExtracted) foundFiles.Add(file.Path);
            }
            return foundFiles.ToArray();
        }

        internal static bool FindArchiveViaName(string path, out DPAbstractArchive archive)
        {
            if (Archives.TryGetValue(path, out archive)) return true;

            archive = null;
            return false;
        }


        internal DPProductRecord CreateRecords()
        {
            string imageLocation = string.Empty;
            var workingExtractionRecord = 
                new DPExtractionRecord(System.IO.Path.GetFileName(FileName), DPSettings.destinationPath, GetSuccessfulFiles(), ErroredFiles.ToArray(), 
                null, ConvertDPFoldersToStringArr(Folders), 0);

            if (Type != ArchiveType.Bundle)
            {
                if (DPSettings.downloadImages == SettingOptions.Yes)
                {
                    imageLocation = DPNetwork.DownloadImage(workingExtractionRecord.ArchiveFileName);
                }
                else if (DPSettings.downloadImages == SettingOptions.Prompt)
                {
                    // TODO: Use more reliable method! Support files!
                    // Pre-check if the archive file name starts with "IM"
                    if (workingExtractionRecord.ArchiveFileName.StartsWith("IM"))
                    {
                        var result = extractControl.extractPage.DoPromptMessage("Do you wish to download the thumbnail for this product?", "Download Thumbnail Prompt", MessageBoxButtons.YesNo);
                        if (result == DialogResult.Yes) imageLocation = DPNetwork.DownloadImage(workingExtractionRecord.ArchiveFileName);
                    }
                }
                var workingProductRecord = new DPProductRecord(ProductInfo.ProductName, ProductInfo.Tags.ToArray(), ProductInfo.Author, 
                                            null, DateTime.Now, imageLocation, 0, 0);
                DPDatabase.AddNewRecordEntry(workingProductRecord, workingExtractionRecord);
                return workingProductRecord;
            }
            return null;
        }

        [Obsolete("Will be updated soon.")]
        internal DPAbstractFile? FindFileViaName(string name)
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
        internal ArchiveType DetermineArchiveType()
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
            var fileNames = new HashSet<string>(Contents.Count);
            var folderNames = new HashSet<string>(Folders.Count);

            foreach (var content in Contents.Values)
            {
                var fileNameWOExt = IOPath.GetFileNameWithoutExtension(content.Path);
                fileNames.Add(fileNameWOExt);
                if (content is DPAbstractArchive) continue;

                var dpfile = (DPFile) content;
                if (ProductInfo.Author.Length == 0)
                {
                    if (!string.IsNullOrEmpty(dpfile.author)) ProductInfo.Author = dpfile.author;
                }
                if (ProductInfo.SKU.Length == 0)
                {
                    if (!string.IsNullOrEmpty(dpfile.id)) ProductInfo.SKU = dpfile.id;
                }
            }
            foreach (var folder in Folders.Values)
            {
                folderNames.Add(folder.RelativePath);
            }
            var tagsArray = new HashSet<string>(fileNames.Count + folderNames.Count + 3);

            if (ProductInfo.Author.Length != 0) tagsArray.Add(ProductInfo.Author);
            tagsArray.UnionWith(folderNames);
            tagsArray.UnionWith(fileNames);
            if (ProductInfo.SKU.Length != 0) tagsArray.Add(ProductInfo.SKU);
            }

            ProductInfo.Tags = tagsArray;

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

        public bool FolderExists(string fPath)
        {
            foreach (var path in Folders.Keys)
            {
                if (path == fPath)
                {
                    return true;
                }
            }
            return false;
        }

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


        internal string[] SplitProductName() {
            var matches = productNameRegex.Matches(ProductInfo.ProductName);
            List<string> tokens = new List<string>(matches.Count);
            foreach (Match match in matches) {
                tokens.Add(match.Value);
            }
            return tokens.ToArray();
        }

        internal void QuickAnalyzeFiles()
        {
            foreach (var content in Contents)
            {
                if (content.GetType() == typeof(DPFile))
                {
                    var file = (DPFile)content;
                    if (file.IsReadable()) file.QuickReadFileAsync();
                }
            }
        }

        internal DPAbstractArchive(string _path, bool innerArchive = false, string? relativePathBase = null)
        {
            UID = DPIDManager.GetNewID();
            IsInnerArchive = innerArchive; // Order matters.
            // Make a file but we don't want to check anything.
            Path = _path;
            Parent = null;

            if (Path != null || Path != string.Empty)
            {
                FileName = IOPath.GetFileName(Path);
            }
            if (relativePathBase != null)
            {
                RelativePath = IOPath.GetRelativePath(relativePathBase, Path);
            }
            if (DPProcessor.workingArchive != this && DPProcessor.workingArchive != null)
            {
                ListName = DPProcessor.workingArchive.FileName + '\\' + Path;
            }
            Ext = IOPath.GetExtension(Path).Substring(1).ToLower();
            HierachyName = IOPath.GetFileName(Path);
            ProductInfo = new DPProductInfo(IOPath.GetFileNameWithoutExtension(Path));

            if (IsInnerArchive)
                DPProcessor.workingArchive.Contents.Add(this);

            Archives.Add(Path, this);
        }

        ~DPAbstractArchive()
        {
            Archives.Remove(Path ??= string.Empty);
        }
    }
}