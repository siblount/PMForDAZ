namespace DAZ_Installer.Core.Extraction {
    /// <summary>
    /// The default extractor factory that creates extractors for the default archive formats.
    /// </summary>
    public class DPExtractorFactory : IDPExtractorFactory
    {
        public static readonly DPExtractorFactory Singleton = new DPExtractorFactory();

        /// <summary>
        /// A private constructor to prevent instantiation.
        /// </summary>
        private DPExtractorFactory() { }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns>The default extractor, null if <paramref name="format"/> is <see cref="ArchiveFormat.Unknown"/></returns>
        public DPAbstractExtractor? CreateExtractor(ArchiveFormat format)
        {
            return format switch
            {
                ArchiveFormat.SevenZ => new DP7zExtractor(),
                ArchiveFormat.RAR => new DPRARExtractor(),
                ArchiveFormat.WinZip => new DPZipExtractor(),
                _ => null,
            };
        }
    }
}
