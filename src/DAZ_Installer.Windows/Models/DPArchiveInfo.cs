using System;
using System.Collections.Generic;
using System.Linq;
using DAZ_Installer.Core;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO;

namespace DAZ_Installer.Windows.DP {
    /// <summary>
    /// Demonstrates the current status of an archive being processed.
    /// </summary>
    /// <seealso cref="DPExtractJob"/>
    public record DPArchiveInfo
    {
        /// <summary>
        /// Provides additional information about the archive
        /// </summary>
        [Flags]
        public enum InfoFlags
        {
            /// <summary>
            /// No additional information.
            /// </summary>
            None,
            /// <summary>
            /// A note that indicates that the archive already had existed according to the DB.
            /// </summary>
            /// <remarks>
            /// This will explain why an archive was cancelled.
            /// </remarks>
            ProductAlreadyExists
        }
        /// <summary>
        /// A record struct for holding error information about an archive during processing.
        /// </summary>
        /// <param name="Exception">The exception that was thrown, if any.</param>
        /// <param name="Explanation">An sole or additional explanation for the error, if any.</param>
        public record struct ErrorInfo(Exception? Exception, string? Explanation);
        /// <summary>
        /// The path of the archive.
        /// </summary>
        /// <remarks>By default, on creation, the path will be normalized using <see cref="PathHelper.NormalizePath(string)"/></remarks>
        public string FilePath;
        /// <summary>
        /// The status of the archive.
        /// </summary>
        /// <remarks>By default, on creation, is <see cref="DPArchiveStatus.Pending"/></remarks>
        public DPArchiveStatus Status;
        /// <summary>
        /// A list of any errors with possible exception and/or explanations.
        /// </summary>
        public IList<ErrorInfo> Errors = [];
        /// <summary>
        /// The archive object, if one exists.
        /// </summary>
        /// <remarks>
        /// It is possible for this to be null if the archive is cancelled before the processor
        /// is able to provide an archive file for this. This should not be null for nested
        /// archives and processed (regardless if it failed or not) 
        /// </remarks>
        public IDPArchive? Archive = null;
        /// <summary>
        /// Additional information about the archive.
        /// </summary>
        public InfoFlags Flags = InfoFlags.None;

        /// <summary>
        /// Create an archive info object with archive.
        /// </summary>
        /// <param name="archive">The archive</param>
        public DPArchiveInfo(IDPArchive archive)
        {
            Archive = archive;
            FilePath = PathHelper.NormalizePath(archive.FileInfo?.Path ?? string.Empty);
            Status = DPArchiveStatus.Pending;
        }

        /// <summary>
        /// Create an archive info object with a path.
        /// </summary>
        /// <remarks>
        /// The <see cref="FilePath"/> property will be the normalized version of <paramref name="path"/>.
        /// </remarks>
        /// <param name="path">The file path on disk of the archive.</param>
        public DPArchiveInfo(string path) => (FilePath, Status) = (PathHelper.NormalizePath(path), DPArchiveStatus.Pending);

        /// <summary>
        /// Creates a new DPArchiveInfo record with the added error info.
        /// </summary>
        /// <param name="errorInfo">A new error info to add</param>
        /// <returns>A new DPArchiveInfo with the added <paramref name="errorInfo"/>.</returns>
        public DPArchiveInfo WithError(ErrorInfo errorInfo) => this with { Errors = [..Errors, errorInfo] };
        /// <summary>
        /// Creates a new DPArchiveInfo record with the <paramref name="flags"/> bitwise or'd to the 
        /// current <see cref="Flags"/>.
        /// </summary>
        /// <param name="flags">The flags to bitwise or with.</param>
        /// <returns>A new DPArchiveInfo with updated flags.</returns>
        public DPArchiveInfo AppendFlags(InfoFlags flags) => this with { Flags = Flags | flags };
        /// <summary>
        /// Returns an enumerable of distinct error messages determined by the exception or explanation message.
        /// </summary>
        /// <param name="errors">The raw error infos from the archive info</param>
        /// <returns>An enumerable of distinct error messages</returns>
        public static IEnumerable<ErrorInfo> GetUniqueErrors(IList<ErrorInfo> errors)
        {
            return errors.GroupBy(e => e.Exception?.Message ?? e.Explanation)
                        .Select(g => g.First());
        }
    }
}