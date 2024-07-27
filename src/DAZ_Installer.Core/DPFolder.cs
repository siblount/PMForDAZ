// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.IO;
using Serilog;
namespace DAZ_Installer.Core
{
    public class DPFolder : DPAbstractNode
    {
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPFolder>();
        /// <summary>
        /// A list of subfolders in this folder.
        /// </summary>
        public List<DPFolder> Subfolders = new();
        /// <summary>
        /// A list of files in this folder.
        /// </summary>
        public readonly HashSet<DPFile> Contents = new();
        /// <summary>
        /// Describes whether this folder is a content folder.
        /// </summary>
        public bool IsContentFolder { get; set; }
        /// <summary>
        /// Gets a value indicating whether this folder is part of a content folder structure.
        /// </summary>
        /// <remarks>
        /// A folder is considered part of a content folder structure if:
        /// <list type="bullet">
        /// <item><description>It is a content folder itself (i.e., <see cref="IsContentFolder"/> is true).</description></item>
        /// <item><description>It has a parent folder that is part of a content folder structure.</description></item>
        /// </list>
        /// This property is useful for determining if a folder is within the hierarchy of a content folder,
        /// which may affect how it's processed or displayed in the application.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if this folder is a content folder or has a parent that is part of a content folder structure; otherwise, <c>false</c>.
        /// </returns>
        public bool IsPartOfContentFolder => !IsContentFolder && ((Parent?.IsPartOfContentFolder ?? false) || (Parent?.IsContentFolder ?? false));
        /// <summary>
        /// A constructor for creating a folder object.
        /// </summary>
        /// <param name="path">The path to set for this folder.</param>
        /// <param name="arc">The associated archive.</param>
        /// <param name="parent">The parent of this folder, if any. If null, it will search and create the parent if this is a subfolder. </param>
        public DPFolder(string path, DPArchive arc, DPFolder? parent) : base(path, arc)
        {
            Logger.Debug("Creating folder for {Path}", path);
            // ZipArchive returns folders with a trailing slash, so we need to remove it.
            // Potentially others may do the same.
            Path = PathHelper.CleanDirPath(path);

            arc.Folders.TryAdd(NormalizedPath, this);
            Parent = parent;
        }

        /// <summary>
        /// Create folder (and subfolders) for file. This is used when a file is added to the archive and the folder it is in does not exist.
        /// This can occur when certain extractors discover files first rather than folders.
        /// Make sure that the folder does not exist before calling this function!
        /// </summary>
        /// <param name="dpFilePath">The path to create folders for.</param>
        /// <param name="associatedArchive">The associated archive to create folders to.</param>
        public static DPFolder CreateFoldersForFile(string dpFilePath, DPArchive associatedArchive)
        {
            var seperator = PathHelper.GetSeperator(dpFilePath);
            var pathParts = dpFilePath.Split(seperator);
            DPFolder? currentFolder = null;
            var currentPath = "";

            for (var i = 0; i < pathParts.Length - 1; i++)
            {
                currentPath = System.IO.Path.Combine(currentPath, pathParts[i]);
                
                if (associatedArchive.FindFolder(PathHelper.NormalizePath(currentPath), out var existingFolder))
                {
                    currentFolder = existingFolder;
                    continue;
                }

                var newFolder = new DPFolder(PathHelper.SwitchToSeperator(currentPath, seperator), associatedArchive, currentFolder);

                currentFolder = newFolder;
            }

            return currentFolder!;
        }

