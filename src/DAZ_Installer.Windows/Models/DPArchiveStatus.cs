namespace DAZ_Installer.Windows.DP {
    /// <summary>
    /// Demonstrates the current status of an archive being processed.
    /// </summary>
    /// <seealso cref="DPExtractJob"/>
    public enum DPArchiveStatus {
        /// <summary>
        /// The archive has not yet been processed.
        /// </summary>
        Pending,
        /// <summary>
        /// The archive is currently being processed.
        /// </summary>
        Processing,
        /// <summary>
        /// The archive has successfully been processed successfully.
        /// </summary>
        Completed,
        /// <summary>
        /// The archive has been completed but with some issues.
        /// </summary>
        CompletedWithIssues,
        /// <summary>
        /// The archive was scheduled to be processed, or was being processed, but is now cancelled.
        /// </summary>
        Cancelled,
        /// <summary>
        /// The archive is currently in process of being processed but has not been confirmed.
        /// </summary>
        /// <seealso cref="Cancelled"/>
        CancellationRequested,
        /// <summary>
        /// The archive is marked for cancellation, but a request has not been submitted yet and has not been cancelled yet.
        /// </summary>
        /// <seealso cref="Cancelled"/>
        /// <seealso cref="CancellationRequested"/>
        CancellationPending,
        /// <summary>
        /// The archive has been processed but failed.
        /// </summary>
        Failed
    }
}