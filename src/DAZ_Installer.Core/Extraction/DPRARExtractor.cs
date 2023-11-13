using DAZ_Installer.External;
using DAZ_Installer.IO;
using Serilog;
using Serilog.Context;

namespace DAZ_Installer.Core.Extraction
{
    public class DPRARExtractor : DPAbstractExtractor
    {
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DPRARExtractor>();
        internal IRARFactory Factory { get; init; } = new RARFactory();
        private Session? session = null;
        private class Session
        {
            public DPExtractSettings settings;
            public DPExtractionReport report = new();
            public bool tempOnly = false;
        }

        public DPRARExtractor() { }
        /// <summary>
        /// Constructor used for testing
        /// </summary>
        /// <param name="factory">The factory to use for creating <see cref="RAR"/> handlers.</param>
        internal DPRARExtractor(ILogger logger, IRARFactory factory) => (Logger, Factory) = (logger, factory);
        #region Event Methods

        private void HandleMissingVolume(IRAR sender, MissingVolumeEventArgs e)
        {
            var msg = $"{sender.CurrentFile.FileName} is missing volume : {e.VolumeName}.";
            Logger.Warning(msg);
            var args = new DPArchiveErrorArgs(session.settings.Archive, null, msg);
            EmitOnArchiveError(session.settings.Archive, args);
        }


        public void HandleNewFile(IRAR sender, NewFileEventArgs e)
        {
            try
            {
                if (e.fileInfo.IsDirectory)
                {
                    if (session.settings.Archive.FolderExists(e.fileInfo.FileName)) return;
                    var f = new DPFolder(e.fileInfo.FileName, session.settings.Archive, null);
                    Logger.Debug("Discovered new directory: {0}", f.FileName);
                }
                else
                {
                    var f = DPFile.CreateNewFile(e.fileInfo.FileName, session.settings.Archive, null);
                    Logger.Debug("Discovered new file: {0}", f.FileName);
                }
            } catch (Exception ex)
            {
                Logger.Error(ex, "Unexpected error occurred while handling new file: {0}", e.fileInfo.FileName);
            }
        }


