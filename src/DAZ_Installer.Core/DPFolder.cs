// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.IO;
using Serilog;
using IOPath = System.IO.Path;
namespace DAZ_Installer.Core
{
    public class DPFolder : DPAbstractNode
    {
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPFolder>();
        public override string NormalizedPath => PathHelper.NormalizePath(Path);
        public List<DPFolder> subfolders = new();
        private readonly Dictionary<string, DPFile> contents = new();
        public ICollection<DPFile> Contents => contents.Values;
        public bool IsContentFolder { get; set; }
        /// <summary>
        ///  Determined later in ProcessArchive().
        /// </summary>
        public bool IsPartOfContentFolder => (Parent?.IsPartOfContentFolder ?? false) || (Parent?.IsContentFolder ?? false);
        public DPFolder(string path, DPArchive arc, DPFolder? parent) : base(path, arc)
        {
            Logger.Debug("Creating folder for {Path}", path);
            // ZipArchive returns folders with a trailing slash, so we need to remove it.
            // Potentially others may do the same.
            Path = PathHelper.CleanDirPath(path);

            //if (relativePathBase != null)
            //{
            //    relativePath = Path.GetRelativePath(path, relativePathBase);
            //}
            Parent = parent;
            arc.Folders.TryAdd(Path, this);

        }

        /// <summary>
        /// Create folder (and subfolders) for file. This is used when a file is added to the archive and the folder it is in does not exist.
        /// This can occur when certain extractors discover files first rather than folders.
        /// </summary>
        /// <param name="dpFilePath"></param>
        /// <param name="associatedArchive"></param>
        /// <returns></returns>
        public static DPFolder CreateFoldersForFile(string dpFilePath, DPArchive associatedArchive)
        {
            var workingStr = PathHelper.Up(dpFilePath);
            DPFolder firstFolder = null;
            DPFolder previousFolder = null;

            // Continously get relative path.
            while (workingStr != "")
            {
                if (associatedArchive.FindFolder(workingStr, out _))
                {
                    workingStr = PathHelper.Up(workingStr);
                    continue;
                }
                if (firstFolder == null)
                {
                    firstFolder = new DPFolder(workingStr, associatedArchive, null);
                    previousFolder = firstFolder;
                }
                else
                {
                    var workingParent = new DPFolder(workingStr, associatedArchive, previousFolder);
                    previousFolder = workingParent;
                }
                workingStr = PathHelper.Up(workingStr);
            }
            return firstFolder!;
        }

        public void UpdateChildrenRelativePaths(DPProcessSettings settings)
        {
            DPFolder? contentFolder = IsContentFolder ? this : GetContentFolder();
            if (contentFolder is null)
            {
                Logger.Warning("Content folder was null, could not update relative paths for {Path}", Path);
                return;
            }
            foreach (DPFile child in contents.Values)
            {
                // This prevents the code for running twice on a child that was previously processed when ManifestAndAuto is on.
                if (!string.IsNullOrEmpty(child.RelativePathToContentFolder) && !string.IsNullOrEmpty(child.RelativeTargetPath))
                    continue;
                child.RelativePathToContentFolder = contentFolder.CalculateChildRelativePath(child);
                child.RelativeTargetPath = contentFolder.CalculateChildRelativeTargetPath(child, settings);
            }
        }

        /// <summary>
        /// Calculates the path of a child relative to this folder.
        /// </summary>
        /// <param name="child">The child of this folder.</param>
        /// <returns>A string representing the relative path of the child relative to this folder.</returns>
        public string CalculateChildRelativePath(DPAbstractNode child) => PathHelper.GetRelativePathOfRelativeParent(child.Path, Path);

