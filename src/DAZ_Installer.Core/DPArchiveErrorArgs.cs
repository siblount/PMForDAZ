namespace DAZ_Installer.Core
{
    /// <summary>
    /// Represents error arguments for <see cref="DPArchive"/> errors.
    /// </summary>
    public sealed class DPArchiveErrorArgs : DPErrorArgs
    {
        /// <summary>
        /// The archive the <see cref="DPProcessor"/> was processing when the error occurred.
        /// </summary>
        public IDPArchive Archive { get; init; }
        /// <summary>
        /// <inheritdoc cref="DPArchiveErrorArgs"/>
        /// </summary>
        /// <param name="ex">The exception thrown by the error, if any.</param>
        /// <param name="explaination">The additional explaination for the error/situation.</param>
        /// <param name="archive">The archive that errored.</param>
        internal DPArchiveErrorArgs(IDPArchive archive, Exception? ex = null, string? explaination = null) : base(ex, explaination)
        {
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
