namespace DAZ_Installer.Core
{
    /// <summary>
    /// Represents the extraction progress arguments.
    /// </summary>
    public class DPExtractProgressArgs : EventArgs
    {
        /// <summary>
        /// Percentage of the total extraction progress.
        /// </summary>
        public readonly byte ExtractionPercentage = 0;
        /// <summary>
        /// The archive that is currently extracting files.
        /// </summary>
        public readonly IDPArchive Archive;
        /// <summary>
        /// The file that is currently being extracted from archive. Sometimes this is null. This can occur
        /// when the archive has just finished the extraction process.
        /// </summary>
        public readonly IDPAbstractNode? File;

        internal DPExtractProgressArgs(byte percent, IDPArchive archive, IDPAbstractNode? file) : base()
        {
            ExtractionPercentage = percent;
            Archive = archive;
            File = file;
        }
    }
}