        #endregion
        #region Override Methods
        public override DPExtractionReport ExtractToTemp(DPExtractSettings settings)
        {
            Logger.Information("Extracting to temp");
            session = new Session() { settings = settings, report = new DPExtractionReport(), tempOnly = true };
            try
            {
                return Extract(settings);
            } catch { throw; }
            finally { session = null; }
        }
        public override DPExtractionReport Extract(DPExtractSettings settings)
        {
            using var _ = LogContext.PushProperty("Archive", settings.Archive.FileName);
            Logger.Information("Preparing to extract");
            mode = Mode.Extract;
            EmitOnExtracting();
            FileSystem = settings.Archive.FileSystem;
            DPArchive arc = settings.Archive;
            session ??= new Session() { report = new DPExtractionReport(), settings = settings };
            CancellationToken = settings.CancelToken;

            var report = new DPExtractionReport()
            {
                Settings = settings,
                ExtractedFiles = new(settings.FilesToExtract.Count),
            };
            session.report = report;
            if (CancellationToken.IsCancellationRequested) return report;
            if (arc.Contents.Count == 0) Peek(arc);

            if (arc.FileInfo is null || !arc.FileInfo.Exists)
            {
                handleError(arc, DPArchiveErrorArgs.ArchiveDoesNotExistOrNoAccessExplanation, report, null, null);
                Logger.Debug("FileInfo is null: {0} | FileInfo.Exists: {1}", arc.FileInfo is null, arc.FileInfo?.Exists);
                EmitOnExtractFinished();
                return report;
            }

            using (var RARHandler = Factory.Create(arc.FileInfo.Path))
            {
                try
                {
                    // TODO: Update destination path.
                    RARHandler.Open(RAR.OpenMode.Extract);
                    var flags = (RAR.ArchiveFlags)RARHandler.ArchiveData.Flags;
                    var isFirstVolume = flags.HasFlag(RAR.ArchiveFlags.FirstVolume);
                    var isVolume = flags.HasFlag(RAR.ArchiveFlags.Volume);
                    
                    if (isVolume && !isFirstVolume) 
                        handleError(arc, "The archive is not the first volume out of a multi-volume archive. Only input the first volume", null, null, null);
                    for (var i = 0; i < settings.FilesToExtract.Count && RARHandler.ReadHeader(); i++)
                    {
                        CancellationToken.ThrowIfCancellationRequested();
                        var arcHasFile = arc.Contents.TryGetValue(PathHelper.NormalizePath(RARHandler.CurrentFile.FileName), out var file);
                        if (!RARHandler.CurrentFile.IsDirectory && arcHasFile && file?.AssociatedArchive == arc)
                        {
                            if (!ExtractFile(RARHandler, settings, report))
                                RARHandler.Skip();
                            EmitOnExtractionProgress(settings.Archive, new DPExtractProgressArgs((byte)((float)i / settings.FilesToExtract.Count), arc, file));
                        }
                        else
                        {
                            if (arcHasFile && file!.AssociatedArchive != arc)
                                handleError(arc, string.Format(DPArchiveErrorArgs.FileNotPartOfArchiveErrorFormat, file.Path), report, file, null);
                            i--;
                        }

                    }
                    RARHandler.Close();
                }
                catch (Exception e)
                {
                    handleError(arc, "An unexpected error occured while processing the archive", null, null, e);
                }
            }
            EmitOnExtractFinished();
            return report;
        }
        public override void Peek(DPArchive arc)
        {
            using var _ = LogContext.PushProperty("Archive", arc.FileName);
            Logger.Information("Preparing to peek");
            mode = Mode.Peek;
            FileSystem = arc.FileSystem;
            EmitOnPeeking();
            if (arc.FileInfo is null || !arc.FileInfo.Exists)
            {
                EmitOnArchiveError(arc, new DPArchiveErrorArgs(arc, null, DPArchiveErrorArgs.ArchiveDoesNotExistOrNoAccessExplanation));
                EmitOnPeekFinished();
                return;
            }
            using var RARHandler = Factory.Create(arc.FileInfo.Path);
            RARHandler.MissingVolume += HandleMissingVolume;
            RARHandler.NewFile += HandleNewFile;
            session = new Session() { report = new DPExtractionReport(), settings = new() { Archive = arc, } };

            try
            {
                CancellationToken.ThrowIfCancellationRequested();
                // TODO: Can we remove this?
                RARHandler.DestinationPath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(arc.Path));
                // Create path and see if it exists.
                var dir = FileSystem.CreateDirectoryInfo(RARHandler.DestinationPath);
                if (!dir.PreviewCreate()) Logger.Warning("The current destination directory is not whitelisted and will not be created");
                dir.TryCreate();

                RARHandler.Open(RAR.OpenMode.List);
                var flags = (RAR.ArchiveFlags)RARHandler.ArchiveData.Flags;
                var isFirstVolume = flags.HasFlag(RAR.ArchiveFlags.FirstVolume);
                var isVolume = flags.HasFlag(RAR.ArchiveFlags.Volume);

                if (isVolume && !isFirstVolume)
                {
                    handleError(arc, "The archive is not the first volume out of a multi-volume archive. Only input the first volume.", null, null, null);
                    return;
                }

                while (RARHandler.ReadHeader())
                {
                    if (RARHandler.CurrentFile.IsDirectory) continue;
                    CancellationToken.ThrowIfCancellationRequested();
                    TestFile(RARHandler, arc);
                }
                RARHandler.Close();
            }
            catch (Exception e)
            {
                handleError(arc, "An unexpected error occured while processing the archive.", null, null, e);
            }
            EmitOnPeekFinished();
        }
        #endregion
        private bool ExtractFile(IRAR handler, DPExtractSettings settings, DPExtractionReport report)
        {
            var fileName = handler.CurrentFile.FileName;
            DPArchive arc = settings.Archive;
            
            // Means that archive was modified while we were extracting.
            if (!arc.Contents.TryGetValue(PathHelper.NormalizePath(fileName), out var file))
            {
                handleError(arc, DPArchiveErrorArgs.FileNotPartOfArchiveErrorFormat, report, file, null);
                return false;
            }
            IDPFileInfo? fileInfo = null!;
        EXTRACT:
            try
            {
                var targetPath = session!.tempOnly ? Path.Combine(settings.TempPath,
                                                                 Path.GetFileNameWithoutExtension(arc.FileName),
                                                                 fileName) : file.TargetPath;
                handler.DestinationPath = Path.GetDirectoryName(targetPath)!;
                // Create folders for the destination path if needed.
                var dir = FileSystem.CreateDirectoryInfo(handler.DestinationPath);
                if (!dir.Exists && !dir.TryCreate())
                {
                    handleError(arc, DPArchiveErrorArgs.UnauthorizedAccessExplanation, report, file, null);
                    return false;
                }
                
                fileInfo ??= FileSystem.CreateFileInfo(targetPath);
                if (!FileSystem.Scope.IsFilePathWhitelisted(targetPath)) 
                    throw new OutOfScopeException(targetPath);
                handler.Extract(targetPath);

                // Only update if we didn't error.
                file.FileInfo = fileInfo;
                report.ExtractedFiles.Add(file);
            }
            catch (IOException e)
            {
                var msg = string.Empty;
                if (e.Message == "File CRC Error" || e.Message == "File could not be opened.")
                {
                    var flags = (RAR.ArchiveFlags)handler.ArchiveData.Flags;
                    var isVolume = flags.HasFlag(RAR.ArchiveFlags.Volume);
                    var continuesNext = handler.CurrentFile.ContinuedOnNext;
                    var isEncrypted = handler.CurrentFile.encrypted;
                    msg = (!isVolume || !continuesNext) && !isEncrypted ? "Archive could be corrupt (or simply not an RAR file)." : string.Empty;
                    handleError(arc, msg, report, file, e);
                }
                // Check to see if we are attempting to overwrite a file that we don't have access to (ex: hidden/read-only/anti-virus/user no access).
                else if (e.Message == "File write error." || e.Message == "File read error." || e.Message == "File could not be opened.")
                {
                    fileInfo ??= FileSystem.CreateFileInfo(file.TargetPath);
                    if (fileInfo.TryAndFixOpenRead(out var stream, out Exception? ex))
                    {
                        stream?.Dispose();
                        goto EXTRACT;
                    }
                    else
                    {
                        msg = fileInfo.Exists ? "File could not be extracted and cannot overwrite existing file on disk due to file permissions." :
                            DPArchiveErrorArgs.UnauthorizedAccessAfterExplanation;
                        handleError(arc, msg, report, file, new AggregateException(e, ex));
                    }
                }
                return false;
            } catch (OutOfScopeException e)
            {
                handleError(arc, $"Extractor attempted to extract file to a destination that was not specified: {file.TargetPath}", report, file, e);
                return false;
            } catch (Exception e)
            {
                handleError(arc, "An unexpected error occured while processing the archive.", report, file, e);
                return false;
            }
            return true;
        }

