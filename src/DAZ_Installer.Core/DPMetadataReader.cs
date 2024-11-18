using Serilog;
using Serilog.Context;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Core
{
    /// <summary>
    /// The default implementation of <see cref="IDPMetadataReader"/>, used to read metadata from .dsx files.
    /// </summary>
    public sealed class DPMetadataReader : IDPMetadataReader
    {
        /// <summary>
        /// A singleton instance of <see cref="DPMetadataReader"/>.
        /// </summary>
        /// <remarks>
        /// You should use this instance to interact with the class versus creating a new instance.
        /// </remarks>
        public readonly static DPMetadataReader Instance = new();
        public ILogger Logger { get; set; } = Log.Logger.ForContext<DPMetadataReader>();
        /// <summary>
        /// Attempts to read the metadata of the files on disk and calls <see cref="IDPDSXFile.CheckContents(StreamReader)"/> for each file in <paramref name="files"/>.
        /// </summary>
        /// <remarks>
        /// Supports normal and gzipped compressed file streams. If errors occur, they are logged and the process continues.
        /// </remarks>
        /// <param name="files">The files to read metadata from.</param>
        /// <param name="token">The cancellation token to use; use <see cref="CancellationToken.None"/> if not needed.</param>
        public void ReadMetadata(IEnumerable<IDPDSXFile> files, CancellationToken token)
        {
            Stream? fstream = null!;
            foreach (IDPDSXFile file in files)
            {
                if (token.IsCancellationRequested) return;
                using (LogContext.PushProperty("File", file.Path))
                    // If it did not extract correctly or we don't have access, just skip it.
                    // This is low-priority.
                    if (file.FileInfo is null || !file.FileInfo.Exists)
                    {
                        Logger.Warning("FileInfo was null or returned does not exist, skipping file to read meta data", file.Path);
                        Logger.Debug("FileInfo is null: {0}, FileInfo exists: {1}", file.FileInfo is null, file?.FileInfo?.Exists);
                        continue;
                    }
                try
                {
                    if (!file.FileInfo!.TryAndFixOpenRead(out fstream, out Exception? ex))
                    {
                        Logger.Error(ex, "Failed to open read stream for file for reading meta");
                        continue;
                    }
                    if (fstream is null)
                    {
                        Logger.Error("OpenRead returned successful but also returned null stream, skipping meta read");
                        continue;
                    }
                    using var stream = IsStreamGZipped(fstream) ? new GZipStream(fstream, CompressionMode.Decompress) : fstream;
                    fstream.Seek(0, SeekOrigin.Begin);
                    using var streamReader = new StreamReader(stream, Encoding.UTF8, true);
                    file.CheckContents(streamReader);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to read contents of {0}", file.Path);
                }
                finally
                {
                    fstream?.Dispose();
                }
            }
        }

        private static bool IsStreamGZipped(Stream stream)
        {
            if (stream.ReadByte() == 0x1F && stream.ReadByte() == 0x8B)
            {
                stream.Seek(0, SeekOrigin.Begin);
                return true;
            }
            stream.Seek(0, SeekOrigin.Begin);
            return false;
        }
    }
}
