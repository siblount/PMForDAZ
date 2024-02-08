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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.DP
{
    internal class DPExtractJob
    {
        public static ILogger Logger { get; set; } = Log.Logger.ForContext<DPExtractJob>();
        public List<string> FilesToProcess { get; init; }
        public bool Completed { get; protected set; } = false;
        public DPProcessor Processor = new();
        public Task TaskJob { get; protected set; }
        public DPSettings UserSettings { get; protected set; }

        public static DPTaskManager extractJobs = new();
        private ProgressCombo progressCombo = Extract.ExtractPage.progressCombo;
        public static Queue<DPExtractJob> Jobs { get; } = new Queue<DPExtractJob>();
        // TODO: Check if a product is already in list.

        //public DPExtractJob(string[] files)
        //{
        //    filesToProcess = files;
        //}

        public DPExtractJob(ListView.ListViewItemCollection files)
        {
            Jobs.Enqueue(this);
            var _files = new List<string>(files.Count);

            foreach (ListViewItem file in files)
                _files.Add(file.Text);
            FilesToProcess = _files;
        }
        public Task DoJob()
        {
            TaskJob = extractJobs.AddToQueue(ProcessListAsync);
            return TaskJob;
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
            if (Processor.State == ProcessorState.PreparingExtraction)
            {
                // TO DO: Highlight files in red for files that failed to extract.
                Extract.ExtractPage.BeginInvoke(() =>
                {
                    Extract.ExtractPage.SuspendLayout();
                    try
                    {
                        Extract.ExtractPage.AddToList(Processor.CurrentArchive);
                        Extract.ExtractPage.AddToHierachy(Processor.CurrentArchive);
                        progressCombo.ChangeProgressBarStyle(true);
                        progressCombo.SetText($"Preparing to extract contents in {Processor.CurrentArchive.FileName}...");
                        progressCombo.SetProgress(0);
                    } catch (Exception ex)
                    {
                        Logger.Error(ex, "An error occurred while attempting to add archive to list");
                    } finally
                    {
                        Extract.ExtractPage.ResumeLayout();
                    }
                });
            }
            else if (Processor.State == ProcessorState.Analyzing)
            {
                progressCombo.ChangeProgressBarStyle(true);
                progressCombo.SetText($"Analyzing file contents in {Processor.CurrentArchive.FileName}...");
            }
        }

        private void Processor_ExtractProgress(object sender, DPExtractProgressArgs e)
        {
            progressCombo.ChangeProgressBarStyle(false);
            progressCombo.SetProgress(e.ExtractionPercentage);
            progressCombo.SetText($"Extracting contents from {e.Archive.FileName}...{e.ExtractionPercentage}%");
        }

        private void Processor_MoveProgress(DPProcessor sender, DPExtractProgressArgs e)
        {
            progressCombo.ChangeProgressBarStyle(true);
            progressCombo.SetText($"Moving files from {e.Archive.FileName} to destination...%");
        }

        private void Processor_ProcessError(object sender, DPProcessorErrorArgs e)
        {
            MessageBox.Show("An unexpected error occurred while processing the archive.\n" +
                            $"Error: {e.Explaination}\n" +
                            $"Stack Trace: {e.Ex?.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void Processor_ArchiveExit(object sender, DPArchiveExitArgs e)
        {
            // TODO: Do stuff w/ archive.
            if (!e.Processed) return;
            // Create records if applicable.
            // TODO: Only add if successful extraction, and all files from temp were moved, and/or user didn't cancel operation.
            progressCombo.ChangeProgressBarStyle(true);
            Logger.Information("Creating records for {arc}", e.Archive.FileName);
            progressCombo.SetText($"Creating records for {e.Archive.FileName}...");
            CreateRecords(e.Archive, e.Report!);

            switch (UserSettings.PermDeleteSource)
            {
                case SettingOptions.Yes:
                    RemoveSourceFiles();
                    break;
                case SettingOptions.Prompt:
                    DialogResult result = MessageBox.Show("Do you wish to PERMENATELY delete all of the source files regardless if it was extracted or not? This cannot be undone.",
                        "Delete soruce files", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes) RemoveSourceFiles();
                    break;
            }
        }

        private void Processor_ArchiveEnter(object sender, DPArchiveEnterArgs e)
        {
            if (Program.Database.ArchiveFileNames.Contains(e.Archive.FileName))
            {
                switch (UserSettings.InstallPrevProducts)
                {
                    case SettingOptions.No:
                        Processor.CancelCurrentArchive();
                        break;
                    case SettingOptions.Prompt:
                        DialogResult result = MessageBox.Show($"It seems that \"{e.Archive.FileName}\" was already processed. " +
                            $"Do you wish to continue processing this file?", "Archive already processed",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (result == DialogResult.No) Processor.CancelCurrentArchive();
                        break;
                }
            }
        }

        private void ProcessListAsync(CancellationToken t)
        {
            try
            {
                // Tell the progress combo we are beginning by enabling visibility of the progress bar and cancel button.
                progressCombo.StartProgress();

                // Register the cancellation token so we can cancel the process.
                var token = progressCombo.CancellationTokenSource.Token;
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
                    ForceFileToDest = new Dictionary<DPFile, string>(0),
                };
                SetupEventHandlers();

                var c = FilesToProcess.Count;
                for (var i = 0; i < c; i++)
                {
                    var x = FilesToProcess[i];
                    int percentage = (int)((double)i / c * 100);
                    progressCombo.SetProgress(percentage);
                    progressCombo.SetText($"Processing archive {i + 1}/{c}: " +
                        $"{Path.GetFileName(x)}...({percentage}%)");
                    Processor.ProcessArchive(x, processSettings);
                }

                // Update the database after this run.
                Program.Database.GetInstalledArchiveNamesQ();
            } catch (Exception ex)
            {
                Logger.Error(ex, "An error occurred while attempting to process archive list");
            } finally
            {
                progressCombo.SetText($"Finished processing archives");
                progressCombo.ChangeProgressBarStyle(false);
                progressCombo.SetProgress(100);
                progressCombo.EndProgress();

                Completed = true;
                Jobs.Dequeue();
                GC.Collect();
            }
            
        }

        public void RemoveSourceFiles()
        {
            var scopeSettings = new DPFileScopeSettings(FilesToProcess, Array.Empty<string>(), false, true);
            var fs = new DPFileSystem(scopeSettings);

            foreach (var file in FilesToProcess)
            {
                Exception? ex = null;
                if (UserSettings.DeleteAction == RecycleOption.DeletePermanently)
                    fs.CreateFileInfo(file).TryAndFixDelete(out ex);
                else
                {
                    try
                    {
                        if (fs.Scope.IsFilePathWhitelisted(file)) 
                            FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    } catch (Exception e) { ex = e; }
                }
                if (ex != null)
                    Logger.Error(ex, "Failed to delete source file: {file}", file);
            }
        }

        private DPProductRecord? CreateRecords(DPArchive arc, DPExtractionReport report)
        {
            if (arc.Type != ArchiveType.Product) return null;
            var imageLocation = string.Empty;

            // Extraction Record successful folder/file paths will now be relative to their content folder (if any).
            var successfulFiles = new List<string>(arc.Contents.Count);
            // Folders where a file was extracted underneath it.
            // Ex: Content/Documents/a.txt was extracted, therefore "Documents" is added.
            var foldersExtracted = new HashSet<string>(arc.Contents.Count);

            // Add the paths relative to the content folder.
            foreach (DPFile file in report.ExtractedFiles)
            {
                successfulFiles.Add(file.RelativePathToContentFolder!);
                if (!string.IsNullOrWhiteSpace(file.RelativePathToContentFolder))
                    foldersExtracted.Add(Path.GetDirectoryName(file.RelativePathToContentFolder)!);
            }
            var erroredFiles = report.ErroredFiles.Keys.Select(x => x.RelativePathToContentFolder!).ToArray();

            if (UserSettings.DownloadImages == SettingOptions.Yes)
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
