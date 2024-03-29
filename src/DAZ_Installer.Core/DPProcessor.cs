﻿// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core.Extraction;

using DAZ_Installer.IO;
using Serilog;
using Serilog.Context;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace DAZ_Installer.Core
{
    // GOAL: Extract files through RAR. While it discovers files, add it to list.
    // Then, deeply analyze each file; determine best approach; and execute best approach (or ask).
    // Lastly, clean up.
    public class DPProcessor
    {
        // SecureString - System.Security
        // We use these variables in case the user changes the settings in mist of an extraction process
        public static readonly ImmutableDictionary<string, string> DefaultRedirects = ImmutableDictionary.Create<string, string>(StringComparer.OrdinalIgnoreCase)
                                                                                                         .AddRange(new KeyValuePair<string, string>[]{ new("docs", "Documentation"),
                                                                                                                                                       new("Documents", "Documentation"),
                                                                                                                                                       new("Readme", "Documentation"),
                                                                                                                                                       new("ReadMe's", "Documentation"),
                                                                                                                                                       new("Readmes", "Documentation"),
                                                                                                                                                       new("Transport", "Vehicles"),
                                                                                                                                                       new("Scene", "Scenes")});
        public static readonly ImmutableHashSet<string> DefaultContentFolders = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase)
                                                                                                .Union(new[] {"aniBlocks", "Animals", "Architecture", "Camera Presets", "data", "DAZ Studio Tutorials", "Documentation", "Documents",
                                                                                                              "Environments", "General", "Light Presets", "Lights", "People", "Presets", "Props", "Render Presets", "Render Settings", "Runtime",
                                                                                                              "Scene Builder", "Scene Subsets", "Scenes", "Scripts", "Shader Presets", "Shaders", "Support", "Templates", "Textures", "Vehicles" });
        public DPProcessSettings CurrentProcessSettings { get; private set; } = new();
        public ILogger Logger { get; set; } = Log.Logger.ForContext<DPProcessor>();
        public AbstractFileSystem FileSystem { get; set; } = new DPFileSystem();
        public AbstractTagProvider TagProvider { get; set; } = new DPTagProvider();
        public AbstractDestinationDeterminer DestinationDeterminer { get; set; } = new DPDestinationDeterminer();
        private CancellationTokenSource CancellationTokenSource { get; set; } = new();
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;
        private CancellationTokenSource ArchiveCancellationSource { get; set; } = new();
        private CancellationToken ArchiveCancellationToken { get; set; } = CancellationToken.None;
        private bool ArchiveCancelled => ArchiveCancellationToken.IsCancellationRequested || CancellationToken.IsCancellationRequested;
        public string TempLocation => Path.Combine(CurrentProcessSettings.TempPath, @"DazProductInstaller\");
        public string DestinationPath => CurrentProcessSettings.DestinationPath;
        public DPArchive CurrentArchive { get; private set; } = null!;
        public ProcessorState State { get => state; private set { state = value; StateChanged?.Invoke(); } }

        /// <summary>
        /// An event that is invoked when a file that is being extracted, moved, or deleted throws an error.
        /// <seealso cref="ProcessError"/>
        /// </summary>
        public event DPProcessorEventHandler<DPErrorArgs>? FileError;
        /// <summary>
        /// An event that is invoked when a
        /// </summary>
        public event DPProcessorEventHandler<DPProcessorErrorArgs>? ProcessError;
        public event DPProcessorEventHandler<DPArchiveEnterArgs>? ArchiveEnter;
        public event DPProcessorEventHandler<DPArchiveExitArgs>? ArchiveExit;
        public event DPProcessorEventHandler<DPExtractProgressArgs>? ExtractProgress;
        public event DPProcessorEventHandler<DPExtractProgressArgs>? MoveProgress;
        public event Action? Finished;
        public event Action? StateChanged;
         
        private ProcessorState state;
        // public event FilePreMove

        /// <summary>
        /// Emits the <see cref="ArchiveEnter"/> event and passes the arguments required. <para/>
        /// References <paramref name="cancel"/> to determine whether <see cref="DPProcessor"/> should
        /// cancel operations or not.
        /// </summary>
        private void EmitOnArchiveEnter()
        {
            Logger.Information("Entering archive {arc}", CurrentArchive.FileName);
            if (ArchiveEnter is null) return;
            var args = new DPArchiveEnterArgs(CurrentArchive);
            ArchiveEnter.Invoke(this, args);
        }
        /// <summary>
        /// Emits the <see cref="ArchiveExit"/> event and passes the arguments required. <para/>
        /// </summary>
        /// <param name="successfullyProcessed">Tell whether the archive had been successfully processed.</param>
        private void EmitOnArchiveExit(bool successfullyProcessed, DPExtractionReport? report)
        {
            if (successfullyProcessed) Logger.Information("Exiting archive {0} with success", CurrentArchive.FileName);
            else Logger.Warning("Exiting archive {0} with failures", CurrentArchive.FileName);
            Logger.Debug("Archive exit report: {@0}", report);
            ArchiveExit?.Invoke(this, new DPArchiveExitArgs(CurrentArchive, report, successfullyProcessed));
        }

        private void EmitOnProcessError(DPProcessorErrorArgs args)
        {
            Logger.Error(args.Ex, args.Explaination);
            if (ProcessError is null) return;
            ProcessError.Invoke(this, args);
        }

        private void EmitOnExtractionProgress(DPArchive _, DPExtractProgressArgs args) => ExtractProgress?.Invoke(this, args);

        private void processArchiveInternal([NotNull] DPArchive archiveFile, DPProcessSettings settings)
        {
            Stack<DPArchive> archivesToProcess = new();
            Stack<Tuple<DPArchive, DPExtractionReport>> parentArchives = new();

            archivesToProcess.Push(archiveFile);
            CancellationTokenSource = new();
            CancellationToken = CancellationTokenSource.Token;
            while (archivesToProcess.TryPop(out DPArchive arc))
            {
                CurrentArchive = arc!;
                DPExtractionReport? report = null;
                PopParentArchive(parentArchives);
                ArchiveCancellationSource = new();
                ArchiveCancellationToken = ArchiveCancellationSource.Token;
                EmitOnArchiveEnter();
                try
                {
                    using (LogContext.PushProperty("Archive", arc.FileName))
                        Logger.Information("Processing archive");
                    var arcDebugInfo = new
                    {
                        Name = arc.FileName,
                        NestedArchive = arc.IsInnerArchive,
                        Path = arc.IsInnerArchive ? arc.Path : arc?.FileInfo?.Path,
                        Extractor = arc.Extractor?.GetType().Name,
                        ParentArchiveNestedArchive = arc?.AssociatedArchive?.IsInnerArchive,
                        ParentArchiveName = arc?.AssociatedArchive?.FileName,
                        ParentArchivePath = arc?.AssociatedArchive?.IsInnerArchive ?? false ? arc?.AssociatedArchive?.Path :
                                                                                              arc?.AssociatedArchive?.FileInfo?.Path,
                        ParentExtractor = arc?.AssociatedArchive?.Extractor?.GetType().Name,
                    };
                    Logger.Debug("Archive that is about to be processed: {@Arc}", arcDebugInfo);
                    if (CancellationToken.IsCancellationRequested) { HandleEarlyExit(); return; }

                    State = ProcessorState.Starting;
                    try
                    {
                        FileSystem.CreateDirectoryInfo(TempLocation).Create();
                    }
                    catch (Exception e)
                    {
                        EmitOnProcessError(new DPProcessorErrorArgs(e, "Unable to create temp directory.") { Continuable = true });
                    }

                    State = ProcessorState.Peeking;
                    if (ArchiveCancelled) { HandleEarlyExit(); return; }
                    if (arc.Extractor is null)
                    {
                        EmitOnProcessError(new DPProcessorErrorArgs(null, "Unable to process archive. Potentially not an archive or archive is corrupted."));
                        HandleEarlyExit();
                        continue;
                    }
                    arc.Extractor.CancellationToken = ArchiveCancellationToken;
                    if (!tryCatch(() => arc!.PeekContents(), "Failed to peek into archive")) continue;

                    // Check if we have enough room.
                    if (!HandleOnDestinationNotEnoughSpace())
                    {
                        HandleEarlyExit();
                        continue;
                    }

                    State = ProcessorState.PreparingExtraction;
                    HashSet<DPFile> filesToExtract = null!;
                    if (!tryCatch(prepareOperations, "Failed to prepare for extraction")) continue;
                    if (ArchiveCancelled) { HandleEarlyExit(); continue; }
                    if (!tryCatch(() => filesToExtract = DestinationDeterminer.DetermineDestinations(arc, settings), "Failed to determine destinations for files")) continue;

                    State = ProcessorState.Extracting;
                    var extractSettings = new DPExtractSettings()
                    {
                        TempPath = CurrentProcessSettings.TempPath,
                        Archive = arc,
                        FilesToExtract = filesToExtract,
                        OverwriteFiles = CurrentProcessSettings.OverwriteFiles,
                        CancelToken = ArchiveCancellationToken,
                    };

                    if (ArchiveCancelled) { HandleEarlyExit(); continue; }
                    if (!tryCatch(() => report = arc.ExtractContents(extractSettings), "Failed to extract contents for archive")) continue;

                    // DPCommon.WriteToLog("We are done");
                    Logger.Information("Analyzing the archive - fetching tags");
                    State = ProcessorState.Analyzing;
                    if (!tryCatch(() => arc.Type = arc.DetermineArchiveType(), "Failed to analyze archive")) continue;
                    if (!tryCatch(() => TagProvider.GetTags(arc, settings), "Failed to get tags for archive")) continue;

                    foreach (var subarc in arc.Subarchives.Where(x => x.Extracted))
                    {
                        archivesToProcess.Push(subarc);
                    }

                    // Create record.
                    parentArchives.Push(new Tuple<DPArchive, DPExtractionReport>(arc, report)); // TODO: Use method to determine whether an archive was successfully processed.
                }
                catch (Exception ex)
                {
                    handleError(ex, "An unexpected error occured while processing archive.");
                    EmitOnArchiveExit(false, report);
                }
            }

            PopParentArchive(parentArchives);

        }

        // TODO: RetryArchive()
        public void ProcessArchive(string filePath, DPProcessSettings settings)
        {
            validateProcessSettings(ref settings);
            CurrentProcessSettings = settings;
            FileSystem.Scope = setupScope(settings);
            // Create new archive.
            var archiveFile = DPArchive.CreateNewParentArchive(FileSystem.CreateFileInfo(filePath));
            CurrentArchive = archiveFile;

            processArchiveInternal(archiveFile, settings);
            Finished?.Invoke();
            State = ProcessorState.Idle;
        }

        /// <summary>
        /// For testing.
        /// </summary>
        /// <param name="arc"></param>
        /// <param name="settings"></param>
        internal void ProcessArchive(DPArchive arc, DPProcessSettings settings)
        {
            validateProcessSettings(ref settings);
            CurrentProcessSettings = settings;
            FileSystem.Scope = setupScope(settings);
            CurrentArchive = arc;
            processArchiveInternal(arc, settings);
            Finished?.Invoke();
            State = ProcessorState.Idle;
        }

        /// <summary>
        /// Validates the <see cref="DPProcessSettings"/> object and throws an exception for any non-nullable properties that are null
        /// and defaults any nullable, null properties to their default values.
        /// </summary>
        /// <param name="settings">The settings to check and manipulate (will modify if nullable, null properties are detected)./></param>
        private static void validateProcessSettings(ref DPProcessSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings.DestinationPath, nameof(settings.DestinationPath));
            ArgumentNullException.ThrowIfNull(settings.TempPath, nameof(settings.TempPath));
            ArgumentNullException.ThrowIfNull(settings.ForceFileToDest, nameof(settings.ForceFileToDest));
            settings.ContentRedirectFolders ??= new(DefaultRedirects, StringComparer.OrdinalIgnoreCase);
            settings.ContentFolders ??= new(DefaultContentFolders, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a scope based off of DPProcessSettings.
        /// </summary>
        /// <returns></returns>
        private static DPFileScopeSettings setupScope(DPProcessSettings settings)
        {
            var filesToAllow = new List<string>(settings.ForceFileToDest.Values);
            return new DPFileScopeSettings(filesToAllow, new[] { settings.DestinationPath, settings.TempPath }, false, false, true, false);
        }

        /// <summary>
        /// Checks whether the destination has enough space for the archive.
        /// </summary>
        /// <returns>True if the destination has enough space, otherwise false.</returns>
        /// <exception cref="Exception">An exception caused by creating the <see cref="IDPDriveInfo"/> object.</exception>
        private bool DestinationHasEnoughSpace() => (ulong)FileSystem.CreateDriveInfo(CurrentProcessSettings.DestinationPath).AvailableFreeSpace > CurrentArchive.TrueArchiveSize;

        /// <summary>
        /// Checks whether the temp path has enough space for the archive.
        /// </summary>
        /// <returns>True if temp has enough space, otherwise false.</returns>
        /// <exception cref="Exception">An exception caused by creating the <see cref="IDPDriveInfo"/> object.</exception>
        private bool TempHasEnoughSpace() => (ulong)FileSystem.CreateDriveInfo(CurrentProcessSettings.TempPath).AvailableFreeSpace > CurrentArchive.TrueArchiveSize;


        // TODO: Clear temp needs to remove as much space as possible. It will error when we have file handles.
        private void ClearTemp()
        {
            Logger.Information("Clearing temp location at {TempLocation}", TempLocation);
            var tmpScope = FileSystem.Scope;
            FileSystem.Scope = new DPFileScopeSettings(Array.Empty<string>(), new[] { TempLocation }, false, throwOnPathTransversal: true);
            IDPDirectoryInfo info = FileSystem.CreateDirectoryInfo(TempLocation);
            if (!TryHelper.Try(() => info.Delete(true), out Exception? ex))
                Logger.Error(ex, "Failed to clear temp location");
            else Logger.Information("Cleared temp location");
            FileSystem.Scope = tmpScope;
        }

        private void prepareOperations()
        {
            Logger.Information("Preparing operations");
            while (!ArchiveCancelled && !TempHasEnoughSpace())
            {
                ClearTemp();
                if (TempHasEnoughSpace()) break;
                Logger.Warning("Temp location does not have enough space after clearing temp, requesting for an action");
                // Requires user help.
                var args = new DPProcessorErrorArgs(null, "Temp location does not have enough space") { Continuable = true };
                EmitOnProcessError(args);
            }
            if (CurrentArchive.Extractor != null) CurrentArchive.Extractor.ExtractProgress += EmitOnExtractionProgress;
            else Logger.Warning("Extractor is null, cannot report extraction progress");
            ReadMetaFiles(CurrentProcessSettings);
        }

        private void HandleEarlyExit()
        {
            State = ProcessorState.Idle;
            CurrentArchive.Extractor.ExtractProgress -= EmitOnExtractionProgress;
            EmitOnArchiveExit(false, null);
        }

        private bool HandleOnDestinationNotEnoughSpace()
        {
            if (DestinationHasEnoughSpace()) return true;
            while (!ArchiveCancelled && !DestinationHasEnoughSpace())
            {
                var args = new DPProcessorErrorArgs(null, "Destination does not have enough space.") { Continuable = true };
                EmitOnProcessError(args);
            }
            return !ArchiveCancelled || DestinationHasEnoughSpace();
        }

        /// <summary>
        /// Executes an action and emits the error and handles the early exit procedure if an exception occurs.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="errorMessage">The additional explanation of the error</param>
        /// <returns>Whether the try-catch caught an exception or not.</returns>
        private bool tryCatch(Action action, string errorMessage)
        {
            if (!TryHelper.Try(action, out Exception? ex))
            {
                handleError(ex, errorMessage);
                return false;
            }
            return true;
        }

        private void handleError(Exception ex, string errorMessage)
        {
            EmitOnProcessError(new DPProcessorErrorArgs(ex, errorMessage));
            HandleEarlyExit();
            return;
        }

        /// <summary>
        /// Reads the files listed in <see cref="DPArchive.DSXFiles"/>.
        /// </summary>
        private void ReadMetaFiles(DPProcessSettings settings)
        {
            // Extract the DAZ Files that have not been extracted.
            var extractSettings = new DPExtractSettings(settings.TempPath,
                CurrentArchive!.DSXFiles.Where((f) => f.FileInfo is null || !f.FileInfo.Exists),
                true, CurrentArchive);
            if (ArchiveCancelled) return;
            CurrentArchive.ExtractContentsToTemp(extractSettings);
            Stream? stream = null!;
            foreach (DPDSXFile file in CurrentArchive!.DSXFiles)
            {
                if (ArchiveCancelled) return;
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
                        EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to open read stream for file for reading meta"));
                        continue;
                    }
                    if (stream is null)
                    {
                        EmitOnProcessError(new DPProcessorErrorArgs(null, "OpenRead returned successful but also returned null stream, skipping meta read"));
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
                    EmitOnProcessError(new DPProcessorErrorArgs(ex, $"Unable to read contents of {file.Path}"));
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }

        /// <summary>
        /// Cancels the processing of the archive.
        /// </summary>
        public void CancelProcessing()
        {
            try
            {
                CancellationTokenSource.Cancel();
                ArchiveCancellationSource.Cancel();
            }
            catch (Exception ex)
            {
                EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to cancel processing"));
            }
        }
        /// <summary>
        /// Cancels only the current archive.
        /// </summary>
        public void CancelCurrentArchive()
        {
            try
            {
                ArchiveCancellationSource.Cancel();
            } catch (Exception ex)
            {
                EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to cancel current archive"));
            }
        }
        private void PopParentArchive(Stack<Tuple<DPArchive, DPExtractionReport>> s)
        {
            if (s.TryPop(out var parentArc))
            {
                var temp = CurrentArchive;
                var report = parentArc.Item2;
                CurrentArchive = parentArc.Item1;
                try { EmitOnArchiveExit(report.SuccessPercentage >= 0.1f, report); } catch { }
                CurrentArchive = temp;
            }
        }

    }
}
