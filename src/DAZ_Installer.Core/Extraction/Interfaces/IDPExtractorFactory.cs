namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// Factory for creating extractors.
    /// </summary>
    public interface IDPExtractorFactory
    {
        /// <summary>
        /// Attempts to create an extractor for the given format.
        /// </summary>
        /// <param name="format">The archive format of the archive</param>
        /// <returns>The default extractor for the given <paramref name="format"/>, if one exists.</returns>
        DPAbstractExtractor? CreateExtractor(ArchiveFormat format);
    }
}