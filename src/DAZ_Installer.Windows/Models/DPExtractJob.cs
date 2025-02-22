// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.Database;
using DAZ_Installer.IO;
using DAZ_Installer.UI;
using DAZ_Installer.Windows.Pages;
using Microsoft.VisualBasic.FileIO;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.DP
{
    /// <inheritdoc/>
    public class DPExtractJob : IDPExtractJob
    {
        /// <summary>
        /// The logger for the <see cref="DPExtractJob"/> class.
        /// </summary>
        public ILogger Logger { get; set; } = Log.ForContext<DPExtractJob>();
        /// <summary>
        /// The view to use for extracting archives.
        /// </summary>
        /// <value>By default, <see cref="Extract.ExtractPage"/> upon initialization.</value>
        public IExtractView ExtractView { get; init; }
        /// <summary>
        /// The progress combo to use for extracting archives.
        /// </summary>
        /// <value>
        /// By default, returns the value of the <see cref="Extract.progressCombo"/> 
        /// property upon initialization.
        /// </value>
        public IProgressCombo ProgressCombo { get; init; }
        /// <summary>
        /// The processor to use for processing archives.
        /// </summary>
        /// <value>By default, <see cref="DPProcessor"/></value>
        public IDPProcessor Processor { get; set; } = new DPProcessor();
        /// <summary>
        /// The file system to use for interacting with files.
        /// </summary>
        /// <value>By default, a <see cref="DPFileSystem"/>.</value>
        public AbstractFileSystem FileSystem { get; set; } = new DPFileSystem();
        /// <summary>
        /// The message box provider to use for displaying messages.
        /// </summary>
        /// <value>By default, a <see cref="UI.MessageBoxProvider.Instance"/>.</value>
        public IMessageBoxProvider MessageBoxProvider { get; set; } = UI.MessageBoxProvider.Instance;
        /// <summary>
        /// The record manager to use for creating records.
        /// </summary>
        /// <value>By default, a <see cref="DPRecordManager"/> Instance.</value>
        public IDPRecordManager RecordManager { get; init; }
        /// <summary>
        /// The database to use for processing the files.
        /// </summary>
        public IDPDatabase Database { get; init; }
        /// <summary>
        /// The initial files to process.
        /// </summary>
        public string[] InitialFilesToProcess { get; init; }

        /// <summary>
        /// The task job to process the files.
        /// </summary>
        public Task? TaskJob { get; protected set; }
        /// <summary>
        /// The user settings to use for processing the files.
        /// </summary>
        /// <remarks>
        /// The user settings will not be null once the Task is being executed (not in queue).
        /// </remarks>
        public DPSettings? UserSettings { get; protected set; }
        /// <summary>
        /// Determines whether the entire job has been cancelled or not.
        /// </summary>
        public bool JobCancelled { get; private set; } = false;
        private ConcurrentDictionary<string, DPArchiveInfo> ArchiveInfosMap { get; init; }
        private readonly static DPTaskManager ExtractJobs = new();
        // TODO: Check if a product is already in list.

        /// <summary>
        /// Creates a new instance of the <see cref="DPExtractJob"/> with the files to process.
        /// </summary>
        /// <param name="files">The initial files to process.</param>
        /// <param name="extractView">The view to use for extracting archives, if null, <see cref="Extract.ExtractPage"/> is used.</param>
        /// <param name="progressCombo">The progress combo to use for extracting archives, if null, <see cref="Extract.progressCombo"/> is used.</param>
        /// <param name="database">The database to use for processing the files.</param>
        /// <param name="recordManager">The record manager to use for adding records.</param>
        public DPExtractJob(IEnumerable<string> files, IExtractView? extractView = null, IProgressCombo? progressCombo = null, IDPDatabase? database = null, IDPRecordManager? recordManager = null)
        {
            InitialFilesToProcess = [..files];
            ArchiveInfosMap = new(3, InitialFilesToProcess.Length * 2, PathComparer.Instance);
            ExtractView = extractView ?? Extract.ExtractPage;
            ProgressCombo = progressCombo ?? Extract.ExtractPage.progressCombo;
            Database = database ?? Program.Database;
            RecordManager = recordManager ?? new DPRecordManager(Database);

            foreach (var file in InitialFilesToProcess) {
                ArchiveInfosMap[file] = new DPArchiveInfo(file);
            }
        }

        /// <inheritdoc/>
        public Task DoJob()
        {
            TaskJob = ExtractJobs.AddToQueue(ProcessArchivesAsync);
            return TaskJob;
        }

        /// <inheritdoc/>
        public void CancelJob()
        {
            JobCancelled = true;
            Processor.CancelProcessing();
            var ArchiveInfosToUpdate = ArchiveInfosMap.Values
                .Where(archive => archive.Status is not DPArchiveStatus.Completed
                    and not DPArchiveStatus.CompletedWithIssues
                    and not DPArchiveStatus.Failed
                    and not DPArchiveStatus.Cancelled);
            
            foreach (var info in ArchiveInfosToUpdate)
            {
                SetArchiveStatus(info.FilePath, DPArchiveStatus.CancellationRequested);
            }
        }

        /// <inheritdoc/>
        public void CancelCurrentArchive()
        {
            if (Processor.CurrentArchive is null) return;
            Processor.CancelCurrentArchive();
            if (Processor.CurrentArchive.FileInfo is not null)
                SetArchiveStatus(Processor.CurrentArchive.FileInfo.Path, DPArchiveStatus.CancellationRequested);
            else Logger.Error("Failed to set archive status of current archive due to null FileInfo");
        }


        /// <inheritdoc/>
        public void SkipArchive(string archivePath)
        {
            if (!ArchiveInfosMap.TryGetValue(archivePath, out var archiveInfo))
                throw new ArgumentException("File is not in the list of files to process", nameof(archivePath));

            if (JobCancelled) return;
            
            switch (archiveInfo.Status)
            {
                case DPArchiveStatus.Cancelled:
                case DPArchiveStatus.CancellationRequested:
                case DPArchiveStatus.CancellationPending:
                case DPArchiveStatus.Completed:
                case DPArchiveStatus.CompletedWithIssues:
                case DPArchiveStatus.Failed:
                    return;
            }

            if (archiveInfo.Status is DPArchiveStatus.Pending)
                SetArchiveStatus(archiveInfo.FilePath, DPArchiveStatus.CancellationPending);
            if (Processor.CurrentArchive == archiveInfo.Archive || Processor.CurrentArchive?.FileInfo?.Path == archiveInfo.FilePath) {
                Processor.CancelCurrentArchive();
                SetArchiveStatus(archiveInfo.FilePath, DPArchiveStatus.CancellationRequested);
            }
        }

        /// <inheritdoc/>
        public IImmutableDictionary<string, DPArchiveInfo> GetArchiveInfosSnapshot() => 
            ArchiveInfosMap.ToImmutableDictionary();

        private void SetupEventHandlers()
        {
            Processor.ArchiveEnter += Processor_ArchiveEnter;
            Processor.ArchiveExit += Processor_ArchiveExit;
            Processor.ProcessError += Processor_ProcessError;
            Processor.FileError += Processor_FileError;
            Processor.StateChanged += Processor_StateChanged;
            Processor.ExtractProgress += Processor_ExtractProgress;
            Processor.MoveProgress += Processor_MoveProgress;
        }

        private void RemoveEventHandlers() {
            Processor.ArchiveEnter -= Processor_ArchiveEnter;
            Processor.ArchiveExit -= Processor_ArchiveExit;
            Processor.ProcessError -= Processor_ProcessError;
            Processor.FileError -= Processor_FileError;
            Processor.StateChanged -= Processor_StateChanged;
            Processor.ExtractProgress -= Processor_ExtractProgress;
            Processor.MoveProgress -= Processor_MoveProgress;
        }

        private void UpdateExtractView(string archivePath) {
            if (ArchiveInfosMap.TryGetValue(archivePath, out var archiveInfo)) {
                ExtractView.OnExtractJobStatusUpdate(this, archiveInfo);
            } else Logger.Warning("Attempted to update extract view but could not" + 
                                "find associated archive: {arc}", archivePath);
        }

        private Task Processor_ExtractProgress(IDPProcessor p, DPExtractProgressArgs args) {
            ExtractView.OnExtractionProgressUpdate(p, args);
            return Task.CompletedTask;
        }

        private Task Processor_MoveProgress(IDPProcessor p, DPExtractProgressArgs args) {
            ExtractView.OnMoveProgressUpdate(p, args);
            return Task.CompletedTask;
        }

        private Task Processor_FileError(IDPProcessor sender, DPArchiveErrorArgs args)
        {
            var info = EnsureArchiveInfo(args.Archive);
            ArchiveInfosMap.TryUpdate(info.FilePath, info.WithError(new(args.Ex, args.Explaination)), info);
            return Task.CompletedTask;
        }

        private void Processor_StateChanged()
        {
            if (Processor.CurrentArchive is null)
            {
                Logger.Error("Got a null archive in Processor_StateChanged");
                return;
            }
            if (Processor.CurrentArchive.FileInfo is null) {
                Logger.Error("Got a null FileInfo for the current archive in Processor_StateChanged");
                return;
            }
            CancelIfRequested(Processor.CurrentArchive);
            if (Processor.State == ProcessorState.PreparingExtraction)
            {
                SetArchiveStatus(Processor.CurrentArchive.FileInfo.Path, DPArchiveStatus.Processing);
            }
            else UpdateExtractView(Processor.CurrentArchive.FileInfo.Path);
            ExtractView.OnProcessorStateUpdate(Processor);
            return;
        }

        private Task Processor_ProcessError(IDPProcessor _, DPProcessorErrorArgs e)
        {
            if (Processor.CurrentArchive is not null)
            {
                var archiveInfo = EnsureArchiveInfo(Processor.CurrentArchive);
                ArchiveInfosMap.TryUpdate(archiveInfo.FilePath, archiveInfo.WithError(new(e.Ex, e.Explaination)), archiveInfo);
                UpdateExtractView(Processor.CurrentArchive.FileInfo!.Path);
            } else Logger.Error("Processor_CurrentArchive is null in Processor_ProcessError");
            return Task.CompletedTask;
        }

        // ArchiveExit is ALWAYS called for every Processsor_ArchiveEnter
        private async Task Processor_ArchiveExit(object sender, DPArchiveExitArgs e)
        {
            var info = EnsureArchiveInfo(e.Archive);
            DPArchiveStatus status;
            if (e.Processed)
                status = info.Errors.Count > 0 ? DPArchiveStatus.CompletedWithIssues : DPArchiveStatus.Completed;
            else {
                status = info.Status switch
                {
                    DPArchiveStatus.CancellationPending => DPArchiveStatus.Cancelled,
                    DPArchiveStatus.CancellationRequested => DPArchiveStatus.Cancelled,
                    _ => DPArchiveStatus.Failed
                };
            }
            SetArchiveStatus(e.Archive.FileInfo!.Path, status);

            if (!e.Processed) return;
            if (e.Report is not null) {
                ExtractView.OnCreatingRecords(e.Archive);
                await RecordManager.CreateAndAddRecord(e.Report, UserSettings!);
            }

            // If the archive is an nested archive (aka. was inside a source archive), then we do not consider it as a source file.
            if (e.Archive.IsInnerArchive) return;
            switch (UserSettings!.PermDeleteSource)
            {
                case SettingOptions.Yes:
                    if (e.Archive.FileInfo is not null) RemoveSourceFile(e.Archive.FileInfo.Path);
                    else Logger.Warning("Could not delete source file because archive's FileInfo was null.");
                    break;
                case SettingOptions.Prompt:
                    DialogResult result;
                    if (UserSettings.DeleteAction == RecycleOption.DeletePermanently)
                        result = MessageBoxProvider.Show("Do you wish to PERMENATELY DELETE the source file? This cannot be undone.",
                            "Delete source files", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    else
                        result = MessageBoxProvider.Show("Do you wish to recycle the source file? You can undo this by restoring the file from your recycle bin.",
                            "Recycle source files", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (e.Archive.FileInfo is not null) RemoveSourceFile(e.Archive.Path);
                        else Logger.Warning("Could not delete source file because archive's FileInfo was null.");
                    }
                    break;
            }
        }

        private async Task Processor_ArchiveEnter(IDPProcessor sender, DPArchiveEnterArgs e)
        {
            var info = EnsureArchiveInfo(e.Archive);
            UpdateExtractView(e.Archive.FileInfo!.Path);
            if (CancelIfRequested(e.Archive)) return;
            CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(15));
            var exists = await Database.ContainsArchive(e.Archive.FileName);
            if (exists == false) return;
            if (exists is null)
            {
                DialogResult result = MessageBoxProvider.Show($"An error occurred while attempting to check if \"{e.Archive.FileName}\" was already processed. " +
                    "This may indicate a database failure which could mean the product will install but with no record. " + 
                    "Do you wish to continue processing this file?\n\n" +
                    "Pressing Cancel will stop the entire extraction job.", "Database failure - do you wish to proceed?",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel) CancelJob();
                if (result == DialogResult.No) CancelCurrentArchive();
            } else
            {
                switch (UserSettings!.InstallPrevProducts)
                {
                    case SettingOptions.Prompt:
                        DialogResult result = MessageBoxProvider.Show($"It seems that \"{e.Archive.FileName}\" was already processed. " +
                            $"Do you wish to continue processing this file?", "Archive already processed",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (result == DialogResult.No) CancelCurrentArchive();
                        break;
                    case SettingOptions.No:
                        CancelCurrentArchive();
                        return;
                }
            }
        }

        private void ProcessArchivesAsync()
        {
            try
            {
                Logger.Debug("Beginning process with the following initial files: {@arcs}", InitialFilesToProcess);
                // Tell the progress combo we are beginning by enabling visibility of the progress bar and cancel button.
                ProgressCombo.StartProgress();

                // Register the cancellation token so we can cancel the process.
                var token = ProgressCombo.Token;
                token.Register(CancelJob);

                // Snapshot the settings and this will be what we use
                // throughout the entire extraction process.
                UserSettings = DPSettings.GetCopy();
                var processSettings = new DPProcessSettings
                {
                    ContentFolders = UserSettings.CommonContentFolderNames,
                    ContentRedirectFolders = UserSettings.FolderRedirects,
                    DestinationPath = UserSettings.DestinationPath,
                    TempPath = UserSettings.TempDir,
                    InstallOption = UserSettings.HandleInstallation,
                    OverwriteFiles = UserSettings.OverwriteFiles == SettingOptions.Yes ||
                                    UserSettings.OverwriteFiles == SettingOptions.Prompt,
                    ForceFileToDest = [],
                };
                SetupEventHandlers();
                ExtractView.AddToQueue(this);

                var c = InitialFilesToProcess.Length;
                for (var i = 0; i < c; i++)
                {
                    var x = InitialFilesToProcess[i];
                    Logger.Information("Preparing to process {arc}", x);

                    if (JobCancelled) return;

                    if (!ArchiveInfosMap.ContainsKey(x))
                        Logger.Error("Could not find archive info for initial file.");

                    // If the archive is cancelled, then we skip it.
                    if (CancelIfRequested(x, false)) 
                        continue;
                    
                    int percentage = (int)((double)i / c * 100);
                    ProgressCombo.SetProgress(percentage);
                    ProgressCombo.SetText($"Processing archive {i + 1}/{c}: " +
                        $"{Path.GetFileName(x)}...({percentage}%)");
                    Processor.ProcessArchive(x, processSettings);
                }
            } catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while attempting to process archive list");
            } finally
            {
                ProgressCombo.SetText($"Finished processing archives");
                ProgressCombo.ChangeProgressBarStyle(false);
                ProgressCombo.SetProgress(100);
                ProgressCombo.EndProgress();
                RemoveEventHandlers();
                GC.Collect();
            }
        }

        /// <summary>
        /// Removes the source file from the file system depending on the user settings.
        /// </summary>
        /// <param name="file">The source file to delete.</param>
        private void RemoveSourceFile(string file)
        {
            var scopeSettings = new DPFileScopeSettings([file], Array.Empty<string>(), false, true);
            FileSystem.Scope = scopeSettings;
            var fi = FileSystem.CreateFileInfo(file);
            Exception? ex;
            if (UserSettings!.DeleteAction == RecycleOption.DeletePermanently)
                fi.TryAndFixDelete(out ex);
            else
                fi.TryAndFixSendToRecycleBin(out ex);
            if (ex != null)
                Logger.Error(ex, "An error occurred while attempting to delete source file {file}", file);
        }

        /// <summary>
        /// Cancels the archive if the Processor is currently working on the archive with the specified path.
        /// It also returns a bool if the archive is in a cancellable state.
        /// </summary>
        /// <param name="archivePath">The archive path to cancel and/or check for cancellation.</param>
        /// <param name="callProcessor">
        /// Determines whether to call <see cref="IDPProcessor.CancelCurrentArchive"/> 
        /// if the archive is in a <see cref="DPArchiveStatus.CancellationPending"/> state.
        /// </param>
        /// <returns></returns>
        private bool CancelIfRequested(string archivePath, bool callProcessor = true) {
            var currentArchive = Processor.CurrentArchive;

            if (!ArchiveInfosMap.TryGetValue(archivePath, out var archiveInfo) || (archiveInfo.Archive != currentArchive && archiveInfo.FilePath != currentArchive?.FileInfo?.Path)) 
                return false;
            
            if (archiveInfo.Status is not DPArchiveStatus.CancellationPending) 
                return IsInCancellableState(archiveInfo.Status);
            
            if (callProcessor) Processor.CancelCurrentArchive();

            SetArchiveStatus(archivePath, callProcessor ? DPArchiveStatus.Cancelled: DPArchiveStatus.CancellationRequested);
                
            return IsInCancellableState(archiveInfo.Status);
        }

        /// <summary>
        /// Cancels an archive if requested.
        /// </summary>
        /// <remarks>
        /// This also checks if the parent has been cancelled, recursively if the archive has not been marked for cancellation.
        /// </remarks>
        /// <param name="archive">The archive to cancel.</param>
        /// <param name="callProcessor">Whether to call <see cref="IDPProcessor.CancelCurrentArchive"/></param>
        /// <returns>If the archive is scheduled for cancellation.</returns>
        private bool CancelIfRequested(IDPArchive archive, bool callProcessor = true) {
            if (archive.FileInfo is null)
            {
                Logger.Error("Could not check for cancellation due to null FileInfo for archive {arc}", archive.Path);
                return archive.AssociatedArchive is not null && CancelIfRequested(archive.AssociatedArchive, callProcessor);
            }
            var result = CancelIfRequested(archive.FileInfo.Path, callProcessor);
            // Recursively check the parent archive hierachy for nested archives.
            return archive.AssociatedArchive is null ? result : result || CancelIfRequested(archive.AssociatedArchive, callProcessor);
        }

        private DPArchiveInfo EnsureArchiveInfo(IDPArchive archive)
        {
            // It should not be null as this is only for archives on disk.
            // But if it is, we really messed up.
            if (archive.FileInfo is null) 
                throw new ArgumentException("Archive's FileInfo is null", nameof(archive));
            var archiveInfo = ArchiveInfosMap.GetOrAdd(archive.FileInfo.Path, new DPArchiveInfo(archive));

            // Add the archive object if the object was created via path only.
            var archiveInfoWithArchive = archiveInfo with { Archive = archive };
            if (ArchiveInfosMap.TryUpdate(archive.FileInfo.Path, archiveInfo with { Archive = archive }, archiveInfo))
                archiveInfo = archiveInfoWithArchive;
            // If we created a new archive (ex: for a nested archive) and the parent is scheduled for cancellation, then
            // mark the nested archive for cancellation.
            if (archive.Parent is not null && ArchiveInfosMap.TryGetValue(archive.Parent.Path, out DPArchiveInfo? value) 
                                            && IsInCancellableState(value.Status))
                SetArchiveStatus(archiveInfo.FilePath, DPArchiveStatus.CancellationPending);
            return archiveInfo;
        }


        private void SetArchiveStatus(string archive, DPArchiveStatus status) {
            ArchiveInfosMap.AddOrUpdate(archive, 
                _ => new DPArchiveInfo(archive) { Status = status },
                (_, info) => info with { Status = status });
            UpdateExtractView(archive);
        }

        private static bool IsInCancellableState(DPArchiveStatus status) => status is DPArchiveStatus.CancellationRequested or 
                                                                                      DPArchiveStatus.Cancelled or 
                                                                                      DPArchiveStatus.CancellationPending;
    }
}
