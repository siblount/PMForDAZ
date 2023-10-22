using DAZ_Installer.IO;
using System.Diagnostics;
using Serilog;
using Serilog.Context;


namespace DAZ_Installer.Core.Extraction
{
    public class DP7zExtractor : DPAbstractExtractor
    {
        public override ILogger Logger { get; set; } = Log.Logger.ForContext<DP7zExtractor>();
        internal IProcessFactory Factory { get; init; } = new ProcessFactory();
        private struct Entity
        {
            public string Path;
            public bool isDirectory;

            public bool IsEmpty => Path == null;
        }
        private bool _hasEncryptedFiles = false;
        private bool _hasEncryptedHeader = false;
        private IProcess? _process = null;
        private bool _processHasStarted = false;
        private string _arcPassword = string.Empty;

        // Peek phase variables.
        private bool _peekErrored = false;
        private bool _peekFinished = false;

        // Extract phase variables.
        private bool _extractErrored = false;
        private bool _extractFinished = false;

        // Extract phase variables.
        private bool _moveErrored = false;
        private bool _moveFinished = false;

        private bool _seekingFiles = false;

        private Entity _lastEntity = new() { };
        private DPExtractionReport workingExtractionReport = null!;
        private DPArchive workingArchive = null!;

        // Flag
        private bool tempOnly = false;
        private DPExtractSettings workingSettings => workingExtractionReport.Settings;
        private string tempFolder => Path.Combine(workingSettings.TempPath, Path.GetFileNameWithoutExtension(workingArchive.FileName));

        public DP7zExtractor() { }
        /// <summary>
        /// Constructor for testing
        /// </summary>
        internal DP7zExtractor(ILogger logger, IProcessFactory factory) => (Logger, Factory) = (logger, factory);
        public override DPExtractionReport Extract(DPExtractSettings settings)
        {
            using var _ = LogContext.PushProperty("Archive", settings.Archive.FileName);
            return extractInternal(settings, false);
        }

        public override DPExtractionReport ExtractToTemp(DPExtractSettings settings)
        {
            using var _ = LogContext.PushProperty("Archive", settings.Archive.FileName);
            return extractInternal(settings, true);
        }

        public override void Peek(DPArchive archive)
        {
            using var _ = LogContext.PushProperty("Archive", archive.FileName);
            Reset();
            Logger.Information("Preparing to peek");
            mode = Mode.Peek;
            workingArchive = archive;
            FileSystem = archive.FileSystem;
            EmitOnPeeking();
            _process = Setup7ZProcess();
            if (StartProcess())
            {
                var time = TimeSpan.FromSeconds(120);
                if (!SpinWait.SpinUntil(() => _peekFinished || CancellationToken.IsCancellationRequested, time))
                    handleError(archive, $"Peek timeout of {time.TotalSeconds} seconds exceeded.", null, null, null);
            }
            KillProcess();
            EmitOnPeekFinished();
            Logger.Information("Peek finished");
        }

