// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Linq;
using IOPath = System.IO.Path;
namespace DAZ_Installer.DP
{
    internal class DPFolder : DPAbstractFile
    {

        internal List<DPFolder> subfolders = new List<DPFolder>();
        private Dictionary<string, DPAbstractFile> children = new Dictionary<string, DPAbstractFile>();
        internal bool isContentFolder { get; set;}
        /// <summary>
        ///  Determined later in ProcessArchive().
        /// </summary>
        internal bool isPartOfContentFolder
        {
            get => (Parent?.isPartOfContentFolder ?? false) || (Parent?.isContentFolder ?? false);
        }
        internal DPFolder(string path, DPFolder parent) : base(path)
        {
            UID = DPIDManager.GetNewID();
            // Check if path is root.
            // GetDirectoryName returns "" if looks like filename.  
            Path = PathHelper.GetDirectoryPath(path);

            //if (relativePathBase != null)
            //{
            //    relativePath = Path.GetRelativePath(path, relativePathBase);
            //}
            Parent = parent;
            WillExtract = true;
            DPProcessor.workingArchive.Folders.TryAdd(Path, this);

        }

        internal static DPFolder CreateFolderForFile(string dpFilePath)
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
                    }
                    else
                    {
                        var workingParent = new DPFolder(workingStr, previousFolder);
                        //if (previousFolder != null)
                        //{
                        //    var IDPFolder = (DPAbstractFile) previousFolder;
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
                    var relativePath = PathHelper.GetRelativePath(child.Path, Path);
                    child.RelativePath = relativePath;
                }
            }
            else
            {
                var contentFolder = GetContentFolder();
                if (contentFolder != null)
                {
                    foreach (var child in children.Values)
                    {
                        var relativePath = PathHelper.GetRelativePath(child.Path, contentFolder.Path);
                        child.RelativePath = relativePath;
                    }
                }
            }

        }

        internal DPFolder GetContentFolder()
        {
            if (Parent == null && (!Parent.isPartOfContentFolder || !Parent.isContentFolder)) return null;
            else
            {
                DPFolder workingFolder = this;
                while (workingFolder != null && workingFolder.isContentFolder == false)
                {
                    workingFolder = workingFolder.Parent;
                }
                return workingFolder;
            }
        }

        /// <summary>
        /// Handles the addition of the file to children property and subfolders property (if child is a DPFolder).
        /// </summary>
        /// <param name="child">DPFolder, DPArchive, DPFile</param>
        internal void addChild(DPAbstractFile child)
        {
            if (child.GetType() == typeof(DPFolder))
            {
                var dpFolder = (DPFolder)child;
                subfolders.Add(dpFolder);
                return;
            }
            children.TryAdd(child.Path, child);
        }

        internal void removeChild(DPAbstractFile child)
        {
            if (child.GetType() == typeof(DPFolder))
            {
                var dpFolder = (DPFolder)child;
                subfolders.Remove(dpFolder);
                return;
            }
            children.Remove(child.Path);
        }

        internal DPAbstractFile[] GetFiles()
        {
            return children.Values.ToArray();
        }

        internal DPFolder FindFolder(string _path)
        {
            if (Path == _path) return this;
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
        internal static DPFolder[] FindChildFolders(string _path, DPFolder self)
        {
            var folderArr = new List<DPFolder>();
            foreach (var folder in DPProcessor.workingArchive.Folders.Values)
            {
                if (folder == self) continue;
                // And make sure it only is one level up.
                if (folder.Path.Contains(_path) && IOPath.GetFileName(_path) == IOPath.GetFileName(folder.Path) 
                                                && PathHelper.GetNumOfLevelsAbove(folder.Path, _path) == 1)
                {
                    folderArr.Add(folder);
                }
            }
            return folderArr.ToArray();
        }
        internal bool DetermineIfContentFolder()
        {
            // First, check if parent is a folder.
            string selfFolderName = PathHelper.GetLastDir(Path, false);
            bool selfIsContentName = DPSettings.commonContentFolderNames.Contains(selfFolderName)
                        || DPSettings.folderRedirects.ContainsKey(selfFolderName);
            bool parentsAreContent = false;
            if (Parent != null && Parent.GetType() == typeof(DPFolder))
            {
                if (Parent.isContentFolder) return false;
                foreach (var subfolder in Parent.subfolders)
                {
                    if (subfolder == this) continue;
                    var folderName = IOPath.GetDirectoryName(subfolder.Path);
                    // TO DO: Check if it contains given name, uppercased name and lower cased name.
                    if (DPSettings.commonContentFolderNames.Contains(folderName, StringComparer.CurrentCultureIgnoreCase)
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
        /// <summary>
        /// <inheritdoc/>
        /// <p> This function removes and updates the root folders list instead of root contents list. </p>
        /// </summary>
        /// <param name="newParent">The new parent for this folder.</param>

        internal override void UpdateParent(DPFolder? newParent) {
            // If we were null, but now we're not...
            if (_parent == null && newParent != null) {
                // Remove ourselves from root folders list of the working archive.
                try {
                    DPProcessor.workingArchive.RootFolders.Remove(this);
                } catch {}

                // Call the folder's addChild() to add ourselves to the children list.
                newParent.addChild(this);
                _parent = newParent;
            } else if (_parent == null && newParent == null) {
                // Try to find a parent.
                var potParent = DPProcessor.workingArchive.FindParent(this);

                // If we found a parent, then update it. This function will be called again.
                if (potParent != null) {
                    Parent = potParent;
                } else {
                    // Otherwise, create a folder for us.
                    potParent = CreateFolderForFile(Path);
                    
                    // If we have successfully created a folder for us, then update it. This function will be called again.
                    if (potParent != null) Parent = potParent;
                    else { // Otherwise, we are supposed to be at root.
                        _parent = null;
                        if (!DPProcessor.workingArchive.RootFolders.Contains(this)) {
                            DPProcessor.workingArchive.RootFolders.Add(this);
                        }
                    }
                }
            } else if (_parent != null && newParent != null) {
                // Remove ourselves from previous parent children.
                _parent.removeChild(this);

                // Add ourselves to new parent's children.
                newParent.addChild(this);

                _parent = newParent;
            } else if (_parent != null && newParent == null) {
                // Remove ourselves from previous parent's children.
                _parent.removeChild(this);

                // Add ourselves to the archive's root content list.
                DPProcessor.workingArchive.RootFolders.Add(this);
                _parent = newParent;
            }
        }


    }
}
