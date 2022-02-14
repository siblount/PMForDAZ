// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DAZ_Installer
{
    public class DPFolder : IDPWorkingFile
    {
        
        public string path { get; set; }
        public string relativePath { get; set; }
        public string destinationPath { get; set; }
        public string ext { get; set; }
        public bool extract { get; set; }
        public string extractedPath { get; set; }
        public uint uid { get; set; }
        public DPFolder parent 
        {   get => _parent;
            set
            {
                // If we were null, but now we're not...
                if (_parent == null && value != null)
                {
                    // Remove ourselves from root folders.
                    try
                    {
                        DPProcessor.workingArchive.rootFolders.Remove(this);
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
                        potParent = CreateFolderForFile(path);
                        if (potParent != null) parent = potParent; // Recursion will handle _parent setting.
                        // Goes to first if.
                        else
                        {
                            _parent = null;
                            if (!DPProcessor.workingArchive.rootFolders.Contains(this))
                            {
                                DPProcessor.workingArchive.rootFolders.Add(this);
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
                } else if (_parent != null && value == null)
                {
                    // Remove ourselves from previous parent children.
                    var s = (IDPWorkingFile)this;
                    _parent.removeChild(ref s);

                    DPProcessor.workingArchive.rootFolders.Add(this);
                    _parent = value;

                }
            }
            //set
            //{
            //    try
            //    {
            //        if (value == null)
            //        {
            //            if (!DPProcessor.workingArchive.rootFolders.Contains(this))
            //                DPProcessor.workingArchive.rootFolders.Add(this);
            //        }
            //        else DPProcessor.workingArchive.rootFolders.Remove(this);
            //    }
            //    catch { }
            //    _parent = value;
            //    //DPProcessor.workingArchive.folders.Remove(path);
            //}
        }
        public bool wasExtracted { get; set; } = false;
        public ListViewItem associatedListItem { get; set; }
        public TreeNode associatedTreeNode { get; set; }


        public List<DPFolder> subfolders = new List<DPFolder>();
        private Dictionary<string, IDPWorkingFile> children = new Dictionary<string, IDPWorkingFile>();
        private DPFolder _parent { get; set; }
        internal bool isContentFolder 
        {
            get => _isContentFolder;
            set
            {
                if ((_isContentFolder == true && value == false )|| (_isContentFolder == false && value == false))
                {
                    // Make children's isPartOfContentFolder false.
                    foreach (var subfolder in subfolders)
                    {
                        subfolder.isPartOfContentFolder = false;
                    }
                } else if ((_isContentFolder == false && value == true) || (_isContentFolder == true && value == true))
                {
                    // Make children's isPartOfContentFolder true.
                    foreach (var subfolder in subfolders)
                    {
                        subfolder.isPartOfContentFolder = true;
                    }
                }
                _isContentFolder = value;
            }
        }
        /// <summary>
        ///  Determined later in ProcessArchive().
        /// </summary>
        internal bool isPartOfContentFolder {
            get {
                if (_isPartOfContentFolder == false)
                {
                    var isPart = (parent != null && parent.isPartOfContentFolder == true);
                    _isPartOfContentFolder = isPart;
                    return _isPartOfContentFolder;
                }
                else return _isPartOfContentFolder;
            }
            set { _isPartOfContentFolder = value; } 
        }
        private bool _isPartOfContentFolder = false;
        private bool _isContentFolder = false;
        public DPFolder() { }
        public DPFolder(string _path, DPFolder __parent) {
            uid = DPIDManager.GetNewID();

            DPGlobal.dpObjects.Add(uid, this);
            // Check if path is root.
            // GetDirectoryName returns "" if looks like filename.  
            path = PathHelper.GetDirectoryPath(_path);
            
            //if (relativePathBase != null)
            //{
            //    relativePath = Path.GetRelativePath(path, relativePathBase);
            //}
            parent = __parent;
            extract = true;
            DPProcessor.workingArchive.folders.TryAdd(path, this);

        }
        ~DPFolder ()
        {
            DPIDManager.RemoveID(uid);
        }

        public static DPFolder CreateFolderForFile(string dpFilePath)
        {
            var workingStr = DPCommon.Up(dpFilePath);
            DPFolder firstFolder = null;
            DPFolder previousFolder = null;

            // Continously get relative path.
            while (workingStr != "")
            {
                // to do:

                var found = DPProcessor.workingArchive.RecursivelyFindFolder(workingStr, out _);

                if (!found)
                {
                    if (firstFolder == null)
                    {
                        firstFolder = new DPFolder(workingStr, null);
                        previousFolder = firstFolder;
                    } else
                    {
                        var workingParent = new DPFolder(workingStr, previousFolder);
                        //if (previousFolder != null)
                        //{
                        //    var IDPFolder = (IDPWorkingFile) previousFolder;
                        //    workingParent.addChild(ref IDPFolder);
                        //}
                        previousFolder = workingParent;
                    }
                }
                workingStr = DPCommon.Up(workingStr);
            }
            return firstFolder;
        }

        internal void UpdateChildrenRelativePaths()
        {
            // Needs to be relative to the content folder.
            if (isContentFolder)
            {
                foreach (var child in children.Values)
                {
                    var relativePath = PathHelper.GetRelativePath(child.path, path);
                    child.relativePath = relativePath;
                }
            } else
            {
                var contentFolder = GetContentFolder();
                if (contentFolder != null)
                {
                    foreach (var child in children.Values)
                    {
                        var relativePath = PathHelper.GetRelativePath(child.path, contentFolder.path);
                        child.relativePath = relativePath;
                    }
                }
            }

        }

        internal DPFolder GetContentFolder()
        {
            if (parent == null && (!parent.isPartOfContentFolder || !parent.isContentFolder)) return null;
            else
            {
                DPFolder workingFolder = this;
                while (workingFolder != null && workingFolder.isContentFolder == false)
                {
                    workingFolder = workingFolder.parent;
                }
                return workingFolder;
            }
        }
        
        /// <summary>
        /// Handles the addition of the file to children property and subfolders property (if child is a DPFolder).
        /// </summary>
        /// <param name="child">DPFolder, DPArchive, DPFile</param>
        public void addChild(ref IDPWorkingFile child)
        {
            if (child.GetType() == typeof(DPFolder))
            {
                var dpFolder = (DPFolder)child;
                subfolders.Add(dpFolder);
                return;
            }
            children.TryAdd(child.path, child);
        }

        public void removeChild(ref IDPWorkingFile child)
        {
            if (child.GetType() == typeof(DPFolder))
            {
                var dpFolder = (DPFolder)child;
                subfolders.Remove(dpFolder);
                return;
            }
            children.Remove(child.path);
        }

        public IDPWorkingFile[] GetFiles()
        {
            return children.Values.ToArray();
        }

        public DPFolder FindFolder(string _path)
        {
            if (path == _path) return this;
            else
            {
                foreach (var folder in subfolders)
                {
                    var result = folder.FindFolder(_path);
                    if (result != null) return result;
                }
            }
            return null;
        }
        public static DPFolder[] FindChildFolders(string _path, DPFolder self)
        {
            var folderArr = new List<DPFolder>();
            foreach (var folder in DPProcessor.workingArchive.folders.Values)
            {
                if (folder == self) continue;
                // And make sure it only is one level up.
                if (folder.path.Contains(_path) && (Path.GetFileName(_path) == Path.GetFileName(folder.path)) && PathHelper.GetNumOfLevelsAbove(folder.path, _path) == 1)
                {
                    folderArr.Add(folder);
                }
            }
            return folderArr.ToArray();
        }
        internal bool DetermineIfContentFolder()
        {
            // First, check if parent is a folder.
            string selfFolderName = PathHelper.GetLastDir(path, false);
            bool selfIsContentName = DPSettings.commonContentFolderNames.Contains(selfFolderName)
                        || DPSettings.folderRedirects.ContainsKey(selfFolderName);
            bool parentsAreContent = false;
            if (parent != null && parent.GetType() == typeof(DPFolder))
            {
                if (parent.isContentFolder) return false;
                foreach (var subfolder in parent.subfolders)
                {
                    if (subfolder == this) continue;
                    var folderName = Path.GetDirectoryName(subfolder.path);
                    // TO DO: Check if it contains given name, uppercased name and lower cased name.
                    if (DPSettings.commonContentFolderNames.Contains(folderName,StringComparer.CurrentCultureIgnoreCase) 
                        || DPSettings.folderRedirects.ContainsKey(folderName) || DPSettings.folderRedirects.ContainsKey(folderName.ToLower()))
                    {
                        parentsAreContent = true;
                        break;
                    }
                }
            }

            // Also check if subfolders were originally marked as content folders and make them false.
            foreach (var folder in subfolders)
            {
                folder.isContentFolder = false;
            }

            return selfIsContentName && !parentsAreContent;
        }

        
    }
}
