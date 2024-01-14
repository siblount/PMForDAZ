namespace DAZ_Installer.Core
{
    /// <summary>
    /// Options for how <see cref="DPProcessor"/> should determine which files
    /// should be moved into the user's library.
    /// </summary>
    // TODO: Turn this into a flag, change "Automatic" to "File Sense"
    // TODO: Flag 1 - Manifest, 2 - Auto, 3 - FallbackToAuto
    public enum InstallOptions
    {
        /// <summary>
        /// Will strictly read the manifest in the archive, if any, to determine
        /// which files should be moved into the user's library. <para/>
        /// If there is no manifest detected, then no files will be moved.
        /// </summary>
        ManifestOnly,
        /// <summary>
        /// Will rely on the manifest first then do "File Sense" by checking which files
        /// are under a "Content Folder" defined in <see cref="DPProcessSettings.ContentFolders"/>. <para/>
        /// Additionally, any nested archive will be processed to find any potential products. Lastly, any files/folders
        /// not under a content folder will not be moved unless it is defined in the manifest.
        /// </summary>
        ManifestAndAuto,
        /// <summary>
        /// Does "File Sense" by checking which files are under a "Content Folder" 
        /// defined in defined in <see cref="DPProcessSettings.ContentFolders"/>. <para/>
        /// Additionally, any nested archive will be processed to find any potential products. <para/>
        /// Lastly, any files/folders not under a content folder will not be moved 
        /// unless it is defined in the manifest.
        /// </summary>
        Automatic
    }

    /// <summary>
    /// DPProcessSettings is used for extraction and processing operations. 
    /// </summary>
    public struct DPProcessSettings
    {
        /// <summary>
        /// The temporary path to use for operations. Null not allowed.
        /// </summary>
        public string TempPath = string.Empty;
        /// <summary>
        /// The final destination path to use for operations. Null not allowed.
        /// </summary>
        public string DestinationPath = string.Empty;
        /// <summary>
        /// The user-defined folder names to use as content folders. <para/>
        /// If null, <see cref="DPProcessor"/> will use most common content folders.
        /// </summary>
        public HashSet<string>? ContentFolders = null;
        /// <summary>
        /// The user-defined content redirect folders to redirect certain folder names to a content folder. <para/>
        /// If <see langword="null"/>, <see cref="DPProcessor"/> will use most common content redirect folders.
        /// </summary>
        public Dictionary<string, string>? ContentRedirectFolders = null;
        /// <summary>
        /// The method the processor should use for determining which files should be moved into 
        /// the destination path or not. <para/>
        /// </summary>
        public InstallOptions InstallOption = InstallOptions.ManifestAndAuto;
        /// <summary>
        /// Determines whether the processor should overwrite files if it already exists in the user library.
        /// Temp files will always be overwritten.
        /// </summary>
        public bool OverwriteFiles = true;
        /// <summary>
        /// Overrides the usual process of determining the destination of files and forces them to be moved to the specified destination in this dictionary. <para/>
        /// This means that no matter what the file is, it will be moved to the specified destination. <para/>
        /// </summary>
        /// <typeparam name="DPFile">The file to force to a destination.</typeparam>
        /// <typeparam name="string">The destination to force the file to.</typeparam>
        public Dictionary<DPFile, string> ForceFileToDest = new(0);
        public DPProcessSettings(string temp, string dest, InstallOptions options,
            HashSet<string>? folders = null, Dictionary<string, string>? redirects = null, bool overwriteFiles = true)
        {
            TempPath = temp;
            DestinationPath = dest;
            InstallOption = options;
            ContentFolders = folders;
            ContentRedirectFolders = redirects;
            OverwriteFiles = overwriteFiles;
        }
    }
}
