using DAZ_Installer.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Windows.DP
{
    /// <summary>
    /// A class that represents an extraction job to process archives.
    /// </summary>
    /// <remarks>
    /// This class is used to interop with the UI and the processor to process archives.
    /// </remarks>
    public interface IDPExtractJob
    {
        /// <summary>
        /// The processor to use for processing archives.
        /// </summary>
        public IDPProcessor Processor { get; }

        /// <summary>
        /// The initial files to process.
        /// </summary>
        public string[] InitialFilesToProcess { get; }

        /// <summary>
        /// The <see cref="Task"/> object used for processing the job.
        /// </summary>
        /// <remarks>
        /// Use this for async operations or checking the status of the job. 
        /// This will be null if the job has not been started.
        /// <br/>
        /// Additionally, the job may not run immediately. It may be queued for processing.
        /// </remarks>
        public Task? TaskJob { get; }

        /// <summary>
        /// Adds the job to the queue to be processed.
        /// </summary>
        /// <returns>The Task object</returns>
        public Task DoJob();

        /// <summary>
        /// Cancels processing current and pending archives.
        /// </summary>
        /// <seealso cref="CancelCurrentArchive"/>
        /// <seealso cref="SkipArchive(string)"/>
        public void CancelJob();

        /// <summary>
        /// Cancels the current archive being processed.
        /// </summary>
        /// <seealso cref="CancelJob"/>
        /// <seealso cref="SkipArchive(string)"/>
        public void CancelCurrentArchive();

        /// <summary>
        /// Skips/cancels the archive from being processed.
        /// </summary>
        /// <remarks>
        /// If the archive has not yet been processed, it will be skipped.
        /// If the archive has been processed, it will not be skipped.
        /// If the archive is currently in process, a cancellation request will be issued.
        /// </remarks>
        /// <param name="archivePath">The path of the archive to skip/cancel.</param>
        /// <exception cref="ArgumentException">The archive is not the list of files to process</exception>
        public void SkipArchive(string archivePath);

        /// <summary>
        /// Gets an immutable snapshot of the current archive infos.
        /// </summary>
        /// <remarks>
        /// This method creates a new copy of the dictionary each time it's called.
        /// For performance-critical code, cache the result if you need to access it multiple times.
        /// </remarks>
        /// <returns>An immutable dictionary containing the current state of all archive infos.</returns>
        public IImmutableDictionary<string, DPArchiveInfo> GetArchiveInfosSnapshot();
    }
}