        /// <summary>
        /// Updates the relative paths of all children files in this folder. This requires that the folder or a parent folder 
        /// is declared as a content folder by setting <see cref="IsContentFolder"/> to true on the content folder.
        /// It updates the <see cref="DPAbstractNode.RelativePathToContentFolder"/> and <see cref="DPAbstractNode.RelativeTargetPath"/> properties.
        /// <paramref name="settings"/> is used to calculate the <see cref="DPAbstractNode.RelativeTargetPath"/>.
        /// </summary>
        /// <param name="settings">The process settings to calculate the <see cref="DPAbstractNode.RelativeTargetPath"/> property of child contents in <see cref="Contents"/></param>
        public void UpdateChildrenRelativePaths(DPProcessSettings settings)
        {
            DPFolder? contentFolder = IsContentFolder ? this : GetContentFolder();
            if (contentFolder is null)
            {
                Logger.Warning("Content folder was null, could not update relative paths for {Path}", Path);
                return;
            }
            foreach (DPFile child in Contents)
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
        /// Calculates the target path of a child relative to this folder. Requires <paramref name="settings"/> to
        /// to determine the relative target path which is used to calculate the target path of the child.
        /// </summary>
        /// <param name="child">The child of this folder.</param>
        /// <param name="settings">The settings object in use.</param>
        /// <returns>A string representing the target path of the child relative to this folder.</returns>
        public string CalculateChildRelativeTargetPath(DPAbstractNode child, DPProcessSettings settings)
        {
            // TODO: In Processor, make sure the ContentRedirectFolders is never null.
            ArgumentNullException.ThrowIfNull(settings.ContentRedirectFolders, nameof(settings.ContentRedirectFolders));

            var containsKey = settings.ContentRedirectFolders.ContainsKey(FileName);
            if (!IsContentFolder || !containsKey) return child.RelativePathToContentFolder!;

            var i = Path.LastIndexOf(PathHelper.GetSeperator(Path));
            var newPath = PathHelper.NormalizePath(
                i != -1 ? string.Concat(Path.AsSpan(0, i + 1), settings.ContentRedirectFolders[FileName]) : settings.ContentRedirectFolders[FileName]
            );
            var childNewPath = child.NormalizedPath;
            i = childNewPath.IndexOf(NormalizedPath);
            if (i != -1) childNewPath = childNewPath.Remove(i, Path.Length).Insert(i, newPath);

            return PathHelper.GetRelativePathOfRelativeParent(childNewPath, newPath);
        }

        /// <summary>
        /// Attempts to find the folder that is declared as a content folder (via: <see cref="IsContentFolder"/>).
        /// It will first check if the current folder is a content folder, if not, it will recursively check the parent folders if they are content folders.
        /// It will return the first folder that is declared as a content folder.
        /// </summary>
        /// <returns>
        /// The current folder if it is marked as <see cref="IsContentFolder"/>, 
        /// or a parent folder (or parent of parents) that is a content folder, 
        /// or null if one could not be found. 
        /// </returns>
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
        /// Handles the addition of the file to children property and subfolders property (if child is a <see cref="DPFolder"/>).
        /// Nothing more, nothing less.
        /// <para/>
        /// DO NOT use this for moving a child from one folder to another. 
        /// This does not update the <see cref="DPAbstractNode.Parent"/> property of the child. 
        /// Change the parent property of the child instead to handle everything.
        /// </summary>
        /// <param name="child">Either a <see cref="DPFolder"/> or a <see cref="DPFile"/>.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="child"/> is not a <see cref="DPFolder"/> or a <see cref="DPFile"/>.</exception>
        public void AddChild(DPAbstractNode child)
        {
            if (child is not DPFolder && child is not DPFile)
                throw new ArgumentException("Child must be a DPFolder or DPFile.", nameof(child));

            if (child is DPFolder folder)
                Subfolders.Add(folder);
            else if (child is DPFile file && !Contents.Contains(file))
                Contents.Add(file);
        }

        /// <summary>
        /// Removes a child from the children property or subfolders property (if child is a <see cref="DPFolder"/>).
        /// This does not update the <see cref="DPAbstractNode.Parent"/> property of the child. Use <see cref="DPAbstractNode.Parent"/> property to handle that.
        /// </summary>
        /// <param name="child">Either a <see cref="DPFolder"/> or a <see cref="DPFile"/>.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="child"/> is not a <see cref="DPFolder"/> or a <see cref="DPFile"/>.</exception>
        public void RemoveChild(DPAbstractNode child)
        {
            if (child is not DPFolder && child is not DPFile)
                throw new ArgumentException("Child must be a DPFolder or DPFile.", nameof(child));
            if (child.GetType() == typeof(DPFolder))
            {
                var dpFolder = (DPFolder)child;
                Subfolders.Remove(dpFolder);
                return;
            }
            Contents.Remove((DPFile)child);
        }

        /// <summary>
        /// Updates the parent of this folder and manages the associated relationships.
        /// </summary>
        /// <param name="newParent">The new parent folder for this folder. Can be null if the folder is to become a root folder.</param>
        /// <remarks>
        /// This method handles various scenarios:
        /// <list type="bullet">
        /// <item><description>When the current parent is null and a new parent is assigned.</description></item>
        /// <item><description>When both the current and new parent are null (attempts to find or create a parent).</description></item>
        /// <item><description>When changing from one parent to another.</description></item>
        /// <item><description>When removing the current parent (becoming a root folder).</description></item>
        /// </list>
        /// The method ensures that:
        /// <list type="bullet">
        /// <item><description>The folder is removed from its previous parent's children list (if applicable).</description></item>
        /// <item><description>The folder is added to its new parent's children list (if applicable).</description></item>
        /// <item><description>The folder is added to or removed from the associated archive's root folders list as necessary.</description></item>
        /// <item><description>If no parent is found or created, the folder becomes a root folder in the associated archive.</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="NullReferenceException">May be thrown if <see cref="AssociatedArchive"/> is null.</exception>

        protected override void UpdateParent(DPFolder? newParent)
        {
            // If we were null, but now we're not...
            if (parent == null && newParent != null)
            {
                // Remove ourselves from root folders list of the working archive.
                if (AssociatedArchive?.RootFolders.Contains(this) ?? false)
                    AssociatedArchive.RootFolders.Remove(this);

                // Call the folder's addChild() to add ourselves to the children list.
                newParent.AddChild(this);
                parent = newParent;
                AssociatedArchive?.Folders.TryAdd(NormalizedPath, this);
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
                    // Fake a file so we can create folders for us.
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
