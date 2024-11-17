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
        public HashSet<IDPFile> FilesToExtract = new(0);
        /// <summary>
        /// An archive to extract from. This can be implicitly set by <see cref="FilesToExtract"/>. <para/>
        /// All files in <see cref="FilesToExtract"/> must be in this archive.
        /// Or in other words, the <br/> <see cref="DPAbstractNode.AssociatedArchive"/> of all 
        /// files in <see cref="FilesToExtract"/> must be this archive.
        /// </summary>
        public IDPArchive Archive = null!;
        /// <summary>
        /// The cancellation token to use for the extraction. This setting will update <see cref="DPAbstractExtractor.CancellationToken"/>.
        /// </summary>
        public CancellationToken CancelToken = CancellationToken.None;
        /// <summary>
        /// An extraction settings object for the extractor to use.
        /// </summary>
        /// <param name="temp">The temporary directory to store data, if needed.</param>
        /// <param name="filesToExtract">The files to extract from the archive.</param>
        /// <param name="overwriteFiles">Determines whether it is okay to overwrite files.</param>
        /// <param name="archive">The archive to extract files from. 
        /// If not provided, it will be detected from the first file in 
        /// <paramref name="filesToExtract"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="archive"/> is null and could not find the 
        /// associated archive from the first file in <paramref name="filesToExtract"/>.
        /// </exception>
        public DPExtractSettings(string? temp, IEnumerable<IDPFile> filesToExtract, bool overwriteFiles = true, IDPArchive? archive = null)
        {
            TempPath = temp ?? string.Empty;
            OverwriteFiles = overwriteFiles;
            FilesToExtract = new HashSet<IDPFile>(filesToExtract);
            Archive = archive ?? filesToExtract.FirstOrDefault()?.AssociatedArchive!;
            ArgumentNullException.ThrowIfNull(Archive, nameof(archive));
        }
    }
}
