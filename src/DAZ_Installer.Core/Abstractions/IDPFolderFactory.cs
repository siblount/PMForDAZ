namespace DAZ_Installer.Core {
    /// <summary>
    /// A factory for creating folders.
    /// </summary>
    public interface IDPFolderFactory {
        /// <summary>
        /// Create folder (and subfolders) for an <see cref="IDPAbstractNode"/>.
        /// </summary> 
        /// <param name="dpFilePath">The path to create folders for.</param>
        /// <param name="associatedArchive">The associated archive to create folders to.</param>
        public IDPFolder CreateFolders(string dpFilePath, IDPArchive associatedArchive);
        /// <summary>
        /// Creates a new folder with the given path, parent archive, and parent folder.
        /// </summary>
        /// <param name="path">The raw path to set for this folder.</param>
        /// <param name="parentArchive">The parent archive for this folder.</param>
        /// <param name="parentFolder">The parent folder for this folder, if any.</param>
        public IDPFolder CreateFolder(string path, IDPArchive parentArchive, IDPFolder? parentFolder);
    }
}