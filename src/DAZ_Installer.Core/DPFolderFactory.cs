using DAZ_Installer.IO;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// A factory for creating folders.
    /// </summary>
    public sealed class DPFolderFactory : IDPFolderFactory
    {
        /// <summary>
        /// A single instance of the factory.
        /// </summary>
        /// <remarks>
        /// Use this singleton object to interact with the class.
        /// </remarks> 
        public static readonly DPFolderFactory Instance = new();
        /// <summary>
        /// A private constructor to prevent instantiation.
        /// </summary>
        private DPFolderFactory() { }
        /// <summary>
        /// Create folder (and subfolders) for file. This is used when a file is added to the archive and the folder it is in does not exist.
        /// This can occur when certain extractors discover files first rather than folders.
        /// Make sure that the folder does not exist before calling this function!
        /// </summary>
        /// <param name="dpFilePath">The path to create folders for.</param>
        /// <param name="associatedArchive">The associated archive to create folders to.</param>
        public IDPFolder CreateFolders(string dpFilePath, IDPArchive associatedArchive)
        {
            var seperator = PathHelper.GetSeperator(dpFilePath);
            var pathParts = dpFilePath.Split(seperator);
            IDPFolder? currentFolder = null;
            var currentPath = "";

            for (var i = 0; i < pathParts.Length - 1; i++)
            {
                currentPath = Path.Combine(currentPath, pathParts[i]);

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
        /// <inheritdoc/>
        /// </summary>
        /// <returns>A <see cref="DPFolder"/> object</returns>
        public IDPFolder CreateFolder(string path, IDPArchive parentArchive, IDPFolder? parentFolder)
        {
            return new DPFolder(path, parentArchive, parentFolder);
        }

    }
}