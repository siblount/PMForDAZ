namespace DAZ_Installer.Core
{
    /// <summary>
    /// Represents error arguments for <see cref="DPArchive"/> errors.
    /// </summary>
    public class DPArchiveErrorArgs : DPProcessorErrorArgs
    {
        /// <summary>
        /// The archive the <see cref="DPProcessor"/> was processing when the error occurred.
        /// </summary>
        public DPArchive Archive { get; init; }
        /// <summary>
        /// Determine whether the archive should cancel the operation or not.
        /// <para/>
        /// Change this value to <see langword="true"/> if you wish to cancel processing the 
        /// archive. <para/>
        /// This will only be honored if <see cref="Continuable"/> is <see langword="true"/>.
        /// </summary>
        public new bool CancelOperation { get; set; } = true;
        /// <summary>
        /// <inheritdoc cref="DPArchiveErrorArgs"/>
        /// </summary>
        /// <param name="ex">The exception thrown by the error, if any.</param>
        /// <param name="explaination">The additional explaination for the error/situation.</param>
        /// <param name="archive">The archive that errored.</param>
        internal DPArchiveErrorArgs(DPArchive archive, Exception? ex = null,
            string? explaination = null) : base(ex, explaination)
        {
            Ex = ex;
            if (explaination != null)
                Explaination = explaination;
            Archive = archive;
        }
        /// <summary>
        /// Explanation for when the archive is encrypted.
        /// </summary>
        internal const string EncryptedArchiveExplanation = "Cannot process encrypted archives at this time.";
        /// <summary>
        /// Explanation for when the archive contains encrypted files.
        /// </summary>
        internal const string EncryptedFilesExplanation = "Cannot process archives with encrypted files at this time.";
        internal const string UnauthorizedAccessExplanation = "Failed to extract file due to unauthorized access.";
        internal const string UnauthorizedAccessAfterExplanation = "Failed to extract file due to unauthorized access (even after attempting to fix file attribute).";
        internal const string ArchiveDoesNotExistOrNoAccessExplanation = "Archive does not exist on disk or has permissions issue.";
        /// <summary>
        /// Format for explanation for when a file is not part of the archive.
        /// </summary>

        internal const string FileNotPartOfArchiveErrorFormat = "File {0} is not part of this archive.";
    }
}
