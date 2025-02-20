// This code is licensed under the Keep It Free License V1.
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
    /// <summary>
    /// Implements the core processing logic for DAZ Studio Product archives, serving as the primary
    /// orchestrator for extracting, analyzing, and installing DAZ Studio products.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The DPProcessor is the main implementation that handles the complete workflow of processing
    /// DAZ Studio Product archives. It coordinates between various components to ensure proper
    /// extraction, analysis, and installation of products.
    /// </para>
    /// 
    /// Key responsibilities include:
    /// <list type="bullet">
    ///     <item>Managing the extraction process through various extractors</item>
    ///     <item>Analyzing archive contents to determine product structure</item>
    ///     <item>Coordinating metadata reading and tag assignment</item>
    ///     <item>Managing temporary and destination paths</item>
    ///     <item>Handling nested archives and complex product structures</item>
    ///     <item>Providing progress updates and error handling</item>
    /// </list>
    /// </remarks>
    public class DPProcessor : IDPProcessor
    {
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
        
        /// <summary>
        /// The current process settings being used by the processor.
        /// </summary>
        public DPProcessSettings CurrentProcessSettings { get; private set; } = new();
        /// <summary>
        /// The logger that will be used to log messages.
        /// </summary>
        /// <returns>By default, the reference to <see cref="Log.Logger"/> for <see cref="DPProcessor"/>.</returns>
        public ILogger Logger { get; set; } = Log.Logger.ForContext<DPProcessor>();

        /// <summary>
        /// The factory that creates new <see cref="IDPArchive"/> objects, specifically on <see cref="ProcessArchive(string, DPProcessSettings)"/>.
        /// </summary>
        /// <value>A <see cref="DPParentArchiveFactory"/> by default, otherwise an <see cref="IDPParentArchiveFactory"/>.</value>
        public IDPParentArchiveFactory ParentArchiveFactory { get; set; } = DPParentArchiveFactory.Instance;
        /// <summary>
        /// The metadata reader that will be used to read metadata files.
        /// </summary>
        /// <value>A <see cref="DPMetadataReader"/> by default, otherwise an <see cref="IDPMetadataReader"/>.</value>
        public IDPMetadataReader MetadataReader { get; set; } = DPMetadataReader.Instance;
        /// <summary>
        /// The file system that will be used to interact with the file system.
        /// </summary>
        /// <returns>A <see cref="DPFileSystem"/> by default, otherwise an <see cref="AbstractFileSystem"/>.</returns>
        public AbstractFileSystem FileSystem { get; set; } = new DPFileSystem();
        /// <summary>
        /// The tag provider that will be used to get tags for the files.
        /// </summary>
        /// <returns>A <see cref="DPTagProvider"/> by default, otherwise an <see cref="AbstractTagProvider"/>.</returns>
        public AbstractTagProvider TagProvider { get; set; } = DPTagProvider.Singleton;
        /// <summary>
        /// A destination determiner that will be used to determine the destination of the files.
        /// </summary>
        /// <returns>A <see cref="DPDestinationDeterminer"/> by default, otherwise an <see cref="AbstractDestinationDeterminer"/>.</returns>
        public AbstractDestinationDeterminer DestinationDeterminer { get; set; } = DPDestinationDeterminer.Singleton;
        /// <summary>
        /// The cancellation token source that will be used to cancel processing entirely.
        /// </summary>
        /// <remarks>
        /// <b>WARNING</b>:
        /// This token source will be replaced each time <see cref="ProcessArchive(string, DPProcessSettings)"/> is called; 
        /// replaced before the <see cref="ArchiveEnter"/> event is emitted.
        /// The token is ready to be be modified when the <see cref="ArchiveEnter"/> event has been emitted.
        /// </remarks>
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();
        /// <summary>
        /// The cancellation token representing the cancellation of the processing.
        /// </summary>

        /// <seealso cref="ArchiveCancellationToken"/>
        public CancellationToken CancellationToken => CancellationTokenSource.Token;
        /// <summary>
        /// The cancellation token source that will be used to cancel the processing of the current archive.
        /// </summary>
        /// <remarks>
        /// <b>WARNING:</b>
        /// This token source will be replaced each time a new archive is processed; 
        /// replaced before the <see cref="ArchiveEnter"/> event is emitted.
        /// The token is ready to be be modified when the <see cref="ArchiveEnter"/> event has been emitted.
        /// </remarks>
        public CancellationTokenSource ArchiveCancellationSource { get; set; } = new();
        /// <summary>
        /// The cancellation token representing the cancellation of the processing of the current archive.
        /// </summary>
        /// <seealso cref="ArchiveCancellationSource"/> 
        /// <seealso cref="CancellationToken"/>
        private CancellationToken ArchiveCancellationToken => ArchiveCancellationSource.Token;
        /// <summary>
        /// Determines whether the processing of this archive is cancelled or not.
        /// </summary>
        private bool ArchiveCancelled => ArchiveCancellationToken.IsCancellationRequested || CancellationToken.IsCancellationRequested;
        /// <summary>
        /// The location of the temporary files that will be used.
        /// </summary>
        /// <returns>The combination of <see cref="CurrentProcessSettings.TempPath"/> and <c>"DazProductInstaller"</c>.</returns>
        public string TempLocation => Path.Combine(CurrentProcessSettings.TempPath, @"DazProductInstaller\");
        /// <summary>
        /// The current archive that is being processed.
        /// </summary>
        /// <value>The current archive that is being processed or null if the Processor is in <see cref="ProcessorState.Idle"/></value>
        public IDPArchive? CurrentArchive { get; private set; } = null!;
        /// <summary>The current state of the processor.</summary>
        /// <remarks>Invokes <see cref="StateChanged"/> when state is changed.</remarks>
        public ProcessorState State { get => state; private set { state = value; StateChanged?.Invoke(); } }

        /// <summary>
        /// An event that is invoked when a file that is being extracted, moved, or deleted throws an error.
        /// <seealso cref="ProcessError"/>
        /// </summary>
        public event DPProcessorEventHandler<DPErrorArgs>? FileError;

        /// <inheritdoc/>
        public event DPProcessorEventHandler<DPProcessorErrorArgs>? ProcessError;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// The event will be invoked even for cancelled pending archives.
        /// </remarks>
        public event DPProcessorEventHandler<DPArchiveEnterArgs>? ArchiveEnter;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// The event will be invoked even for cancelled pending archives.
        /// </remarks>
        public event DPProcessorEventHandler<DPArchiveExitArgs>? ArchiveExit;
        /// <inheritdoc/>
        public event DPProcessorEventHandler<DPExtractProgressArgs>? ExtractProgress;
        /// <inheritdoc/>
        public event DPProcessorEventHandler<DPExtractProgressArgs>? MoveProgress;
        /// <inheritdoc/>
        public event Action? Finished;
        /// <inheritdoc/>
        public event Action? StateChanged;
         
        private ProcessorState state;
        // public event FilePreMove

        /// <summary>
        /// Emits the <see cref="ArchiveEnter"/> event and passes the arguments required. <para/>
        /// References <paramref name="cancel"/> to determine whether <see cref="DPProcessor"/> should
        /// cancel operations or not.
        /// </summary>
        private async Task EmitOnArchiveEnter()
        {
            Logger.Information("Entering archive {arc}", CurrentArchive!.FileName);
            if (ArchiveEnter is null) return;
            var args = new DPArchiveEnterArgs(CurrentArchive);
            await ArchiveEnter.Invoke(this, args);
        }
        /// <summary>
        /// Emits the <see cref="ArchiveExit"/> event and passes the arguments required. <para/>
        /// </summary>
        /// <param name="successfullyProcessed">Tell whether the archive had been successfully processed.</param>
        private async Task EmitOnArchiveExit(bool successfullyProcessed, DPExtractionReport? report)
        {
            if (successfullyProcessed) Logger.Information("Exiting archive {0} with success", CurrentArchive!.FileName);
            else Logger.Warning("Exiting archive {0} with failures", CurrentArchive!.FileName);
            Logger.Debug("Archive exit report: {@0}", report);
            if (ArchiveExit is null) return;
            await ArchiveExit.Invoke(this, new DPArchiveExitArgs(CurrentArchive, report, successfullyProcessed));
        }

        private async Task EmitOnProcessError(DPProcessorErrorArgs args)
        {
            Logger.Error(args.Ex, args.Explaination);
            if (ProcessError is null) return;
            await ProcessError.Invoke(this, args);
        }

        private async Task EmitOnExtractionProgress(IDPArchive _, DPExtractProgressArgs args)
        {
            if (ExtractProgress is null) return;
            await ExtractProgress.Invoke(this, args);
        }

        private void processArchiveInternal([NotNull] IDPArchive archiveFile, DPProcessSettings settings)
        {
            Stack<IDPArchive> archivesToProcess = new();
            Stack<Tuple<IDPArchive, DPExtractionReport>> parentArchives = new();

            archivesToProcess.Push(archiveFile);
            CancellationTokenSource = new();
            # pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            while (archivesToProcess.TryPop(out IDPArchive arc)) 
            # pragma warning restore CS8600
            {
                CurrentArchive = arc!;
                DPExtractionReport? report = null;
                PopParentArchive(parentArchives);
                ArchiveCancellationSource = new();
                EmitOnArchiveEnter().Wait();
                try
                {
                    if (ArchiveCancelled) { HandleEarlyExit(); continue; }
                    using (LogContext.PushProperty("Archive", arc.FileName))
                        Logger.Information("Processing archive");
                    var arcDebugInfo = new
                    {
                        Name = arc.FileName,
                        NestedArchive = arc.IsInnerArchive,
                        Path = arc.IsInnerArchive ? arc.Path : arc.FileInfo?.Path,
                        Extractor = arc.Extractor?.GetType().Name,
                        ParentArchiveNestedArchive = arc.AssociatedArchive?.IsInnerArchive,
                        ParentArchiveName = arc.AssociatedArchive?.FileName,
                        ParentArchivePath = arc.AssociatedArchive?.IsInnerArchive ?? false ? arc.AssociatedArchive?.Path :
                                                                                              arc.AssociatedArchive?.FileInfo?.Path,
                        ParentExtractor = arc.AssociatedArchive?.Extractor?.GetType().Name,
                    };
                    Logger.Debug("Archive that is about to be processed: {@Arc}", arcDebugInfo);
                    

                    State = ProcessorState.Starting;
                    try
                    {
                        FileSystem.CreateDirectoryInfo(TempLocation).Create();
                    }
                    catch (Exception e)
                    {
                        EmitOnProcessError(new DPProcessorErrorArgs(e, "Unable to create temp directory.") { Continuable = true }).Wait();
                    }

                    State = ProcessorState.Peeking;
                    if (ArchiveCancelled) { HandleEarlyExit(); continue; }
                    if (arc.Extractor is null)
                    {
                        EmitOnProcessError(new DPProcessorErrorArgs(null, "Unable to process archive. Potentially not an archive or archive is corrupted.")).Wait();
                        HandleEarlyExit();
                        continue;
                    }
                    arc.Extractor.CancellationToken = ArchiveCancellationToken;
                    if (!tryCatch(() => arc.PeekContents(), "Failed to peek into archive")) continue;

                    // Check if we have enough room.
                    if (!HandleOnDestinationNotEnoughSpace())
                    {
                        HandleEarlyExit();
                        continue;
                    }

                    State = ProcessorState.PreparingExtraction;
                    HashSet<IDPFile> filesToExtract = null!;
                    if (!tryCatch(prepareOperations, "Failed to prepare for extraction")) continue;
                    if (ArchiveCancelled) { HandleEarlyExit(); continue; }
                    if (!tryCatch(() => filesToExtract = DestinationDeterminer.DetermineDestinations(arc, settings), "Failed to determine destinations for files")) continue;

                    State = ProcessorState.Extracting;
                    var extractSettings = new DPExtractSettings()
                    {
                        TempPath = TempLocation,
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
                    parentArchives.Push(new Tuple<IDPArchive, DPExtractionReport>(arc, report!)); // TODO: Use method to determine whether an archive was successfully processed.
                }
                catch (Exception ex)
                {
                    handleError(ex, "An unexpected error occured while processing archive.");
                    EmitOnArchiveExit(false, report).Wait();
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
            var archiveFile = ParentArchiveFactory.CreateNewParentArchive(FileSystem.CreateFileInfo(filePath));
            CurrentArchive = archiveFile;

            processArchiveInternal(archiveFile, settings);
            Finished?.Invoke();
            State = ProcessorState.Idle;
            CurrentArchive = null;
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
            if (string.IsNullOrWhiteSpace(settings.DestinationPath)) throw new ArgumentException("Destination path cannot be empty", nameof(settings.DestinationPath));
            if (string.IsNullOrWhiteSpace(settings.TempPath)) throw new ArgumentException("Temp path cannot be empty", nameof(settings.TempPath));
            settings.ContentRedirectFolders ??= new(DefaultRedirects, StringComparer.OrdinalIgnoreCase);
            settings.ContentFolders ??= new(DefaultContentFolders, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates a scope based off of DPProcessSettings.
        /// </summary>
        /// <returns>A <see cref="DPFileScopeSettings"/>></returns>
        private static DPFileScopeSettings setupScope(DPProcessSettings settings) => 
            new(settings.ForceFileToDest.Values, new[] { settings.DestinationPath, settings.TempPath }, false, false, true, false);

        /// <summary>
        /// Checks whether the destination has enough space for the archive.
        /// </summary>
        /// <returns>True if the destination has enough space, otherwise false.</returns>
        /// <exception cref="Exception">An exception caused by creating the <see cref="IDPDriveInfo"/> object.</exception>
        private bool DestinationHasEnoughSpace => (ulong)FileSystem.CreateDriveInfo(CurrentProcessSettings.DestinationPath).AvailableFreeSpace > CurrentArchive.TrueArchiveSize;

        /// <summary>
        /// Checks whether the temp path has enough space for <see cref="CurrentArchive"/>.
        /// </summary>
        /// <returns>True if temp has enough space, otherwise false.</returns>
        /// <exception cref="NullReferenceException">If <see cref="CurrentArchive"/> is null.</exception>
        /// <exception cref="Exception">An exception caused by creating the <see cref="IDPDriveInfo"/> object.</exception>
        private bool TempHasEnoughSpace => (ulong)FileSystem.CreateDriveInfo(TempLocation).AvailableFreeSpace > CurrentArchive.TrueArchiveSize;


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
            while (!ArchiveCancelled && !TempHasEnoughSpace)
            {
                ClearTemp();
                if (TempHasEnoughSpace) break;
                Logger.Warning("Temp location does not have enough space after clearing temp, requesting for an action");
                if (ProcessError is null)
                    throw new InvalidOperationException("Temp location does not have enough space and there is no event handler for ProcessError");
                // Requires user help.
                var args = new DPProcessorErrorArgs(null, "Temp location does not have enough space") { Continuable = true };
                EmitOnProcessError(args).Wait();
            }
            if (CurrentArchive!.Extractor != null) CurrentArchive.Extractor.ExtractProgress += EmitOnExtractionProgress;
            else Logger.Warning("Extractor is null, cannot report extraction progress");
            ReadMetaFiles();
        }

        private void HandleEarlyExit()
        {
            State = ProcessorState.Idle;
            if (CurrentArchive is { Extractor: not null })
                CurrentArchive.Extractor.ExtractProgress -= EmitOnExtractionProgress;
            EmitOnArchiveExit(false, null).Wait();
        }

        private bool HandleOnDestinationNotEnoughSpace()
        {
            if (DestinationHasEnoughSpace) return true;
            while (!ArchiveCancelled && !DestinationHasEnoughSpace)
            {
                if (ProcessError == null || ProcessError.GetInvocationList().Length == 0)
                    throw new InvalidOperationException("Destination does not have enough space and there is no event handler for ProcessError");
                var args = new DPProcessorErrorArgs(null, "Destination does not have enough space.") { Continuable = true };
                EmitOnProcessError(args).Wait();
            }
            return !ArchiveCancelled || DestinationHasEnoughSpace;
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
            EmitOnProcessError(new DPProcessorErrorArgs(ex, errorMessage)).Wait();
            HandleEarlyExit();
            return;
        }

        /// <summary>
        /// Reads the files listed in <see cref="IDPArchive.DSXFiles"/>.
        /// </summary>
        private void ReadMetaFiles()
        {
            // Extract the DAZ Files that have not been extracted.
            var extractSettings = new DPExtractSettings(TempLocation,
                CurrentArchive!.DSXFiles.Where((f) => f.FileInfo is null || !f.FileInfo.Exists),
                true, CurrentArchive);
            if (ArchiveCancelled) return;
            CurrentArchive.ExtractContentsToTemp(extractSettings);
            try
            {
                MetadataReader.ReadMetadata(CurrentArchive.DSXFiles, ArchiveCancellationToken);
            }
            catch (Exception ex)
            {
                EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to read metadata")).Wait();
            }
        }

        /// <summary>
        /// Cancels the processing of the archive and any pending archives.
        /// </summary>
        /// <remarks>
        /// Pending nested archives will not be processed at all after calling this function.
        /// It is still possible for some processing for the current archive being processed, but never
        /// the next archives.</remarks>
        public void CancelProcessing()
        {
            try
            {
                CancellationTokenSource.Cancel();
                ArchiveCancellationSource.Cancel();
            }
            catch (Exception ex)
            {
                EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to cancel processing")).Wait();
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
                EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to cancel current archive")).Wait();
            }
        }
        private void PopParentArchive(Stack<Tuple<IDPArchive, DPExtractionReport>> s)
        {
            if (s.TryPop(out var parentArc))
            {
                var temp = CurrentArchive;
                var report = parentArc.Item2;
                CurrentArchive = parentArc.Item1;
                try { EmitOnArchiveExit(report.SuccessPercentage >= 0.1f, report).Wait(); } catch { }
                CurrentArchive = temp;
            }
        }

    }
}
