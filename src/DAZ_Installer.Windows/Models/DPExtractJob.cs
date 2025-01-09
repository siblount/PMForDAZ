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
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.DP
{
    /// <summary>
    /// A class that represents a job to process archives.
    /// </summary>
    public class DPExtractJob
    {
        /// <summary>
        /// The logger for the <see cref="DPExtractJob"/> class.
        /// </summary>
        public static ILogger Logger { get; set; } = Log.Logger.ForContext<DPExtractJob>();
        /// <summary>
        /// The view to use for extracting archives.
        /// </summary>
        /// <value>By default, <see cref="Extract.ExtractPage"/> upon initialization.</value>
        public IExtractView ExtractView { get; set; } = Extract.ExtractPage;
        /// <summary>
        /// The progress combo to use for extracting archives.
        /// </summary>
        /// <value>
        /// By default, returns the value of the <see cref="Extract.progressCombo"/> 
        /// property upon initialization.
        /// </value>
        public IProgressCombo ProgressCombo { get; set; } = Extract.ExtractPage.progressCombo;
        /// <summary>
        /// The processor to use for processing archives.
        /// </summary>
        public IDPProcessor Processor { get; set; } = new DPProcessor();
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
        private Dictionary<string, DPArchiveInfo> ArchiveInfos { get; init; }
        private readonly object archiveInfoLock = new();

        private static DPTaskManager ExtractJobs = new();
        // TODO: Check if a product is already in list.

        /// <summary>
        /// Creates a new instance of the <see cref="DPExtractJob"/> with the files to process.
        /// </summary>
        /// <param name="files">The initial files to process</param>
        public DPExtractJob(IEnumerable<string> files)
        {

            InitialFilesToProcess = [..files];
            ArchiveInfos = new(InitialFilesToProcess.Length * 2, PathComparer.Instance);
            
            foreach (var file in InitialFilesToProcess) {
                ArchiveInfos[file] = new DPArchiveInfo(file);
            }
        }

        /// <summary>
        /// Adds the job to the queue to be processed.
        /// </summary>
        /// <returns>The Task object</returns>
        public Task DoJob()
        {
            TaskJob = ExtractJobs.AddToQueue(ProcessListAsync);
            return TaskJob;
        }

        /// <summary>
        /// Cancels processing current and pending archives.
        /// </summary>
        /// <seealso cref="CancelCurrentArchive"/>
        /// <seealso cref="SkipArchive(string)"/>
        public void CancelJob()
        {
            lock (archiveInfoLock)
            {
                Processor.CancelProcessing();
                var ArchiveInfosToUpdate = ArchiveInfos.Values
                    .Where(archive => archive.Status is not DPArchiveStatus.Completed
                        and not DPArchiveStatus.CompletedWithIssues
                        and not DPArchiveStatus.Failed);
                foreach (var info in ArchiveInfosToUpdate)
                {
                    info.Status = DPArchiveStatus.CancellationRequested;
                }
            }
        }

        /// <summary>
        /// Cancels the current archive being processed.
        /// </summary>
        /// <seealso cref="CancelJob"/>
        /// <seealso cref="SkipArchive(string)"/>
        public void CancelCurrentArchive()
        {
            if (Processor.CurrentArchive is null) return;
            lock (archiveInfoLock)
            {
                Processor.CancelCurrentArchive();
                if (ArchiveInfos.TryGetValue(Processor.CurrentArchive.Path, out var archiveInfo))
                    archiveInfo.Status = DPArchiveStatus.CancellationRequested;
                else
                    Logger.Error("Failed to cancel current archive due to unable to find archive info for {archive}", Processor.CurrentArchive.NormalizedPath);
            }
        }

        /// <summary>
        /// Skips/cancels the archive from being processed.
        /// </summary>
        /// <remarks>
        /// If the archive has not yet been processed, it will be skipped.
        /// If the archive has been processed, it will not be skipped.
        /// If the archive is currently in process, a cancellation request will be issued.
        /// </remarks>
        /// <param name="archivePath">The path of the archive to skip/cancel.</param>
        /// <exception cref="ArgumentException">The archive is not the list of files to process</exception>
        public void SkipArchive(string archivePath)
        {
            if (!ArchiveInfos.TryGetValue(archivePath, out var archiveInfo))
                throw new ArgumentException("File is not in the list of files to process", nameof(archivePath));
            
            lock (archiveInfoLock) {
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
                    archiveInfo.Status = DPArchiveStatus.CancellationPending;
                if (Processor.CurrentArchive is not null && 
                        (Processor.CurrentArchive == archiveInfo.Archive || Processor.CurrentArchive.Path == archiveInfo.FilePath))
                    Processor.CancelCurrentArchive();
                    archiveInfo.Status = DPArchiveStatus.CancellationRequested;
            }
        }

        private void SetupEventHandlers()
        {
            Processor.ArchiveEnter += Processor_ArchiveEnter;
            Processor.ArchiveExit += Processor_ArchiveExit;
            Processor.ProcessError += Processor_ProcessError;
            Processor.StateChanged += Processor_StateChanged;
            Processor.ExtractProgress += Processor_ExtractProgress;
            Processor.MoveProgress += Processor_MoveProgress;
        }


        private void Processor_StateChanged()
        {
            if (Processor.CurrentArchive is null)
            {
                Logger.Error("Got a null archive in Processor_StateChanged");
                return;
            }
            if (CancelIfRequested(Processor.CurrentArchive.Path)) return;
            if (Processor.State == ProcessorState.PreparingExtraction)
            {
                // TO DO: Highlight files in red for files that failed to extract.
                ExtractView.BeginInvoke(() =>
                {
                    ExtractView.SuspendLayout();
                    try
                    {
                        ExtractView.AddToList(Processor.CurrentArchive);
                        ExtractView.AddToHierachy(Processor.CurrentArchive);
                        ProgressCombo.ChangeProgressBarStyle(true);
                        ProgressCombo.SetText($"Preparing to extract contents in {Processor.CurrentArchive.FileName}...");
                        ProgressCombo.SetProgress(0);
                    } catch (Exception ex)
                    {
                        Logger.Error(ex, "An error occurred while attempting to add archive to list");
                    } finally
                    {
                        ExtractView.ResumeLayout();
                    }
                });
            }
            else if (Processor.State == ProcessorState.Analyzing)
            {
                ProgressCombo.ChangeProgressBarStyle(true);
                ProgressCombo.SetText($"Analyzing file contents in {Processor.CurrentArchive.FileName}...");
            }
        }

        private void Processor_ExtractProgress(DPProcessor sender, DPExtractProgressArgs e)
        {
            ProgressCombo.ChangeProgressBarStyle(false);
            ProgressCombo.SetProgress(e.ExtractionPercentage);
            ProgressCombo.SetText($"Extracting contents from {e.Archive.FileName}...{e.ExtractionPercentage}%");
        }

        private void Processor_MoveProgress(DPProcessor sender, DPExtractProgressArgs e)
        {
            ProgressCombo.ChangeProgressBarStyle(true);
            ProgressCombo.SetText($"Moving files from {e.Archive.FileName} to destination...%");
        }

        private void Processor_ProcessError(DPProcessor _, DPProcessorErrorArgs e)
        {
            lock (archiveInfoLock)
            {
                if (Processor.CurrentArchive is not null)
                {
                    if (ArchiveInfos.TryGetValue(Processor.CurrentArchive.Path, out var info))
                        info.Errors.Add(new(e.Ex, e.Explaination));
                    else
                    {
                        Logger.Error("Could not find archive info for {archive}", Processor.CurrentArchive.NormalizedPath);
                    }
                } else Logger.Error("Processor_CurrentArchive is null in Processor_ProcessError");
            }
        }

        private void Processor_ArchiveExit(object sender, DPArchiveExitArgs e)
        {
            lock (archiveInfoLock)
            {
                if (ArchiveInfos.TryGetValue(e.Archive.Path, out var info)) {
                    if (e.Processed)
                        info.Status = info.Errors.Count > 0 ? DPArchiveStatus.CompletedWithIssues : DPArchiveStatus.Completed;
                    else { 
                        info.Status = info.Status switch
                        {
                            DPArchiveStatus.CancellationPending => DPArchiveStatus.Cancelled,
                            DPArchiveStatus.CancellationRequested => DPArchiveStatus.Cancelled,
                            _ => DPArchiveStatus.Failed
                        };
                    }
                } else {
                    Logger.Error("Could not find archive info for {archive}", e.Archive.NormalizedPath);
                }
            }

            if (!e.Processed) return;
            // Create records if applicable.
            // TODO: Only add if successful extraction, and all files from temp were moved, and/or user didn't cancel operation.
            ProgressCombo.ChangeProgressBarStyle(true);
            Logger.Information("Creating records for {arc}", e.Archive.FileName);
            ProgressCombo.SetText($"Creating records for {e.Archive.FileName}...");
            CreateRecords(e.Archive, e.Report!);

            if (e.Archive.IsInnerArchive) return;
            switch (UserSettings!.PermDeleteSource)
            {
                case SettingOptions.Yes:
                    RemoveSourceFile(e.Archive.Path);
                    break;
                case SettingOptions.Prompt:
                    DialogResult result;
                    if (UserSettings.DeleteAction == RecycleOption.DeletePermanently)
                        result = MessageBox.Show("Do you wish to PERMENATELY DELETE the source file? This cannot be undone.",
                            "Delete source files", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    else
                        result = MessageBox.Show("Do you wish to recycle the source file? You can undo this by restoring the file from your recycle bin.",
                            "Recycle source files", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes) RemoveSourceFile(e.Archive.Path);
                    break;
            }
        }

        private async void Processor_ArchiveEnter(DPProcessor sender, DPArchiveEnterArgs e)
        {
            if (CancelIfRequested(e.Archive.Path)) return;
            CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(15));
            var exists = await Program.Database.ContainsArchive(e.Archive.FileName);
            if (exists == false) return;
            if (exists is null)
            {
                DialogResult result = MessageBox.Show($"An error occurred while attempting to check if \"{e.Archive.FileName}\" was already processed. " +
                    "This may indicate a database failure which could mean the product will install but with no record. " + 
                    "Do you wish to continue processing this file?\n\n" +
                    "Pressing Cancel will stop the entire extraction job.", "Database failure - do you wish to proceed?",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
                if (result == DialogResult.Cancel) CancelJob();
                if (result == DialogResult.No) CancelCurrentArchive();
            } else
            {
                switch (UserSettings!.InstallPrevProducts)
                {
                    case SettingOptions.Prompt:
                        DialogResult result = MessageBox.Show($"It seems that \"{e.Archive.FileName}\" was already processed. " +
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

        private void ProcessListAsync(CancellationToken t)
        {
            try
            {
                // Tell the progress combo we are beginning by enabling visibility of the progress bar and cancel button.
                ProgressCombo.StartProgress();

                // Register the cancellation token so we can cancel the process.
                var token = ProgressCombo.Token;
                token.Register(Processor.CancelProcessing);

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
            var fs = new DPFileSystem(scopeSettings);
            var fi = fs.CreateFileInfo(file);
            Exception? ex;
            if (UserSettings!.DeleteAction == RecycleOption.DeletePermanently)
                fi.TryAndFixDelete(out ex);
            else
                fi.TryAndFixSendToRecycleBin(out ex);
            if (ex != null)
                Logger.Error(ex, "An error occurred while attempting to delete source file {file}", file);
        }

        private bool CancelIfRequested(string archivePath, bool callProcessor = true) {
            if (!ArchiveInfos.TryGetValue(archivePath, out var archiveInfo) || archiveInfo.Archive != Processor.CurrentArchive) 
                return false;
            lock (archiveInfoLock) {
                if (callProcessor && archiveInfo.Status is DPArchiveStatus.CancellationPending) {
                    Processor.CancelCurrentArchive();
                    archiveInfo.Status = DPArchiveStatus.CancellationRequested;
                    return true;
                } else if (!callProcessor) archiveInfo.Status = DPArchiveStatus.Cancelled;
            }
            return archiveInfo.Status is DPArchiveStatus.CancellationRequested || 
                   archiveInfo.Status is DPArchiveStatus.Cancelled || 
                   archiveInfo.Status is DPArchiveStatus.CancellationPending;
        }

        private void CreateNewArchiveInfo(IDPArchive archive)
        {
            lock (archiveInfoLock)
            {
                if (ArchiveInfos.ContainsKey(archive.Path))
                    return;
                ArchiveInfos[archive.Path] = new DPArchiveInfo(archive);
            }
        }

        private DPProductRecord? CreateRecords(IDPArchive arc, DPExtractionReport report)
        {
            if (arc.Type != ArchiveType.Product) return null;
            var imageLocation = string.Empty;

            // Extraction Record successful folder/file paths will now be relative to their content folder (if any).
            var successfulFiles = new List<string>(arc.Contents.Count);
            // Folders where a file was extracted underneath it.
            // Ex: Content/Documents/a.txt was extracted, therefore "Documents" is added.
            var foldersExtracted = new HashSet<string>(arc.Contents.Count);

            // Add the paths relative to the content folder.
            foreach (IDPFile file in report.ExtractedFiles)
            {
                successfulFiles.Add(file.RelativePathToContentFolder!);
                if (!string.IsNullOrWhiteSpace(file.RelativePathToContentFolder))
                    foldersExtracted.Add(Path.GetDirectoryName(file.RelativePathToContentFolder)!);
            }
            var erroredFiles = report.ErroredFiles.Keys.Select(x => x.RelativePathToContentFolder!).ToArray();

            if (UserSettings!.DownloadImages == SettingOptions.Yes)
                imageLocation = new DPNetwork().DownloadImage(arc.FileName, TimeSpan.FromSeconds(10));
            else if (UserSettings.DownloadImages == SettingOptions.Prompt)
            {
                // TODO: Use more reliable method! Support files!
                // Pre-check if the archive file name starts with "IM"
                if (arc.FileName.StartsWith("IM"))
                {
                    DialogResult result = MessageBox.Show("Do you wish to download the thumbnail for this product?", "Download Thumbnail Prompt", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes) imageLocation = new DPNetwork().DownloadImage(arc.FileName, TimeSpan.FromSeconds(10));
                }
            }
            
            var author = arc.ProductInfo.Authors.FirstOrDefault(null as string);
            var workingProductRecord = new DPProductRecord(arc.ProductName, arc.ProductInfo.Authors.ToArray(), DateTime.Now, imageLocation, arc.FileName, 
                UserSettings.DestinationPath, arc.ProductInfo.Tags.ToArray(), successfulFiles, 0);
            Program.Database.AddNewRecordEntry(workingProductRecord);
            return workingProductRecord;
        }
    }
}