        /// <summary>
        /// Calculates the target path of a child relative to this folder. Requires the settings object to
        /// check if the folder name is in <see cref="DPSettings.folderRedirects"/>.
        /// </summary>
        /// <param name="child">The child of this folder.</param>
        /// <param name="settings">The settings object in use.</param>
        /// <returns>A string representing the target path of the child relative to this folder.</returns>
        public string CalculateChildRelativeTargetPath(DPAbstractNode child, DPProcessSettings settings)
        {
            // TODO: In Processor, make sure the ContentRedirectFolders is never null.
            var containsKey = settings.ContentRedirectFolders.ContainsKey(FileName);
            if (!IsContentFolder || !containsKey) return child.RelativePathToContentFolder!;

            var i = Path.LastIndexOf(PathHelper.GetSeperator(Path));
            var newPath = PathHelper.NormalizePath(
                i != -1 ? string.Concat(Path.AsSpan(0, i + 1), settings.ContentRedirectFolders[FileName]) : settings.ContentRedirectFolders[FileName]
            );
            var childNewPath = PathHelper.NormalizePath(child.Path);
            i = childNewPath.IndexOf(Path);
            if (i != -1) childNewPath = childNewPath.Remove(i, Path.Length).Insert(i, newPath);

            return PathHelper.GetRelativePathOfRelativeParent(childNewPath, newPath);
        }

        public DPFolder? GetContentFolder()
        {
            if (Parent == null) return null;
            DPFolder? workingFolder = this;
            while (workingFolder != null && workingFolder.IsContentFolder == false)
            {
                workingFolder = workingFolder.Parent;
            }
            return workingFolder;
        }

        /// <summary>
        /// Handles the addition of the file to children property and subfolders property (if child is a DPFolder).
        /// </summary>
        /// <param name="child">DPFolder, DPArchive, DPFile</param>
        public void AddChild(DPAbstractNode child)
        {
            if (child is DPFolder folder)
            {
                subfolders.Add(folder);
                return;
            }
            contents.TryAdd(child.Path, (DPFile)child);
        }

        public void RemoveChild(DPAbstractNode child)
        {
            if (child.GetType() == typeof(DPFolder))
            {
                var dpFolder = (DPFolder)child;
                subfolders.Remove(dpFolder);
                return;
            }
            contents.Remove(child.Path);
        }

        public DPFolder? FindFolder(string _path)
        {
            if (Path == _path) return this;
            else
            {
                foreach (DPFolder folder in subfolders)
                {
                    DPFolder? result = folder.FindFolder(_path);
                    if (result != null) return result;
                }
            }
            return null;
        }
        public static DPFolder[] FindChildFolders(string _path, DPFolder self)
        {
            var folderArr = new List<DPFolder>();
            foreach (DPFolder folder in self.AssociatedArchive!.Folders.Values)
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
        /// <summary>
        /// <inheritdoc/>
        /// <p> This function removes and updates the root folders list instead of root contents list. </p>
        /// </summary>
        /// <param name="newParent">The new parent for this folder.</param>

        protected override void UpdateParent(DPFolder? newParent)
        {
            // If we were null, but now we're not...
            if (parent == null && newParent != null)
            {
                // Remove ourselves from root folders list of the working archive.
                try
                {
                    AssociatedArchive!.RootFolders.Remove(this);
                }
                catch { }

                // Call the folder's addChild() to add ourselves to the children list.
                newParent.AddChild(this);
                parent = newParent;
            }
            else if (parent == null && newParent == null)
            {
                // Try to find a parent.
                DPFolder? potParent = AssociatedArchive!.FindParent(this);

                // If we found a parent, then update it. This function will be called again.
                if (potParent != null)
                {
                    Parent = potParent;
                }
                else
                {
                    // Otherwise, create a folder for us.
                    potParent = CreateFoldersForFile(Path, AssociatedArchive);

                    // If we have successfully created a folder for us, then update it. This function will be called again.
                    if (potParent != null) Parent = potParent;
                    else
                    { // Otherwise, we are supposed to be at root.
                        parent = null;
                        if (!AssociatedArchive!.RootFolders.Contains(this))
                        {
                            AssociatedArchive!.RootFolders.Add(this);
                        }
                    }
                }
            }
            else if (parent != null && newParent != null)
            {
                // Remove ourselves from previous parent children.
                parent.RemoveChild(this);

                // Add ourselves to new parent's children.
                newParent.AddChild(this);

                parent = newParent;
            }
            else if (parent != null && newParent == null)
            {
                // Remove ourselves from previous parent's children.
                parent.RemoveChild(this);

                // Add ourselves to the archive's root content list.
                AssociatedArchive!.RootFolders.Add(this);
                parent = newParent;
            }
        }


    }
}