        private bool StartProcess()
        {
            Logger.Information("Starting 7z process");
            try
            {
                _process!.Start();
                _processHasStarted = true;
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
                _process.StandardInput.WriteLineAsync(_arcPassword);
            }
            catch (Exception ex)
            {
                handleError(workingArchive, "Failed to start 7z process", null, null, ex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Attempts to kill the process, dispose of it, and set it to null.
        /// </summary>
        private void KillProcess()
        {
            if (_process is not null) Logger.Information("Killing 7z process");
            if (_process is not null && !_process.HasExited) 
                Logger.Warning("7z process is being killed while it is not exited");
            try
            {
                if (_process is null) return;
                _process.Kill(true);
                _process.Dispose();
                _process.OutputDataReceived -= Handle7zOutput;
                _process.ErrorDataReceived -= Handle7zErrors;
            } catch (Exception ex)
            {
                Logger.Error(ex, "Failed to kill 7z process");
            }
            _process = null;
        }

        /// <summary>
        /// Creates a new 7z process object depending on the current mode.
        /// If the current mode is Peek, then it will tell 7z to list contents.
        /// Otherwise, it will tell 7z to extract contents.
        /// </summary>
        /// <returns>A 7z process.</returns>
        private IProcess Setup7ZProcess()
        {
            var process = Factory.Create();
            process.StartInfo.FileName = "7za.exe";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += Handle7zOutput;
            process.ErrorDataReceived += Handle7zErrors;

            if (mode == Mode.Peek)
                process.StartInfo.ArgumentList.Add("l");
            else
                process.StartInfo.ArgumentList.Add("x");
            process.StartInfo.ArgumentList.Add("-aoa"); // Overwrite existing files w/o prompt.
            process.StartInfo.ArgumentList.Add("-slt"); // Show technical information.
            // process.StartInfo.ArgumentList.Add("-bb1"); // Show names of processed files in log.

            process.StartInfo.ArgumentList.Add(workingArchive.FileInfo!.Path);
            return process;
        }

        private void Handle7zErrors(string? data)
        {
            Logger.Debug("7z error output: {output}", data ?? "null");
            if (data is null) _peekErrored = _peekFinished = true;
            ReadOnlySpan<char> msg = data;

            if (msg.Contains("Can not open encrypted archive. Wrong password?"))
                handleError(workingArchive, DPArchiveErrorArgs.EncryptedArchiveExplanation, null, null, null);
            // DPCommon.WriteToLog($"Handle 7z errors called! Msg: {e.Data}");
        }

        /// <summary>
        /// Handles the appropriate action when receiving data from StandardOutput.
        /// </summary>
        private void Handle7zOutput(string? data)
        {
            using var _ = LogContext.PushProperty("Archive", workingArchive.FileName);
            
            Logger.Debug("7z output: {output}", data ?? "null");
            if (CancellationToken.IsCancellationRequested) return;
            if (data == null || _hasEncryptedFiles)
            {
                if (mode == Mode.Peek) _peekFinished = true;
                else _extractFinished = true;

                if (_hasEncryptedFiles)
                {
                    handleError(workingArchive, DPArchiveErrorArgs.EncryptedFilesExplanation, null, null, null);
                    return;
                }
                // Finalize the last 7z content.
                if (data is null && !_lastEntity.IsEmpty)
                {
                    FinalizeEntity();
                    _lastEntity = new Entity { };
                }
                if (tempOnly) finalizeTempOnlyOperation();
                return;
            }
            ReadOnlySpan<char> msg = data;
            if (mode == Mode.Peek)
            {
                if (msg.StartsWith("----------")) _seekingFiles = true;
                if (!_seekingFiles) return;

                if (msg.StartsWith("Path"))
                {
                    if (!_lastEntity.IsEmpty) FinalizeEntity();
                    _lastEntity = new Entity { Path = msg.Slice(7).ToString() };
                }
                else if (msg.StartsWith("Size"))
                {
                    if (ulong.TryParse(msg.Slice(7), out var size))
                        workingArchive.TrueArchiveSize += size;
                }
                else if (msg.StartsWith("Attributes"))
                {
                    ReadOnlySpan<char> attributes = msg.Slice("Attributes = ".Length);
                    _lastEntity.isDirectory = attributes.Contains("D");
                }
                else if (msg.StartsWith("Encrypted"))
                    _hasEncryptedFiles = msg.Contains("+");
                else if (msg.Contains("Errors:"))
                    _peekErrored = true;
            }
            else
            {
                // Only check if everything really did extract if 
                var ok = msg.StartsWith("Everything is Ok");
                var errors = msg.Contains("Errors");
                if (!ok && !errors) return;
                _extractErrored = errors;
                _extractFinished = true;
                if (!tempOnly) MoveFiles();
            }
        }
        /// <summary>
        /// Relocates the files from the temporary directory to their final destination. It also updates their file info.
        /// </summary>
        private void MoveFiles()
        {
            // Let anyone know that we are beginning to move files.
            Logger.Information("Preparing to move files to destination");
            EmitOnMoving();

            var i = 0UL;
            var count = workingSettings.FilesToExtract.Count;
            // For 7z specifically, we need to verify that the files were actually extracted and update their file info at the same time.
            UpdateFileInfos();
            try
            {
                foreach (DPFile file in workingSettings.FilesToExtract)
                {
                    CancellationToken.ThrowIfCancellationRequested();
                    EmitOnMoveProgress(workingArchive, new DPExtractProgressArgs((byte)((float)i / count), workingArchive, file));
                    IDPFileInfo? fileInfo = file.FileInfo;
                    // If the file did not extract to temp or it no longer exists (or we don't have access permissions), then we can't move it so we skip it.
                    if (fileInfo is null || !fileInfo.Exists)
                    {
                        Logger.Warning("{file} skipped due to failed extraction or insufficient permissions", file.Path);
                        Logger.Debug("FileInfo is null: {0} | FileInfo.Exists: {1}", fileInfo is null, fileInfo?.Exists);
                        continue;
                    }
                    // Create the directories needed so moving the file can be successful.
                    var targetDir = FileSystem.CreateDirectoryInfo(file.TargetPath);
                    if (!targetDir.Exists && !targetDir.TryCreate())
                    {
                        handleError(workingArchive, $"Failed to create directory for {file.Path}", file, workingExtractionReport, null);
                        continue;
                    }
                    if (fileInfo.TryAndFixMoveTo(file.TargetPath, true, out var ex)) workingExtractionReport.ExtractedFiles.Add(file);
                    else handleError(workingArchive, "Failed to move file to target location", file, workingExtractionReport, ex);
                }
            }
            catch (Exception ex)
            {
                handleError(workingArchive, "An unknown error occured while attempting to move files to target location", null, null, ex);
                _moveErrored = true;
            }
            _moveFinished = true;
        }

        private void finalizeTempOnlyOperation()
        {
            UpdateFileInfos();
            foreach (DPFile file in workingSettings.FilesToExtract)
            {
                if (file.FileInfo is not null) workingExtractionReport.ExtractedFiles.Add(file);
                else workingExtractionReport.ErroredFiles.Add(file, "Failed to extract file to temp directory");
            }
        }

        /// <summary>
        /// Resets the state of the extractor.
        /// </summary>
        private void Reset()
        {
            _moveErrored = _moveFinished = _extractErrored = _extractFinished = _peekErrored =
                _peekFinished = _seekingFiles = _hasEncryptedFiles = _hasEncryptedHeader = false;
            _lastEntity = new Entity { };
            KillProcess();
            _processHasStarted = false;
            workingExtractionReport = null!;
            workingArchive = null!;
            tempOnly = false;
        }

        /// <summary>
        /// Updates all of the file infos in the archive that did extract. This needs to be called after the extraction is finished.
        /// Since 7z extracts ALL files in the directory, we need to update all of the file infos. For example, if a file that originally wasn't
        /// supposed to be moved to the user's library (e.g. unusual content folder) and now they want to move it, we can simply move the file
        /// from the disk instead of extracting the entire archive again.
        /// </summary>
        private void UpdateFileInfos()
        {
            try
            {
                CancellationToken.ThrowIfCancellationRequested();
                foreach (DPFile file in workingArchive.Contents.Values)
                {
                    if (file.AssociatedArchive != workingArchive)
                    {
                        handleError(workingArchive, string.Format(DPArchiveErrorArgs.FileNotPartOfArchiveErrorFormat, file.Path), file, workingExtractionReport, null);
                        Log.Debug("File {0} Associated Archive: {1}", file.FileName, file.AssociatedArchive?.Path);
                        continue;
                    }
                    var fileInfo = FileSystem.CreateFileInfo(Path.Combine(tempFolder, file.Path));
                    if (fileInfo.Exists) file.FileInfo = fileInfo; // Set the file info even if we did not extract it to it's final dest.
                    else handleError(workingArchive, $"{file.Path} did not extract successfully", file, workingExtractionReport, null);
                }
            }
            catch (Exception ex)
            {
                handleError(workingArchive, "Failed to update file infos in working archive", null, null, ex);
            }
        }

        /// <summary>
        /// FinalizeEntity indicates that the last entity is finished and can make a <see cref="DPFile"/> or a <see cref="DPFolder"/>
        /// It has to be done this way because 7z seperates the attributes within each line and the the callback is called
        /// for each line passed in.
        /// </summary>
        private void FinalizeEntity()
        {
            if (_lastEntity.isDirectory)
                // Setting DPFolder to null will automatically create parent folders if they don't exist or
                // automatically add the folder to the parent folder if it does exist.
                new DPFolder(_lastEntity.Path, workingArchive, null);
            else
                DPFile.CreateNewFile(_lastEntity.Path, workingArchive, null);
        }

        private DPExtractionReport StartExtractionProcess(string tempFolder, bool tempOnly = false)
        {
            _process = Setup7ZProcess();
            _process.StartInfo.ArgumentList.Add("-o" + tempFolder);
            if (CancellationToken.IsCancellationRequested) return workingExtractionReport;
            if (!StartProcess())
            {
                EmitOnExtractFinished();
                return workingExtractionReport;
            }
            var time = TimeSpan.FromSeconds(120);
            var extractSuccessful = false;
            if (!(extractSuccessful = SpinWait.SpinUntil(() => _extractFinished || CancellationToken.IsCancellationRequested, time)))
            {
                handleError(workingArchive, $"Extraction timeout of {time.TotalSeconds} seconds exceeded.", null, null, null);
                KillProcess();
            }
            EmitOnExtractFinished();
            Logger.Information("Extract finished");
            if (!extractSuccessful || CancellationToken.IsCancellationRequested) return workingExtractionReport;

            if (!tempOnly)
            {
                if (!SpinWait.SpinUntil(() => _moveFinished || CancellationToken.IsCancellationRequested, time))
                    handleError(workingArchive, $"Move timeout of {time.TotalSeconds} seconds exceeded.", null, null, null);
                EmitOnMoveFinished();
                Logger.Information("Move finished");
            }
            KillProcess();
            return workingExtractionReport;
        }

        /// <summary>
        /// Logs the error, emits the error event, and adds the file to the report if it is not null.
        /// </summary>
        private void handleError(DPArchive arc, string msg, DPFile? file, DPExtractionReport? report, Exception? ex)
        {
            using var _ = LogContext.PushProperty("Archive", arc.FileName);
            Logger.Error(ex, msg);
            EmitOnArchiveError(arc, new DPArchiveErrorArgs(arc, ex, msg));
            if (file is not null && report is not null)
                report.ErroredFiles.Add(file, msg);
        }


        private DPExtractionReport extractInternal(DPExtractSettings settings, bool extractToTemp)
        {
            Reset();
            Logger.Information(extractToTemp ? "Preparing to extract to temp" : "Preparing to extract");
            Logger.Debug("Extract(settings) = \n{@settings}", settings);
            FileSystem = settings.Archive.FileSystem;
            DPArchive archive = workingArchive = settings.Archive;
            tempOnly = extractToTemp;
            CancellationToken = settings.CancelToken;
            if (archive.Contents.Count == 0)
            {
                Logger.Information("Archive Contents length was 0, now peeking...");
                Peek(archive);
            }
            mode = Mode.Extract;
            EmitOnExtracting();
            // TODO: Log warning if process was interrupted while extracting.
            workingExtractionReport = new DPExtractionReport()
            {
                ExtractedFiles = new(settings.FilesToExtract.Count),
                Settings = settings
            };
            if (CancellationToken.IsCancellationRequested) return workingExtractionReport;
            if (archive.FileInfo is null || !archive.FileInfo.Exists)
            {
                handleError(archive, DPArchiveErrorArgs.ArchiveDoesNotExistOrNoAccessExplanation, null, null, null);
                EmitOnExtractFinished();
                return workingExtractionReport;
            }
            var tempDir = FileSystem.CreateDirectoryInfo(tempFolder);
            if (!tempDir.Exists && !TryHelper.Try(() => tempDir.Create(), out var ex))
            {
                handleError(archive, "Failed to create required temp directories for extraction operations", null, null, ex);
                return workingExtractionReport;
            }
            if (CancellationToken.IsCancellationRequested) return workingExtractionReport;
            StartExtractionProcess(tempFolder, extractToTemp);
            return workingExtractionReport;
        }

    }
}
