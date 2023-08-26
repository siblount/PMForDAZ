// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core.Extraction;

using DAZ_Installer.IO;
using Serilog;
using Serilog.Context;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Reflection.Metadata.Ecma335;
using System.Text;

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
                                                                                                                                                       new("Documents", "Documentation") });
        public static readonly ImmutableHashSet<string> DefaultContentFolders = ImmutableHashSet.Create<string>(StringComparer.OrdinalIgnoreCase)
                                                                                                .Union(new[] {"aniBlocks", "Animals", "Architecture", "Camera Presets", "data", "DAZ Studio Tutorials", "Documentation", "Documents",
                                                                                                              "Environments", "General", "Light Presets", "Lights", "People", "Presets", "Props", "Render Presets", "Render Settings", "Runtime",
                                                                                                              "Scene Builder", "Scene Subsets", "Scenes", "Scripts", "Shader Presets", "Shaders", "Support", "Templates", "Textures", "Vehicles" });
        public DPProcessSettings settingsToUse = new();
        public ILogger Logger { get; set; } = Log.Logger.ForContext<DPProcessor>();
        public IContextFactory ContextFactory { get; set; } = new DPIOContextFactory();
        public string TempLocation => Path.Combine(settingsToUse.TempPath, @"DazProductInstaller\");
        public string DestinationPath => settingsToUse.DestinationPath;
        public DPArchive CurrentArchive { get; private set; } = null!;
        public ProcessorState State { get => state; private set { state = value; StateChanged?.Invoke(); } }
        private volatile bool cancel = false;

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
        private DPAbstractIOContext context = DPAbstractIOContext.None;
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
            cancel = args.Cancel;
        }
        /// <summary>
        /// Emits the <see cref="ArchiveExit"/> event and passes the arguments required. <para/>
        /// </summary>
        /// <param name="successfullyProcessed">Tell whether the archive had been successfully processed.</param>
        private void EmitOnArchiveExit(bool successfullyProcessed, DPExtractionReport? report)
        {
            if (successfullyProcessed) Logger.Information("Exiting archive with success");
            else Logger.Warning("Exiting archive with failures");
            Logger.Debug("Archive exit report: {@Report}", report);
            ArchiveExit?.Invoke(this, new DPArchiveExitArgs(CurrentArchive, report, successfullyProcessed));
        }

        private void EmitOnProcessError(DPProcessorErrorArgs args)
        {
            Logger.Error(args.Ex, args.Explaination);
            if (ProcessError is null) return;
            ProcessError.Invoke(this, args);
            cancel = args.Continuable && args.CancelOperation;
        }

        private void EmitOnExtractionProgress(DPExtractProgressArgs args) => ExtractProgress?.Invoke(this, args);

        private void processArchiveInternal(DPArchive archiveFile, DPProcessSettings settings)
        {
            CurrentArchive = archiveFile;

            EmitOnArchiveEnter();
            using (LogContext.PushProperty("Archive", archiveFile.FileName))
            Logger.Information("Processing archive");
            var arcDebugInfo = new { 
                NestedArchive = archiveFile.IsInnerArchive, 
                Name = archiveFile.FileName,
                Path = archiveFile.IsInnerArchive ? archiveFile.Path : archiveFile?.FileInfo?.Path,
                archiveFile?.Extractor,
                ParentArchiveNestedArchive = archiveFile?.AssociatedArchive?.IsInnerArchive,
                ParentArchiveName = archiveFile?.AssociatedArchive?.FileName,
                ParentArchivePath = archiveFile?.AssociatedArchive?.IsInnerArchive ?? false ? archiveFile?.AssociatedArchive?.Path : 
                                                                                              archiveFile?.AssociatedArchive?.FileInfo?.Path,
                ParentExtractor = archiveFile?.AssociatedArchive?.Extractor,
            };
            Logger.Debug("Archive that is about to be processed: {@Arc}", arcDebugInfo);
            if (cancel) return;

            State = ProcessorState.Starting;
            try
            {
                context.CreateDirectoryInfo(TempLocation).Create();
            }
            catch (Exception e)
            {
                EmitOnProcessError(new DPProcessorErrorArgs(e, "Unable to create temp directory."));
                if (cancel)
                {
                    HandleEarlyExit();
                    return;
                }
            }

            State = ProcessorState.Peeking;
            try
            {
                archiveFile.PeekContents();
            }
            catch (Exception ex)
            {
                EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to peek into archive."));
                HandleEarlyExit();
                return;
            }
            // Check if we have enough room.
            if (!HandleOnDestinationNotEnoughSpace())
            {
                HandleEarlyExit();
                return;
            }

            State = ProcessorState.PreparingExtraction;
            HashSet<DPFile> filesToExtract;
            try
            {
                prepareOperations();
                DetermineContentFolders();
                UpdateRelativePaths();
                filesToExtract = DetermineFilesToExtract();
            }
            catch (Exception ex)
            {
                EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to prepare for extraction"));
                HandleEarlyExit();
                return;
            }

            State = ProcessorState.Extracting;
            var extractSettings = new DPExtractSettings()
            {
                TempPath = settingsToUse.TempPath,
                Archive = archiveFile,
                FilesToExtract = filesToExtract,
                OverwriteFiles = settingsToUse.OverwriteFiles,
            };
            DPExtractionReport report;
            try
            {
                report = archiveFile.ExtractContents(extractSettings);
            }
            catch (Exception ex)
            {
                EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to extract contents for archive"));
                HandleEarlyExit();
                return;
            }
            // DPCommon.WriteToLog("We are done");
            Logger.Information("Analyzing the archive - fetching tags");
            State = ProcessorState.Analyzing;
            archiveFile.Type = archiveFile.DetermineArchiveType();
            try
            {
                GetTags(settings);
            }
            catch (Exception ex)
            {
                EmitOnProcessError(new DPProcessorErrorArgs(ex, "Failed to get tags for archive"));
            }

            for (var i = 0; i < archiveFile.Subarchives.Count; i++)
            {
                DPArchive arc = archiveFile.Subarchives[i];
                if (arc.Extracted) processArchiveInternal(arc, settings); // TODO: This can lead to a stack overflow...fix maybe?
            }

            // Create record.
            EmitOnArchiveExit(true, report); // TODO: Use method to determine whether an archive was successfully processed.
            return;
        }

        // TODO: RetryArchive()
        public void ProcessArchive(string filePath, DPProcessSettings settings)
        {
            validateProcessSettings(ref settings);
            settingsToUse = settings;
            cancel = false;
            context = ContextFactory.CreateContext(setupScope(settings), new DriveInfo(settings.DestinationPath));
            // Create new archive.
            var archiveFile = DPArchive.CreateNewParentArchive(context.CreateFileInfo(filePath));
            CurrentArchive = archiveFile;

            processArchiveInternal(archiveFile, settings);
            context = null!;
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
            settingsToUse = settings;
            cancel = false;
            context = ContextFactory.CreateContext(setupScope(settings), new DriveInfo(settings.DestinationPath));
            CurrentArchive = arc;
            processArchiveInternal(arc, settings);
            context = null!;
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
            var list = new List<string>(settings.ContentFolders!.Count + settings.ContentRedirectFolders!.Count + 1);
            list.AddRange(settings.ContentFolders.Select(x => Path.Combine(settings.DestinationPath, x)));
            list.AddRange(settings.ContentRedirectFolders.Select(x => Path.Combine(settings.DestinationPath, x.Value)));
            list.Add(settings.TempPath);
            var filesToAllow = new List<string>(settings.ForceFileToDest.Values);
            return new DPFileScopeSettings(filesToAllow, list, false, false, true, false);
        }

        private void UpdateRelativePaths()
        {
            foreach (DPFile content in CurrentArchive.RootContents)
                content.RelativePathToContentFolder = content.RelativeTargetPath = content.Path;
            foreach (DPFolder folder in CurrentArchive.Folders.Values)
                folder.UpdateChildrenRelativePaths(settingsToUse);
        }

        private HashSet<DPFile> DetermineFilesToExtract()
        {
            // Handle Manifest first.
            var filesToExtract = new HashSet<DPFile>(CurrentArchive.Contents.Count);
            DetermineFromManifests(filesToExtract);
            // Determine via file sense next.
            DetermineViaFileSense(filesToExtract);
            return filesToExtract;
        }

        private void DetermineFromManifests(HashSet<DPFile> filesToExtract)
        {
            if (settingsToUse.InstallOption != InstallOptions.ManifestAndAuto && settingsToUse.InstallOption != InstallOptions.ManifestOnly) return;
            foreach (DPDSXFile? manifest in CurrentArchive!.ManifestFiles.Where(f => f.Extracted))
            {
                Dictionary<string, string> manifestDestinations = manifest.GetManifestDestinations();

                foreach (DPFile file in CurrentArchive.Contents.Values)
                {
                    try
                    {
                        if (!manifestDestinations.ContainsKey(file.Path) || filesToExtract.Contains(file)) continue;
                        file.TargetPath = GetTargetPath(file, overridePath: manifestDestinations[file.Path]);
                        filesToExtract.Add(file);
                    } catch (Exception ex)
                    {
                        Logger.Error("Failed to determine file to extract: {0}", file.Path);
                        Logger.Debug("File information: {@0}", file);
                    }

                    //else
                    //{
                    //    file.WillExtract = settingsToUse.InstallOption != InstallOptions.ManifestOnly;
                    //}
                }
            }
        }

        private void DetermineViaFileSense(HashSet<DPFile> filesToExtract)
        {

            if (settingsToUse.InstallOption != InstallOptions.Automatic && settingsToUse.InstallOption != InstallOptions.ManifestAndAuto) return;
            // Get contents where file was not extracted.
            Dictionary<string, DPFolder>.ValueCollection folders = CurrentArchive.Folders.Values;

            foreach (DPFolder folder in folders)
            {
                if (!folder.IsContentFolder && !folder.IsPartOfContentFolder) continue;
                // Update children's relative path.
                folder.UpdateChildrenRelativePaths(settingsToUse);

                foreach (DPFile child in folder.Contents)
                {
                    //Get destination path and update child destination path.
                    child.TargetPath = GetTargetPath(child);

                    filesToExtract.Add(child);
                }
            }
            // Now hunt down all files in folders that aren't in content folders.
            foreach (DPFolder folder in folders)
            {
                if (folder.IsContentFolder) continue;
                // Add all archives to the inner archives to process for later processing.
                foreach (DPFile file in folder.Contents)
                {
                    if (file is not DPArchive arc) continue;
                    arc.TargetPath = GetTargetPath(arc, true);
                    // Add to queue.
                    CurrentArchive.Subarchives.Add(arc);
                    filesToExtract.Add(arc);
                }
            }

            // Hunt down all files in root content.

            foreach (DPFile content in CurrentArchive.RootContents)
            {
                if (content is not DPArchive arc) continue;
                arc.TargetPath = GetTargetPath(arc, true);
                // Add to queue.
                CurrentArchive.Subarchives.Add(arc);
                filesToExtract.Add(arc);
            }
        }

        /// <summary>
        /// This function returns the target path based on whether it is saving to it's destination or to a
        /// temporary location, whether the <paramref name="file"/> has a relative path or not, and whether
        /// the file's parent is in folderRedirects. <para/>
        /// Additionally, there is <paramref name="overridePath"/> which will be used for combining paths publicly;
        /// <b>however</b>, this will be ignored if the parent name is in the user's folder redirects.
        /// </summary>
        /// <param name="file">The file to get a target path for.</param>
        /// <param name="saveToTemp">Determines whether to get a target path saving to a temporary location.</param>
        /// <param name="overridePath">The path to combine with instead of usual combining. </param>
        /// <returns>The target path for the specified file. </returns>
        private string GetTargetPath(DPAbstractNode file, bool saveToTemp = false, string? overridePath = null)
        {
            var filePathPart = !string.IsNullOrEmpty(overridePath) ? overridePath : file.RelativeTargetPath;

            if (file.Parent is null || !settingsToUse.ContentRedirectFolders!.ContainsKey(Path.GetFileName(file.Parent.Path)))
                return Path.Combine(saveToTemp ? TempLocation : DestinationPath, filePathPart);

            return Path.Combine(saveToTemp ? TempLocation : DestinationPath,
                file.RelativeTargetPath ?? file.Parent.CalculateChildRelativeTargetPath(file, settingsToUse));
        }

        private bool DestinationHasEnoughSpace() => (ulong)context.AvailableFreeSpace > CurrentArchive.TrueArchiveSize;
       
        private bool TempHasEnoughSpace() => (ulong)ContextFactory.CreateContext(context.Scope,
            new DriveInfo(Path.GetPathRoot(TempLocation)!)).AvailableFreeSpace > CurrentArchive.TrueArchiveSize;

        private void DetermineContentFolders()
        {
            // A content folder is a folder whose name is contained in the user's common content folders list
            // or in their folder redirects map.


            // Prepare sort so that the first elements in folders are the ones at root.
            DPFolder[] folders = CurrentArchive.Folders.Values.ToArray();
            var foldersKeys = new byte[folders.Length];

            for (var i = 0; i < foldersKeys.Length; i++)
            {
                foldersKeys[i] = PathHelper.GetSubfoldersCount(folders[i].Path);
            }

            // Elements at the beginning are folders at root levels.
            Array.Sort(foldersKeys, folders);

            foreach (DPFolder? folder in folders)
            {
                var folderName = Path.GetFileName(folder.Path);
                var elgibleForContentFolderStatus = settingsToUse.ContentFolders.Contains(folderName) ||
                                                    settingsToUse.ContentRedirectFolders.ContainsKey(folderName);
                if (folder.Parent is null)
                    folder.IsContentFolder = elgibleForContentFolderStatus;
                else
                {
                    if (folder.Parent.IsContentFolder || folder.Parent.IsPartOfContentFolder) continue;
                    folder.IsContentFolder = elgibleForContentFolderStatus;
                }
            }
        }


        // TODO: Clear temp needs to remove as much space as possible. It will error when we have file handles.
        private void ClearTemp()
        {
            Logger.Information("Clearing temp location at {TempLocation}", TempLocation);
            var tempCtx = ContextFactory.CreateContext(new DPFileScopeSettings(Array.Empty<string>(), new[] { TempLocation }, false, throwOnPathTransversal: true));
            IDPDirectoryInfo info = context.CreateDirectoryInfo(TempLocation);
            if (!TryHelper.Try(() => info.Delete(true), out Exception? ex))
                Logger.Error(ex, "Failed to clear temp location");
            else Logger.Information("Cleared temp location");
        }

        private void prepareOperations()
        {
            Logger.Information("Preparing operations");
            while (!cancel && !TempHasEnoughSpace())
            {
                ClearTemp();
                if (TempHasEnoughSpace()) break;
                Logger.Warning("Temp location does not have enough space after clearing temp, requesting for an action");
                // Requires user help.
                var args = new DPProcessorErrorArgs(null, "Temp location does not have enough space");
                args.Continuable = true;
                EmitOnProcessError(args);
                
            }
            ReadMetaFiles(settingsToUse);
        }

        private void HandleEarlyExit()
        {
            State = ProcessorState.Idle;
            EmitOnArchiveExit(false, null);
        }

        private bool HandleOnDestinationNotEnoughSpace()
        {
            if (DestinationHasEnoughSpace()) return true;
            while (!cancel && !DestinationHasEnoughSpace())
            {
                var args = new DPProcessorErrorArgs(null, "Destination does not have enough space.");
                args.Continuable = true;
                EmitOnProcessError(args);
                cancel = args.CancelOperation;
            }
            return !cancel || DestinationHasEnoughSpace();
        }

        private void GetTags(DPProcessSettings settings)
        {
            // First is always author.
            // Next is folder names.
            ReadContentFiles(settings);
            ReadMetaFiles(settings);
            var tagsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            tagsSet.EnsureCapacity(CurrentArchive.GetEstimateTagCount() + 5 +
                (CurrentArchive.Folders.Count * 2) + ((CurrentArchive.Contents.Count - CurrentArchive.Subarchives.Count) * 2));
            foreach (DPDazFile file in CurrentArchive.DazFiles)
            {
                DPContentInfo contentInfo = file.ContentInfo;
                if (contentInfo.Website.Length != 0) tagsSet.Add(contentInfo.Website);
                if (contentInfo.Email.Length != 0) tagsSet.Add(contentInfo.Email);
                tagsSet.UnionWith(contentInfo.Authors);
            }
            foreach (DPFile content in CurrentArchive.Contents.Values)
            {
                if (content is DPArchive) continue;
                tagsSet.UnionWith(Path.GetFileNameWithoutExtension(content.FileName).Split(' '));
            }
            foreach (KeyValuePair<string, DPFolder> folder in CurrentArchive.Folders)
            {
                tagsSet.UnionWith(PathHelper.GetFileName(folder.Key).Split(' '));
            }
            tagsSet.UnionWith(CurrentArchive.ProductInfo.Authors);
            tagsSet.UnionWith(DPArchive.RegexSplitName(CurrentArchive.ProductInfo.ProductName));
            if (CurrentArchive.ProductInfo.SKU.Length != 0) tagsSet.Add(CurrentArchive.ProductInfo.SKU);
            if (CurrentArchive.ProductInfo.ProductName.Length != 0) tagsSet.Add(CurrentArchive.ProductInfo.ProductName);
            CurrentArchive.ProductInfo.Tags = tagsSet;
        }
        /// <summary>
        /// Reads files that have the extension .dsf and .duf after it has been extracted. 
        /// </summary>
        private void ReadContentFiles(DPProcessSettings settings)
        {
            // Extract the DAZ Files that have not been extracted.
            var extractSettings = new DPExtractSettings(settings.TempPath,
                CurrentArchive!.DazFiles.Where((f) => f.FileInfo is null || !f.FileInfo.Exists),
                true, CurrentArchive);
            if (extractSettings.FilesToExtract.Count > 0) CurrentArchive.ExtractToTemp(extractSettings);
            Stream? stream = null;
            // Read the contents of the files.
            foreach (DPDazFile file in CurrentArchive!.DazFiles)
            {
                // If it did not extract correctly we don't have acces, just skip it.
                if (file.FileInfo is null || !file.FileInfo.Exists)
                {
                    EmitOnProcessError(new DPProcessorErrorArgs(null, $"{file.Path} does not exist on disk (or does not have access to it)."));
                    continue;
                }
                try
                {
                    if (!file.FileInfo!.TryAndFixOpenRead(out stream, out Exception? ex))
                    {
                        EmitOnProcessError(new DPProcessorErrorArgs(ex, $"Failed to open read stream for file: {file.Path}"));
                        continue;
                    }
                    if (stream is null)
                    {
                        EmitOnProcessError(new DPProcessorErrorArgs(null, $"OpenRead returned successful but also returned null stream, skipping {file.Path}"));
                        continue;
                    }
                    if (stream.ReadByte() == 0x1F && stream.ReadByte() == 0x8B)
                    {
                        // It is gzipped compressed.
                        stream.Seek(0, SeekOrigin.Begin);
                        using var gstream = new GZipStream(stream, CompressionMode.Decompress);
                        using var streamReader = new StreamReader(gstream, Encoding.UTF8, true);
                        file.ReadContents(streamReader);
                    }
                    else
                    {
                        // It is normal text.
                        stream.Seek(0, SeekOrigin.Begin);
                        using var streamReader = new StreamReader(stream, Encoding.UTF8, true);
                        file.ReadContents(streamReader);
                    }
                }
                catch (Exception ex)
                {
                    EmitOnProcessError(new DPProcessorErrorArgs(ex, $"Unable to read contents of {file}"));
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }
        /// <summary>
        /// Reads the files listed in <see cref="DPArchive.DSXFiles"/>.
        /// </summary>
        private void ReadMetaFiles(DPProcessSettings settings)
        {
            // Extract the DAZ Files that have not been extracted.
            var extractSettings = new DPExtractSettings(settings.TempPath,
                CurrentArchive!.DazFiles.Where((f) => f.FileInfo is null || !f.FileInfo.Exists),
                true, CurrentArchive);
            CurrentArchive.ExtractContentsToTemp(extractSettings);
            Stream? stream = null!;
            foreach (DPDSXFile file in CurrentArchive!.DSXFiles.Where(x => x.FileName != "Manifest.dsx" && x.FileName != "Supplement.dsx"))
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
                        EmitOnProcessError(new DPProcessorErrorArgs(ex, $"Failed to open read stream for file for reading meta: {file.Path}"));
                        continue;
                    }
                    if (stream is null)
                    {
                        EmitOnProcessError(new DPProcessorErrorArgs(null, $"OpenRead returned successful but also returned null stream, skipping {file.Path}"));
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
    }
}
