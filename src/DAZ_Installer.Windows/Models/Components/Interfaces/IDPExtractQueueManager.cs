using System.Collections.Generic;

namespace DAZ_Installer.Windows.DP
{
    /// <summary>
    /// Manages the queue of archives to be extracted.
    /// </summary>
    public interface IDPExtractQueueManager
    {
        /// <summary>
        /// Adds an archive to the queue.
        /// </summary>
        /// <param name="filePath">The file path of the archive to add.</param>
        /// <param name="groupKey">The group key to set for the archive.</param>
        void AddArchiveToQueue(string filePath, string groupKey);
        /// <summary>
        /// Updates the status of an archive.
        /// </summary>
        /// <param name="filePath">The file path of the archive to update.</param>
        /// <param name="status">The current status of the archive.</param>
        /// <param name="errors">Any errors identified</param>
        void UpdateArchiveStatus(string filePath, DPArchiveStatus status, IEnumerable<DPArchiveInfo.ErrorInfo> errors);
        /// <summary>
        /// Clears the queue of all archives.
        /// </summary>
        void Clear();
    }
}
