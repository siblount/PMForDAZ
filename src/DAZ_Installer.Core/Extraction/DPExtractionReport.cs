namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// A report that is generated after the extraction process, even if the process has been interrupted.
    /// </summary>
    public record DPExtractionReport
    {
        /// <summary>
        /// Files that have been successfully extracted.
        /// </summary>
        public List<DPFile> ExtractedFiles = new(0);
        /// <summary>
        /// The settings used for the extraction.
        /// </summary>
        public DPExtractSettings Settings;
        /// <summary>
        /// Errors that occurred while attempting to extract files. This may be empty if an error occured interanlly
        /// before the extractor could extract the files.
        /// </summary>
        /// <typeparam name="DPFile">The file that errored while attempting to extract it.</typeparam>
        /// <typeparam name="string">The error message.</typeparam>
        public Dictionary<DPFile, string> ErroredFiles = new(0);
        /// <summary>
        /// The percentage of files that successfully extracted.
        /// </summary>
        public float SuccessPercentage => (float)ExtractedFiles.Count / ExtractedFiles.Count + ErroredFiles.Count;
    }
}
