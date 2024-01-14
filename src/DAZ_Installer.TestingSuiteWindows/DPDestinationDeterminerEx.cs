using DAZ_Installer.Core;
using DAZ_Installer.Core.Extraction;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.TestingSuiteWindows
{
    /// <summary>
    /// A destination determiner that uses <see cref="DPDestinationDeterminer"/> to determine the destination of each file in the archive.
    /// The only difference here is that it extracts the meta files (such as manifest files) to temp before calling the base method.
    /// </summary>
    internal class DPDestinationDeterminerEx : DPDestinationDeterminer
    {
        private new ILogger Logger { get; } = Log.ForContext<DPDestinationDeterminerEx>();
        public DPDestinationDeterminerEx() : base() { }
        public override HashSet<DPFile> DetermineDestinations(DPArchive arc, DPProcessSettings settings)
        {
            // First, we need to extract the meta files to temp.
            // In DPDestinationDeterminer, it does NOT extract any files. If the file is not on disk, it will not read Manifest.dsx files.
            // So, we need to extract the meta files to temp.
            ReadMetaFiles(arc, ref settings);

            // Now, we can call the base method.
            return base.DetermineDestinations(arc, settings);
        }

        /// <summary>
        /// Reads the files listed in <see cref="DPArchive.DSXFiles"/>.
        /// </summary>
        private void ReadMetaFiles(DPArchive arc, ref DPProcessSettings settings)
        {
            // Extract the DAZ Files that have not been extracted.
            var extractSettings = new DPExtractSettings(settings.TempPath,
                arc!.DSXFiles.Where((f) => f.FileInfo is null || !f.FileInfo.Exists),
                true, arc);
            arc.ExtractContentsToTemp(extractSettings);
            Stream? stream = null!;
            foreach (DPDSXFile file in arc!.DSXFiles)
            {
                using (LogContext.PushProperty("File", file.Path))
                // If it did not extract correctly we don't have acces, just skip it.
                if (file.FileInfo is null || !file.FileInfo.Exists)
                {
                    Logger.Warning("FileInfo was null or returned does not exist, skipping file to read meta data", file.Path);
                    Logger.Debug("FileInfo is null: {0}, FileInfo exists: {1}", file.FileInfo is null, file?.FileInfo?.Exists);
                    continue;
                }
                try
                {
                    if (!file.FileInfo!.TryAndFixOpenRead(out stream, out Exception? ex))
                    {
                        Logger.Error(ex, "Failed to open read stream for file for reading meta");
                        continue;
                    }
                    if (stream is null)
                    {
                        Logger.Error("OpenRead returned successful but also returned null stream, skipping meta read");
                        continue;
                    }
                    if (stream.ReadByte() == 0x1F && stream.ReadByte() == 0x8B)
                    {
                        // It is gzipped compressed.
                        stream.Seek(0, SeekOrigin.Begin);
                        using var gstream = new GZipStream(stream, CompressionMode.Decompress);
                        using var streamReader = new StreamReader(gstream, Encoding.UTF8, true);
                        file.CheckContents(streamReader);
                    }
                    else
                    {
                        // It is normal text.
                        stream.Seek(0, SeekOrigin.Begin);
                        using var streamReader = new StreamReader(stream, Encoding.UTF8, true);
                        file.CheckContents(streamReader);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Unable to read contents of {file.Path}");
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }
    }
}
