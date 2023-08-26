using DAZ_Installer.IO;

namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// The extract settings for the archive to use.
    /// </summary>
    [Serializable]
    public struct DPExtractSettings
    {
        /// <summary>
        /// The temporary path to use for operations. Null not allowed. <br/>
        /// This is used if the archive needs to be extracted to a temporary location before being moved to the destination.
        /// </summary>
        public string TempPath = string.Empty;
        /// <summary>
        /// Determines whether the extractor should overwrite files if it already exists in the user library.
        /// Temp files will always be overwritten.
        /// </summary>
        public bool OverwriteFiles = true;
        /// <summary>
        /// A collection of files to extract. Files in this collection <b>MUST BE IN <see cref="Archive"/></b>.
        /// </summary>
        /// <paramtype name="DPFile">The file from the archive to extract.</paramtype>
        public HashSet<DPFile> FilesToExtract = new(0);
        /// <summary>
        /// An archive to extract from. This can be implicitly set by <see cref="FilesToExtract"/>. <para/>
        /// All files in <see cref="FilesToExtract"/> must be in this archive.
        /// Or in other words, the <br/> <see cref="DPAbstractNode.AssociatedArchive"/> of all 
        /// files in <see cref="FilesToExtract"/> must be this archive.
        /// </summary>
        public DPArchive Archive = null!;

        public DPExtractSettings(string? temp, IEnumerable<DPFile> filesToExtract, bool overwriteFiles = true, DPArchive? archive = null)
        {
            TempPath = temp ?? string.Empty;
            OverwriteFiles = overwriteFiles;
            FilesToExtract = new HashSet<DPFile>(filesToExtract);
            Archive = archive ?? filesToExtract.FirstOrDefault()?.AssociatedArchive ?? throw new ArgumentException("No archive provided and no files to extract provided.");
        }
    }
}
