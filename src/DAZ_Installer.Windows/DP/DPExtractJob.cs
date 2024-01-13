// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.Database;
using DAZ_Installer.IO;
using DAZ_Installer.Windows.Pages;
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
        private DPProgressCombo ProgressCombo;

        public static DPTaskManager extractJobs = new();
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
            Processor.FileError += Processor_FileError;
            Processor.ExtractProgress += Processor_ExtractProgress;
            Processor.MoveProgress += Processor_MoveProgress;
        }



        private void Processor_StateChanged()
        {
            if (Processor.State == ProcessorState.PreparingExtraction)
            {
                // TO DO: Highlight files in red for files that failed to extract.
                Extract.ExtractPage.AddToList(Processor.CurrentArchive);
                Extract.ExtractPage.AddToHierachy(Processor.CurrentArchive);
                ProgressCombo.ChangeProgressBarStyle(true);
                ProgressCombo.UpdateText($"Preparing to extract contents in {Processor.CurrentArchive.FileName}...");
                ProgressCombo.ProgressBar.Value = 0;
            }
            else if (Processor.State == ProcessorState.Analyzing)
            {
                ProgressCombo.ChangeProgressBarStyle(true);
                ProgressCombo.UpdateText($"Analyzing file contents in {Processor.CurrentArchive.FileName}...");
            }
        }

        private void Processor_ExtractProgress(object sender, DPExtractProgressArgs e)
        {
            if (ProgressCombo.ProgressBar.Style == ProgressBarStyle.Marquee)
                ProgressCombo.ChangeProgressBarStyle(false);
            ProgressCombo.ProgressBar.Value = e.ExtractionPercentage;
            ProgressCombo.UpdateText($"Extracting contents from {e.Archive.FileName}...{e.ExtractionPercentage}%");
        }

        private void Processor_MoveProgress(DPProcessor sender, DPExtractProgressArgs e)
        {
            if (ProgressCombo.ProgressBar.Style != ProgressBarStyle.Marquee)
                ProgressCombo.ChangeProgressBarStyle(true);
            ProgressCombo.UpdateText($"Moving files from {e.Archive.FileName} to destination...%");
        }

        private void Processor_FileError(object sender, DPErrorArgs e) =>
            // TODO: Log error?
            throw new NotImplementedException();

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
            ProgressCombo.ChangeProgressBarStyle(true);
            Logger.Information("Creating records for {arc}", e.Archive.FileName);
            ProgressCombo.UpdateText($"Creating records for {e.Archive.FileName}...");
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
            ProgressCombo = new DPProgressCombo();
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
                ProgressCombo.ProgressBar.Value = (int)((double)i / c * 100);
                ProgressCombo.UpdateText($"Processing archive {i + 1}/{c}: " +
                    $"{Path.GetFileName(x)}...({ProgressCombo.ProgressBar.Value}%)");
                Processor.ProcessArchive(x, processSettings);
            }
            ProgressCombo.UpdateText($"Finished processing archives");
            ProgressCombo.ChangeProgressBarStyle(false);
            ProgressCombo.ProgressBar.Value = 100;
            Completed = true;
            Jobs.Dequeue();
            GC.Collect();
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
                    if (File.Exists(file)) File.Delete(file);
                }
                catch (UnauthorizedAccessException ex)
                {
                    if (del)
                    {
                        del = !del;
                        continue;
                    }
                    // Check to see if the file has a read-only attribute.
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
            var workingExtractionRecord = new DPExtractionRecord(arc.FileName,
                                                                 UserSettings.DestinationPath,
                                                                 successfulFiles.ToArray(),
                                                                 erroredFiles,
                                                                 report.ErroredFiles.Values.ToArray(),
                                                                 foldersExtracted.ToArray(),
                                                                 0);

            if (UserSettings.DownloadImages == SettingOptions.Yes)
                imageLocation = DPNetwork.DownloadImage(workingExtractionRecord.ArchiveFileName);
            else if (UserSettings.DownloadImages == SettingOptions.Prompt)
            {
                // TODO: Use more reliable method! Support files!
                // Pre-check if the archive file name starts with "IM"
                if (workingExtractionRecord.ArchiveFileName.StartsWith("IM"))
                {
                    DialogResult result = MessageBox.Show("Do you wish to download the thumbnail for this product?", "Download Thumbnail Prompt", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes) imageLocation = DPNetwork.DownloadImage(workingExtractionRecord.ArchiveFileName);
                }
            }
            
            var author = arc.ProductInfo.Authors.FirstOrDefault(null as string);
            var workingProductRecord = new DPProductRecord(arc.ProductName, arc.ProductInfo.Tags.ToArray(), author,
                                        null, DateTime.Now, imageLocation, 0, 0);
            Program.Database.AddNewRecordEntry(workingProductRecord, workingExtractionRecord);
            return workingProductRecord;
        }
    }
}
