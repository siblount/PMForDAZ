using System;
using System.Collections.Generic;
using DAZ_Installer.Core;
using DAZ_Installer.Core.Extraction;

namespace DAZ_Installer.Windows.DP {
    /// <summary>
    /// Demonstrates the current status of an archive being processed.
    /// </summary>
    /// <seealso cref="DPExtractJob"/>
    public class DPArchiveInfo
    {
        /// <summary>
        /// A record struct for holding error information about an archive during processing.
        /// </summary>
        /// <param name="Exception">The exception that was thrown, if any.</param>
        /// <param name="Explanation">An sole or additional explanation for the error, if any.</param>
        public record struct ErrorInfo(Exception? Exception, string? Explanation);
        /// <summary>
        /// The path of the archive.
        /// </summary>
        public string FilePath { get; init; }
        /// <summary>
        /// The status of the archive.
        /// </summary>
        public DPArchiveStatus Status { get; set; }
        /// <summary>
        /// A list of any errors with possible exception and/or explanations.
        /// </summary>
        public List<ErrorInfo> Errors { get; init; } = [];
        /// <summary>
        /// The archive object, if one exists.
        /// </summary>
        /// <remarks>
        /// It is possible for this to be null if the archive is cancelled before the processor
        /// is able to provide an archive file for this. This should not be null for nested
        /// archives and processed (regardless if it failed or not) 
        public IDPArchive? Archive { get; set; }

        /// <summary>
        /// Create an archive info object with archive.
        /// </summary>
        /// <param name="archive"></param>
        public DPArchiveInfo(IDPArchive archive)
        {
            Archive = archive;
            FilePath = archive.Path;
        }

        /// <summary>
        /// Create an archive info object with a path.
        /// </summary>
        /// <param name="path">The path to the archive.</param>
        public DPArchiveInfo(string path) => FilePath = path;
    }
}