
using DAZ_Installer.IO;
using Serilog;
using Serilog.Context;

namespace DAZ_Installer.Core.Extraction
{
    /// <summary>
    /// An extractor for zip archives.
    /// </summary>
    public class DPZipExtractor : DPAbstractExtractor
    {
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPZipExtractor>();
        internal virtual IZipArchiveFactory Factory { get; init; } = new ZipArchiveWrapperFactory();
        private bool tempOnly = false;

        public DPZipExtractor() { }
        /// <summary>
        /// Constructor used for testing
        /// </summary>
        /// <param name="logger">The logger to use, if any.</param>
        /// <param name="factory">The factory to use if any.</param>
        internal DPZipExtractor(ILogger logger, IZipArchiveFactory factory) => (Logger, Factory) = (logger, factory);

        public override DPExtractionReport Extract(DPExtractSettings settings)
        {
            using var _ = LogContext.PushProperty("Archive", settings.Archive.FileName);
            Logger.Information("Preparing to extract");
            Logger.Debug("Extract(settings) = \n{@Settings}", settings);
            // Let listeners know that we are beginning to extract.
            EmitOnExtracting();
            // Reset any variables if needed.
            FileSystem = settings.Archive.FileSystem;
            // Peek into the archive if needed.
            DPArchive arc = settings.Archive;
            if (arc.Contents.Count == 0) Peek(arc);

            var max = settings.FilesToExtract.Count;
            // Set up the extraction report to return in case of any issues.
            var e = new DPExtractionReport()
            {
                ExtractedFiles = new(max),
                Settings = settings
            };

            // Check if the archive is on disk or we have access to it.
            if (arc.FileInfo is null || !arc.FileInfo.Exists)
            {
                EmitOnArchiveError(arc, new DPArchiveErrorArgs(arc, null, DPArchiveErrorArgs.ArchiveDoesNotExistOrNoAccessExplanation));
                EmitOnExtractFinished();
                return e;
            }
            try
            {
                // Create the zip archive.
                using var zipArc = Factory.Create(arc.FileInfo.OpenRead());

                // Loop through all the files to extract and attempt to extract them.
                var i = 0;
                foreach (DPFile file in settings.FilesToExtract)
                {
                    // Check if the file is part of this archive, if not, emit an error and continue.
                    if (file.AssociatedArchive != settings.Archive)
                    {
                        HandleError(arc, file, e, null, string.Format(DPArchiveErrorArgs.FileNotPartOfArchiveErrorFormat, file.Path));
                        Log.Debug("File {0} Associated Archive: {1}", file.FileName, file.AssociatedArchive?.Path);
                        continue;
                    }
                    // Extract the file.
                    ExtractFile(zipArc.GetEntry(file.Path), file, settings, e);
                    HandleProgressionZIP(file, ++i, max);
                }
            } catch (Exception ex)
            {
                HandleError(arc, null, e, ex, "An unknown error occured while attempting to extract the archive");
            }
            EmitOnExtractFinished();
            Logger.Information("Finished extracting");
            return e;
        }

        public override DPExtractionReport ExtractToTemp(DPExtractSettings settings)
        {
            tempOnly = true;
            try
            {
                return Extract(settings);
            } catch
            {
                throw;
            } finally
            {
                tempOnly = false;
            }
        }