        private bool TestFile(IRAR handler, DPArchive arc)
        {
            try
            {
                // I'm not sure if UnpackedSize returns negative if the file is partial.
                arc.TrueArchiveSize += (ulong)Math.Max(0, handler.CurrentFile.UnpackedSize);
                handler.Test();
            }
            catch (IOException e)
            {
                if (e.Message == "File CRC Error" || e.Message == "File could not be opened.")
                {
                    var flags = (RAR.ArchiveFlags)handler.ArchiveData.Flags;
                    var isVolume = flags.HasFlag(RAR.ArchiveFlags.Volume);
                    var continuesNext = handler.CurrentFile.ContinuedOnNext;
                    var isEncrypted = handler.CurrentFile.encrypted;
                    var msg = (!isVolume || !continuesNext) && !isEncrypted ? "Archive could be corrupt (or simply not an RAR file)." : string.Empty;
                    var associatedDPFile = arc.Contents.TryGetValue(PathHelper.NormalizePath(handler.CurrentFile.FileName), out var file) ? file : null;
                    handleError(arc, msg, null, associatedDPFile, e);
                    return false;
                }
            }
            return true;
        }
        private void handleError(DPArchive arc, string msg, DPExtractionReport? report, DPFile? file, Exception? e)
        {
            using var _ = LogContext.PushProperty("Archive", arc.FileName);
            Logger.Error(e, msg);
            EmitOnArchiveError(arc, new DPArchiveErrorArgs(arc, e, msg));
            if (file is not null && report is not null)
                report.ErroredFiles.Add(file, msg);
        }
    }
}
