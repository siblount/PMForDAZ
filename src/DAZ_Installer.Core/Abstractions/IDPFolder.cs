namespace DAZ_Installer.Core
{
    /// <summary>
    /// Represents a folder in archive space.
    /// </summary>
    public interface IDPFolder : IDPAbstractNode
    {
        /// <summary>
        /// <inheritdoc cref="IDPFolderFactory"/>
        /// </summary>
        public IDPFolderFactory FolderFactory { get; set; }
        /// <summary>
        /// A list of subfolders in this folder.
        /// </summary>
        List<IDPFolder> Subfolders { get; }

        /// <summary>
        /// A set of files in this folder.
        /// </summary>
        HashSet<IDPFile> Contents { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this folder is a content folder.
        /// </summary>
        bool IsContentFolder { get; set; }

        /// <summary>
        /// Gets a value indicating whether this folder is part of a content folder structure.
        /// </summary>
        bool IsPartOfContentFolder { get; }

        /// <summary>
        /// Updates the relative paths of all children files in this folder.
        /// </summary>
        /// <param name="settings">The process settings to calculate the RelativeTargetPath property of child contents.</param>
        void UpdateChildrenRelativePaths(DPProcessSettings settings);

        /// <summary>
        /// Calculates the path of a child relative to this folder.
        /// </summary>
        /// <param name="child">The child of this folder.</param>
        /// <returns>A string representing the relative path of the child relative to this folder.</returns>
        string CalculateChildRelativePath(IDPAbstractNode child);

        /// <summary>
        /// Calculates the target path of a child relative to this folder.
        /// </summary>
        /// <param name="child">The child of this folder.</param>
        /// <param name="settings">The settings object in use.</param>
        /// <returns>A string representing the target path of the child relative to this folder.</returns>
        string CalculateChildRelativeTargetPath(IDPAbstractNode child, DPProcessSettings settings);

        /// <summary>
        /// Attempts to find the folder that is declared as a content folder.
        /// </summary>
        /// <returns>The content folder or null if one could not be found.</returns>
        IDPFolder? GetContentFolder();

        /// <summary>
        /// Adds a child node to this folder.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        void AddChild(IDPAbstractNode child);

        /// <summary>
        /// Removes a child node from this folder.
        /// </summary>
        /// <param name="child">The child node to remove.</param>
        void RemoveChild(IDPAbstractNode child);
    }
}