        public override void Peek(DPArchive arc)
        {
            using var _ = LogContext.PushProperty("Archive", arc.FileName);
            Logger.Information("Preparing to peek");
            // Emit that we are peeking.
            EmitOnPeeking();
            // Reset any variables if needed.
            arc.TrueArchiveSize = 0;
            FileSystem = arc.FileSystem;

            if (arc.FileInfo is null || !arc.FileInfo.Exists)
            {
                HandleError(arc, null, null, null, DPArchiveErrorArgs.ArchiveDoesNotExistOrNoAccessExplanation);
                Logger.Debug("FileInfo is null: {0} | FileInfo.Exists: {1}", arc.FileInfo is null, arc.FileInfo?.Exists);
                EmitOnPeekFinished();
                return;
            }
            try
            {
                using var zipArc = Factory.Create(arc.FileInfo.OpenRead());
                foreach (var entry in zipArc.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name) && !arc.FolderExists(entry.FullName))
                        // Set folder to null to let it automatically generate subfolders.
                        new DPFolder(entry.FullName, arc, null);
                    else DPFile.CreateNewFile(entry.FullName, arc, null);
                    arc.TrueArchiveSize += (ulong)Math.Max(0, entry.Length);
                }
            } catch (Exception ex)
            {
                HandleError(arc, null, null, ex, "An unknown error occured while attempting to peek the archive");
            }
            Logger.Information("Finished peeking");
            EmitOnPeekFinished();
        }

        private void ExtractFile(IZipArchiveEntry? entry, DPFile file, DPExtractSettings settings, DPExtractionReport report)
        {
            var tryAgain = false;
            DPArchive arc = settings.Archive;
            if (entry is null)
            {
                HandleError(arc, file, report, null, string.Format(DPArchiveErrorArgs.FileNotPartOfArchiveErrorFormat, file.FileName));
                return;
            }

            var expectedPath = tempOnly ? Path.Combine(settings.TempPath, Path.GetFileNameWithoutExtension(arc.Path), entry.Name) : file.TargetPath;
            if (string.IsNullOrWhiteSpace(expectedPath))
            {
                HandleError(arc, file, report, null, "Cannot perform extraction on empty target path");
                return;
            }
            IDPFileInfo? fileInfo = null!;
        EXTRACT:
            try
            {
                var dirInfo = FileSystem.CreateDirectoryInfo(Path.GetDirectoryName(expectedPath)!);
                if (!dirInfo.Exists && !dirInfo.TryCreate()) Logger.Warning("Failed to create directory for {0}", dirInfo.Path);
                // Extract the file and create the file info if it successfully extracted.
                fileInfo ??= FileSystem.CreateFileInfo(expectedPath);
                if (!FileSystem.Scope.IsFilePathWhitelisted(expectedPath))
                {
                    HandleError(arc, file, report, null, $"{expectedPath} is not whitelisted.");
                    return;
                }
                entry.ExtractToFile(expectedPath, settings.OverwriteFiles);
                file.FileInfo = fileInfo;
                report.ExtractedFiles.Add(file);
            }
            catch (IOException e)
            {
                if (e.Message.StartsWith("The file ") && e.Message.EndsWith("already exists"))
                    HandleError(arc, file, report, e, "Attempted to overwrite a file that exists but user chose not to overwrite files");
                else HandleError(arc, file, report, e, "Unknown error occurred while extracting file");
            }
            // Note: System.UnauthorizedAccessException can occur when zip is attempting to overwrite a hidden and/or read-only file.
            catch (UnauthorizedAccessException e)
            {
                if (tryAgain)
                {
                    HandleError(arc, file, report, e, DPArchiveErrorArgs.UnauthorizedAccessAfterExplanation);
                    return;
                }
                if (!TryHelper.TryFixFilePermissions(fileInfo ??= FileSystem.CreateFileInfo(expectedPath), out Exception? ex))
                    HandleError(arc, file, report, new AggregateException(e, ex), DPArchiveErrorArgs.UnauthorizedAccessExplanation);
                // Try it again.
                tryAgain = true;
                goto EXTRACT;
            }
            catch (Exception e)
            {
                HandleError(arc, file, report, e, "Unknown error occurred while extracting file");
            }
        }

        private void HandleProgressionZIP(DPFile file, int i, int max)
        {
            i = Math.Min(i, max);
            var percentComplete = (float)i / max;
            var progress = (byte)Math.Floor(percentComplete * 100);
            DPArchive arc = file.AssociatedArchive!;
            EmitOnExtractionProgress(arc, new DPExtractProgressArgs(progress, arc, file));
        }

        private void HandleError(DPArchive arc, DPFile? file, DPExtractionReport? report, Exception? e, string msg)
        {
            Logger.Error(e, msg);
            EmitOnArchiveError(arc, new DPArchiveErrorArgs(arc, e, msg));
            if (file is not null && report is not null)
            report.ErroredFiles.TryAdd(file, msg);
        }
    }
}
