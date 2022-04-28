// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace DAZ_Installer.DP
{
    // ZIP Transversal Check
    /*
        destFileName = Path.GetFullPath(Path.Combine(destDirectory, entry.Key));
        string fullDestDirPath = Path.GetFullPath(destDirectory + Path.DirectorySeparatorChar);
        if (!destFileName.StartsWith(fullDestDirPath)) {
            throw new ExtractionException("Entry is outside of the target dir: " + destFileName);
        }
    */
    // TO DO: Add tag property.

    internal enum ArchiveType
    {
        Product, Bundle, Unknown
    }
    internal class DPArchive : IDPWorkingFile
    {
        public string path { get; set; }
        public string relativePath { get; set; }
        public string destinationPath { get; set; }
        public string ext { get; set; }
        public bool extract { get; set; } = false;
        public uint uid { get; set; }
        public string extractedPath { get; set; }
        public string hierachyName { get; set; }
        public string fileName { get; set; }
        public string ListName { get; set; }
        public uint fileCount { get; set; }
        public bool wasExtracted { get; set; } = false;
        public List<IDPWorkingFile> contents { get; } = new List<IDPWorkingFile>();
        /// <summary>
        /// All of the folders in the archive (includes subfolders).
        /// </summary>
        public Dictionary<string, DPFolder> folders { get; } = new Dictionary<string, DPFolder>();
        public List<IDPWorkingFile> rootContents { get; } = new List<IDPWorkingFile>();
        public List<DPFolder> rootFolders { get; } = new List<DPFolder>();
        public bool passwordFailed = false;
        public bool cancelledOperation = false;
        public bool secondPasswordPromptHasSeen = false;
        internal DPFile manifestFile { get; set; }
        internal DPFile supplementFile { get; set; }
        internal string imageDownloadLink = "";
        public Control[] progressCombo;
        internal List<string> erroredFiles { get; } = new List<string>();
        internal ArchiveType type { get; set; }
        internal bool isInnerArchive { get; set; } = false;
        internal DPArchive rootArchive { get; set; } = null;
        private List<string> lastVolumes { get; } = new List<string>();
        internal string productName { get; set; }
        internal bool errored { get; set; } = false;
        internal string[] tags { get; set; }
        private Dictionary<string, string> volumePairs = new Dictionary<string, string>(); // First key is the OLD nonworking one, Second key is the working one.
        protected char[] password;
        internal List<DPArchive> internalArchives { get; init; } = new List<DPArchive>();
        private DPArchive self;
        protected char internalDictSeperator = '\\';
        /// <summary>
        /// Parent of folder. Handles child parent relations.
        /// </summary>
        public DPFolder parent
        {
            get => _parent;
            set
            {
                if (!isInnerArchive) { _parent = null; return; }
                // Only applies for inner archives.
                // If we were null, but now we're not...
                if (_parent == null && value != null)
                {
                    // Remove ourselves from root content.
                    try
                    {
                        DPProcessor.workingArchive.rootContents.Remove(this);
                    }
                    catch { }

                    // Call the DPFolder's addchild function to add ourselves to the children list.
                    var s = (IDPWorkingFile)this;
                    value.addChild(ref s);

                    _parent = value;
                }
                else if (_parent == null && value == null)
                {
                    // Find parent.
                    var s = (IDPWorkingFile)this;
                    var potParent = DPProcessor.workingArchive.FindParent(ref s);
                    if (potParent != null)
                    {
                        parent = potParent; // Recursion will handle _parent setting.
                        // Goes to first if.
                    }
                    else
                    {
                        potParent = DPFolder.CreateFolderForFile(path);
                        if (potParent != null) parent = potParent; // Recursion will handle _parent setting.
                        // Goes to first if.
                        else
                        {
                            _parent = null;
                            if (!DPProcessor.workingArchive.rootContents.Contains(s))
                            {
                                DPProcessor.workingArchive.rootContents.Add(s);
                            }
                        }
                    }
                }
                else if (_parent != null && value != null)
                {
                    // Remove ourselves from previous parent children.
                    var s = (IDPWorkingFile)this;
                    _parent.removeChild(ref s);

                    // Add ourselves to new parent children.
                    value.addChild(ref s);

                    _parent = value;
                }
                else if (_parent != null && value == null)
                {
                    // Remove ourselves from previous parent children.
                    var s = (IDPWorkingFile)this;
                    _parent.removeChild(ref s);

                    DPProcessor.workingArchive.rootContents.Add(this);
                    _parent = value;
                }
            }
        }
        public DPFolder _parent { get; set; }
        public ListViewItem associatedListItem { get; set; }
        public TreeNode associatedTreeNode { get; set; }
        internal static Dictionary<string, DPArchive> DPArchives { get; } = new Dictionary<string, DPArchive>();

        public DPArchive(string _path, string relativePathBase = null, bool innerArchive = false)
        {
            self = this;
            uid = DPIDManager.GetNewID();
            DPGlobal.dpObjects.Add(uid, self);
            isInnerArchive = innerArchive; // Order matters.
            // Make a file but we don't want to check anything.
            path = _path;
            parent = null;


            if (path != null || path != string.Empty)
            {
                fileName = Path.GetFileName(path);
            }
            if (relativePathBase != null)
            {
                relativePath = Path.GetRelativePath(relativePathBase, path);
            }
            if (DPProcessor.workingArchive != this && DPProcessor.workingArchive != null)
            {
                ListName = DPProcessor.workingArchive.fileName + '\\' + path;
            }
            ext = Path.GetExtension(path).Substring(1).ToLower();
            hierachyName = Path.GetFileName(path);
            productName = Path.GetFileNameWithoutExtension(path);

            if (isInnerArchive)
                DPProcessor.workingArchive.contents.Add(this);

            DPArchives.Add(path, this);
        }

        internal void QuickAnalyzeFiles()
        {
            foreach (var content in contents)
            {
                if (content.GetType() == typeof(DPFile))
                {
                    var file = (DPFile)content;
                    if (file.IsReadable()) file.QuickReadFileAsync();
                }
            }
        }

        ~DPArchive()
        {
            DPIDManager.RemoveID(uid);
            DPArchives.Remove(path);
        }

        /// <summary>
        ///  Checks whether or not the given ext is what is expected. Checks file headers.
        /// </summary>
        /// <returns>Returns an extension of the appropriate archive extraction method. Otherwise, null.</returns>
        private string CheckArchiveAuthencity()
        {
            FileStream stream;
            // Open file.
            if (isInnerArchive) stream = File.OpenRead(extractedPath);
            else stream = File.OpenRead(path);

            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            stream.Close();
            // ZIP File Header
            // 	50 4B OR 	57 69
            if ((bytes[0] == 80 || bytes[0] == 87) && (bytes[1] == 75 || bytes[2] == 105))
            {
                return "zip";
            }
            // RAR 5 consists of 8 bytes.  0x52 0x61 0x72 0x21 0x1A 0x07 0x01 0x00
            // RAR 4.x consists of 7. 0x52 0x61 0x72 0x21 0x1A 0x07 0x00
            // Rar!
            if (bytes[0] == 82 && bytes[1] == 97 && bytes[2] == 114 && bytes[3] == 33)
            {
                return "rar";
            }

            if (bytes[0] == 55 && bytes[1] == 122 && bytes[2] == 188 && bytes[3] == 175)
            {
                return "7z";
            }
            return string.Empty;
        }
        public void Extract()
        {
            var detectedExt = CheckArchiveAuthencity();
            if (detectedExt == string.Empty)
            {
                if (ext == "rar")
                {
                    DPProcessor.ProcessRAR(ref self);
                }
                else if (ext == "zip")
                {
                    DPProcessor.ProcessZIP(ref self);
                }
                else if (ext == "7z")
                {
                    DPProcessor.Process7Z(ref self);
                }
            }
            else
            {
                if (detectedExt == "rar")
                {
                    DPProcessor.ProcessRAR(ref self);
                }
                else if (detectedExt == "zip")
                {
                    DPProcessor.ProcessZIP(ref self);
                }
                else if (detectedExt == "7z")
                {
                    DPProcessor.Process7Z(ref self);
                }
            }
        }

        /// <summary>
        /// Updates the extracted path for each IDPWorkingFile. Should be called after all files are extracted.
        /// </summary>
        internal void UpdateFilePaths()
        {
            foreach (var content in contents)
            {
                content.extractedPath = Path.Combine(DPProcessor.TEMP_LOCATION, Path.GetFileNameWithoutExtension(path), content.path);
            }
        }

        public void GetTags()
        {
            // First is always author.
            // Next is folder names.
            var author = "";
            var id = "";
            var fileNames = new HashSet<string>(contents.Count);
            var folderNames = new HashSet<string>(folders.Count);
            foreach (var content in contents)
            {
                var fileNameWOExt = Path.GetFileNameWithoutExtension(content.path);
                fileNames.Add(fileNameWOExt);
                if (content.GetType() != typeof(DPFile)) continue;
                var dpfile = (DPFile)content;
                if (author == string.Empty)
                {
                    if (!string.IsNullOrEmpty(dpfile.author)) author = dpfile.author;
                }
                if (id != string.Empty)
                {
                    if (!string.IsNullOrEmpty(dpfile.id)) id = dpfile.id;
                }
            }
            foreach (var folder in folders.Values)
            {
                folderNames.Add(folder.relativePath);
            }
            var tagsArray = new HashSet<string>(fileNames.Count + folderNames.Count + 2);
            if (!string.IsNullOrEmpty(author))
            {
                tagsArray.Add(author);
                tagsArray.UnionWith(folderNames);
                tagsArray.UnionWith(fileNames);
                if (!string.IsNullOrEmpty(id)) tagsArray.Add(id);
            }
            else
            {
                tagsArray.UnionWith(folderNames);
                tagsArray.UnionWith(fileNames);
                if (!string.IsNullOrEmpty(id)) tagsArray.Add(id);
            }
            tags = tagsArray.ToArray();

        }
        public DPFolder FindParent(ref IDPWorkingFile obj)
        {
            var fileName = PathHelper.GetFileName(obj.path);
            if (fileName == string.Empty) fileName = Path.GetFileName(obj.path.TrimEnd(PathHelper.GetSeperator(obj.path)));
            string relativePathOnly = "";
            try
            {
                relativePathOnly = PathHelper.GetAbsoluteUpPath(obj.path.Remove(obj.path.LastIndexOf(fileName)));
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
            foreach (var path in folders.Keys)
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

            foreach (var _folder in folders.Values)
            {
                if (_folder.path == relativePath || _folder.path == PathHelper.SwitchSeperators(relativePath))
                {
                    folder = _folder;
                    return true;
                }
            }
            folder = null;
            return false;
        }

        /// <summary>
        /// This function should be called after all the files have been extracted. If no content folders have been found, this is a bundle.
        /// </summary>
        internal ArchiveType DetermineArchiveType()
        {
            foreach (var folder in folders.Values)
            {
                if (folder.isContentFolder)
                {
                    return ArchiveType.Product;
                }
            }
            foreach (var content in contents)
            {
                if (content.GetType() == typeof(DPArchive)) return ArchiveType.Bundle;
            }
            return ArchiveType.Unknown;
        }

        public void ConnectVolumeDir(string dirPath)
        {
            lastVolumes.Add(Path.GetFileName(dirPath));
            hierachyName = fileName + " (";
            foreach (var volume in lastVolumes)
            {
                hierachyName += $"{volume}/";
            }
            hierachyName = hierachyName.TrimEnd('/') + ')';
        }

        public void SetPassword(string pass)
        {
            password = pass.ToCharArray();
        }

        public string GetPassword()
        {
            return new string(password);
        }

        public void AddVolumePair(string expectedVolume, string rightVolume)
        {
            volumePairs.Add(expectedVolume, rightVolume);
        }

        public string GetRightVolume(string expectedVolume)
        {
            if (volumePairs.TryGetValue(expectedVolume, out string rightVolume))
            {
                return rightVolume;
            }
            return null;
        }
        public IDPWorkingFile FindFileViaName(string name)
        {
            foreach (var file in contents)
            {
                if (file.path.Contains(name)) return file;
            }
            return null;
        }

        internal DPProductRecord CreateRecords()
        {
            var tuple = ConfirmFilesExtraction();
            string[] foundFiles = tuple.Item1;
            string[] missingFiles = tuple.Item2;
            string imageLocation = string.Empty;
            var workingExtractionRecord = 
                new DPExtractionRecord(Path.GetFileName(fileName), DPSettings.destinationPath, foundFiles, erroredFiles.ToArray(), 
                null, ConvertDPFoldersToStringArr(folders), 0);

            if (type != ArchiveType.Bundle)
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
                var workingProductRecord = new DPProductRecord(productName, tags, tags[0], 
                                            null, DateTime.Now, imageLocation, 0, 0);
                DPDatabase.AddNewRecordEntry(workingProductRecord, workingExtractionRecord);
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
        /// Finds files that were supposedly extracted to disk.
        /// </summary>
        /// <returns>An tuple where the first item are the found files, 
        /// and the second are missing files</returns>
        private Tuple<string[], string[]> ConfirmFilesExtraction()
        {
            List<string> foundFiles = new List<string>((int) fileCount);
            List<string> missingFiles = new List<string>();
            foreach (var file in contents)
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
                if (erroredFiles.Contains(missingFiles[i]))
                {
                    missingFiles.RemoveAt(i);
                }
            }

            return new (foundFiles.ToArray(), missingFiles.ToArray());
        }

        // Delete?
        internal void FinalizeFolderStructure()
        {
            rootFolders.Clear();
            foreach (var folder in folders.Values)
            {
                folder.parent = null;
                folder.subfolders.Clear();
            }
            foreach (var folder in folders.Values)
            {
                //folder.parent = null;
                // Find all folders that contain path.
                var childFolders = DPFolder.FindChildFolders(folder.path, folder);

                // Now appropriately add child folders.
                foreach (var child in childFolders)
                {
                    child.parent = folder;
                    folder.subfolders.Add(child);
                }

                // Now find parent for this folder.
                var idp = (IDPWorkingFile)folder;
                folder.parent = FindParent(ref idp);
            }
            DPCommon.WriteToLog(rootFolders);
        }

        internal static bool FindArchiveViaName(string path, out DPArchive archive)
        {
            if (DPArchives.TryGetValue(path, out archive)) return true;

            archive = null;
            return false;
        }
    }
}
