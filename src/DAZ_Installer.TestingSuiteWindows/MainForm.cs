using DAZ_Installer.Core;
using DAZ_Installer.IO;
using Serilog;
using System.Text.Json;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using DAZ_Installer.Core.Extraction;
using System.Text;

namespace DAZ_Installer.TestingSuiteWindows
{
    public partial class MainForm : Form
    {
        private record class ProcessSession(DPProcessor Processor, DPArchive Archive, DPProcessSettings Settings, List<DPExtractionReport> Reports);

        public static MainForm Instance = null!;
        DPFileScopeSettings Scope = new DPFileScopeSettings();
        DPProcessSettings settings = new();
        private Task? lastTask;
        private DPArchive? lastRootArchive;
        ProcessSession? lastSession = null;
        DPProcessor? currentProcessor = null;
        CancellationTokenSource tokenSource = new();
        public MainForm()
        {
            Instance = this;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            saveTxtBox.Text = Path.Combine(Environment.CurrentDirectory, "Output");
            settings.TempPath = tempPathTxtBox.Text = Path.Combine(Program.TempPath, "Temp");
            settings.DestinationPath = destPathTxtBox.Text = Path.Combine(Program.TempPath, "Destination");
            settings.ForceFileToDest = new(0);
            settings.ContentFolders = new(DPProcessor.DefaultContentFolders);
            settings.ContentRedirectFolders = new(DPProcessor.DefaultRedirects);
            settings.InstallOption = InstallOptions.ManifestAndAuto;
            settings.OverwriteFiles = true;
            Scope = new(Enumerable.Empty<string>(), new[] { settings.TempPath, settings.DestinationPath }, false, false, true);
            MessageBox.Show("Warning! Do NOT use this application on your main DAZ Studio library. This application is meant for testing purposes only. " +
                "Doing so WILL result in your data being lost!\n\nIf you are unsure, leave the settings at default or as instructed by a contributor of this project. " +
                "YOU HAVE BEEN WARNED!",
                "Warning",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void browseArchiveBtn_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Acceptable Archives|*.7z;*.rar;*.zip;*.001";
            if (openFileDialog1.ShowDialog() != DialogResult.OK) return;
            archiveTxtBox.Text = openFileDialog1.FileName;
        }

        private void browseDestBtn_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select the folder to install product into";
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;
            destPathTxtBox.Text = folderBrowserDialog1.SelectedPath;
        }

        private void browseTempBtn_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select the folder to use for temporary usage";
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;
            destPathTxtBox.Text = folderBrowserDialog1.SelectedPath;
        }

        private void determineBtn_Click(object sender, EventArgs e)
        {
            startTask(determineDestinationsTask);
        }

        private void extractBtn_Click(object sender, EventArgs e)
        {
            startTask(extractTask);
        }

        private void processBtn_Click(object sender, EventArgs e)
        {
            startTask(processTask);
        }

        private void peekBtn_Click(object sender, EventArgs e) => startTask(peekTask);
        private void peekRecursivelyBtn_Click(object sender, EventArgs e)
        {
            startTask(peekRecursivelyTask);
        }

        private void changeProcessBtn_Click(object sender, EventArgs e)
        {
            var a = new ProcessSettingsDialogue(settings);
            a.ShowDialog();
            a.Settings = settings;
        }
        private void saveBrowseBtn_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.Description = "Select the folder to save process output";
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) return;
            saveTxtBox.Text = folderBrowserDialog1.SelectedPath;
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            saveProcess(true);
        }

        private void treeViewMnuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (treeView1.SelectedNode is null) return;
            var processed = treeView1.SelectedNode.ForeColor == Color.Green;
            markAsShouldntProcessToolStripMenuItem.Enabled = processed;
            markAsShouldProcessToolStripMenuItem.Enabled = !processed;
        }

        private void copyNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(treeView1.SelectedNode.Text);
        }

        private void copyFullPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode.Tag as DPAbstractNode;
            Clipboard.SetText(node!.NormalizedPath);
        }

        private void markAsShouldProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.BeginUpdate();
            var color = Color.Green;
            markNodeAndDescendants(treeView1.SelectedNode, ref color, true);
            treeView1.EndUpdate();
        }

        private void markNodeAndDescendants(TreeNode node, ref Color color, bool bold = false)
        {
            node.ForeColor = color;
            if (bold) node.NodeFont = new Font(treeView1.Font, FontStyle.Bold);
            else node.NodeFont = new Font(treeView1.Font, FontStyle.Regular);
            foreach (var child in node.Nodes.Cast<TreeNode>())
            {
                markNodeAndDescendants(child, ref color, bold);
            }
        }

        private void markAsShouldntProcessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.BeginUpdate();
            var color = Color.Black;
            markNodeAndDescendants(treeView1.SelectedNode, ref color);
            treeView1.EndUpdate();
        }

        private bool validateSettings()
        {
            settings.DestinationPath = destPathTxtBox.Text;
            settings.TempPath = tempPathTxtBox.Text;
            var disableWarnings = disableWarningChkBox.Checked;
            var savePath = saveTxtBox.Text;
            if (!disableWarnings && settings.ContentFolders!.Count == 0)
            {
                // Turn this into a warning with a prompt.
                if (MessageBox.Show("You do not have any content folders, do you wish to continue?", "No content folders", MessageBoxButtons.YesNo, MessageBoxIcon.Error) != DialogResult.Yes) return false;
            }
            if (!disableWarnings && settings.ContentRedirectFolders!.Count == 0)
            {
                if (MessageBox.Show("You do not have any content folder aliases, do you wish to continue?", "No content folder aliases", MessageBoxButtons.YesNo, MessageBoxIcon.Error) != DialogResult.Yes) return false;
            }
            if (settings.ContentRedirectFolders.Any(x => !settings.ContentFolders.Contains(x.Value)))
            {
                MessageBox.Show("You cannot have a content folder alias that does not point to a valid content folder.", "Invalid content folder alias", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!Directory.Exists(settings.DestinationPath))
            {
                var a = new DPFileSystem(new DPFileScopeSettings(Enumerable.Empty<string>(), new[] { settings.DestinationPath }));
                var d = a.CreateDirectoryInfo(settings.DestinationPath);
                if (!d.TryCreate())
                {
                    MessageBox.Show("The destination folder does not exist and could not be created.", "Destination folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            if (!Directory.Exists(settings.TempPath))
            {
                var a = new DPFileSystem(new DPFileScopeSettings(Enumerable.Empty<string>(), new[] { settings.TempPath }));
                var d = a.CreateDirectoryInfo(settings.TempPath);
                if (!d.TryCreate())
                {
                    MessageBox.Show("The temp folder does not exist and could not be created.", "Temp folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            if (!Directory.Exists(savePath))
            {
                var a = new DPFileSystem(new DPFileScopeSettings(Enumerable.Empty<string>(), new[] { savePath }));
                var d = a.CreateDirectoryInfo(savePath);
                if (!d.TryCreate())
                {
                    MessageBox.Show("The save output path does not exist and could not be created.", "Temp folder does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            if (!disableWarnings && Directory.EnumerateFileSystemEntries(settings.DestinationPath).Any())
            {
                if (MessageBox.Show("The destination folder is not empty. Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return false;
            }
            if (!disableWarnings && Directory.EnumerateFileSystemEntries(settings.TempPath).Any())
            {
                if (MessageBox.Show("The temporary folder is not empty. Are you sure you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return false;
            }
            if (!File.Exists(archiveTxtBox.Text))
            {
                MessageBox.Show("The archive to test does not exist.", "Archive does not exist", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private DPArchive setupArchive()
        {
            var archiveLocation = archiveTxtBox.Text;
            Scope = new(new[] { archiveLocation }, new[] { settings.TempPath, settings.DestinationPath }, false, false, true);
            return new DPArchive(new DPFileSystem(Scope).CreateFileInfo(archiveLocation));
        }

        private bool startTask(Action a)
        {
            var deleteFiles = deleteFilesChkBox.Checked;
            if (!validateSettings()) return false;
            if (clearLogsChkBox.Checked) logOutputTxtBox.Clear();
            if (deleteFilesChkBox.Checked)
                if (!lastTask?.IsCompleted ?? false)
                {
                    MessageBox.Show("The last task has not completed yet. Please cancel first before continuing.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    return false;
                }
            lastTask = Task.Run(() =>
            {
                try
                {
                    a();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"An error occurred while running task: {a.Method.Name}");
                    MessageBox.Show($"An error occurred while running a task.\nError message: \n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                BeginInvoke(() => updateProgression(false));
                if (deleteFiles) this.deleteFiles();
            });
            updateProgression(true);
            return true;
        }

        private void updateProgression(bool enable = true)
        {
            cancelBtn.Enabled = enable;
            changeProgressionBar(enable);
        }
        private void changeProgressionBar(bool marquee = false)
        {
            progressBar1.Style = marquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            progressBar1.Value = marquee ? 0 : 100;
        }

        private void deleteFiles()
        {
            try
            {
                Directory.Delete(settings.TempPath, true);
                Directory.Delete(settings.DestinationPath, true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while deleting temporary files");
            }
        }

        private void buildTree(DPArchive arc)
        {
            var rootNode = new TreeNode(arc.FileName);
            Queue<TreeNode> queue = new();
            // Add root contents first.
            foreach (var file in arc.RootContents)
            {
                var rootFile = new TreeNode(file.FileName) { Tag = file };
                rootNode.Nodes.Add(rootFile);
            }

            // Setup the TreeNodes for the root folders first.
            foreach (var rootFolder in arc.RootFolders)
            {
                var rootFolderNode = new TreeNode(rootFolder.FileName) { Tag = rootFolder };
                rootNode.Nodes.Add(rootFolderNode);
                queue.Enqueue(rootFolderNode);
            }

            // Now do the rest of the contents starting with the root folders.
            while (queue.Count != 0)
            {
                var parentNode = queue.Dequeue();
                var folder = (DPFolder)parentNode.Tag;
                foreach (var file in folder.Contents)
                {
                    var childNode = new TreeNode(file.FileName) { Tag = file };
                    parentNode.Nodes.Add(childNode);
                }
                foreach (var subFolder in folder.subfolders)
                {
                    var childNode = new TreeNode(subFolder.FileName) { Tag = subFolder };
                    parentNode.Nodes.Add(childNode);
                    queue.Enqueue(childNode);
                }
            }

            // Add it to the tree.
            try
            {
                // If this is the root archive, then we need to pause drawing operations.
                if (arc.AssociatedArchive is null) BeginInvoke(treeView1.BeginUpdate);

                // Let the UI thread begin adding the nodes (we are on a thread pool thread).
                var result = BeginInvoke(() => treeView1.Nodes.Add(rootNode));

                // Add the subarchives.
                foreach (var subArchive in arc.Subarchives)
                {
                    buildTree(subArchive);
                }

                // Wait for the tree to finish updating.
                result.AsyncWaitHandle.WaitOne();
            }
            finally
            {
                // We need to resume drawing operations at all costs.
                if (arc.AssociatedArchive is null) BeginInvoke(treeView1.EndUpdate);
            }


        }

        private void saveProcess(bool manual = false)
        {
            if (lastSession is null) return;
            var time = DateTime.Now.ToString("d-m-yyyy-hh-mm-ss");
            if (!Directory.Exists(saveTxtBox.Text))
            {
                try
                {
                    Directory.CreateDirectory(saveTxtBox.Text);
                }
                catch
                {
                    MessageBox.Show("Failed to save process output. The directory does not exist and could not be created.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            var path = Path.Combine(saveTxtBox.Text, $"{lastSession.Archive.FileName}_results_{time}.json");
            if (manual)
                File.WriteAllTextAsync(path, ResultCompiler.CompileResults(treeView1.Nodes, lastSession.Settings, lastSession.Archive));
            else
                File.WriteAllTextAsync(path, ResultCompiler.CompileResults(lastSession.Reports, lastSession.Settings, lastSession.Archive));
        }

        private void colorDetermined(HashSet<DPFile> determinedFiles)
        {
            List<TreeNode> nodes = new(determinedFiles.Count);
            // initialize the queue with root-level nodes first; the root-level nodes
            // represent all of the archives processed (which can happen if there are multiple archives in the root)
            Queue<TreeNode> queue = new Queue<TreeNode>(treeView1.Nodes.Cast<TreeNode>());

            while (queue.Count > 0)
            {
                var treeNode = queue.Dequeue();
                var skipped = false;

                // Each node could represent a file or a folder.
                foreach (TreeNode node in treeNode.Nodes)
                {
                    var folder = node.Tag as DPFolder;
                    var file = node.Tag as DPFile;
                    if (folder is not null) queue.Enqueue(node);
                    else
                    {
                        if (determinedFiles.Contains(file!))
                            nodes.Add(node);
                        else skipped = true;
                    }
                }
                // If we didn't skip anything, then also mark the entire folder.
                if (!skipped) nodes.Add(treeNode);
            }


            BeginInvoke(() =>
            {
                treeView1.BeginUpdate();
                foreach (var node in nodes)
                {
                    node.ForeColor = Color.Blue;
                }
                treeView1.EndUpdate();
            });
        }

        private void colorExtractedToTarget(DPArchive arc)
        {
            List<TreeNode> nodes = new(arc.Contents.Count);
            // initialize the queue with root-level nodes first; the root-level nodes
            // represent all of the archives processed (which can happen if there are multiple archives in the root)
            Queue<TreeNode> queue = new Queue<TreeNode>(treeView1.Nodes.Cast<TreeNode>());

            while (queue.Count > 0)
            {
                var treeNode = queue.Dequeue();
                var skipped = false;

                // Each node could represent a file or a folder.
                foreach (TreeNode node in treeNode.Nodes)
                {
                    var folder = node.Tag as DPFolder;
                    var file = node.Tag as DPFile;
                    if (folder is not null) queue.Enqueue(node);
                    else
                    {
                        if (file!.ExtractedToTarget) nodes.Add(node);
                        else skipped = true;
                    }
                }
                // If we didn't skip anything, then also mark the entire folder.
                if (!skipped) nodes.Add(treeNode);
            }

            BeginInvoke(() =>
            {
                treeView1.BeginUpdate();
                foreach (var node in nodes)
                {
                    node.ForeColor = Color.Green;
                    node.NodeFont = new Font(treeView1.Font, FontStyle.Bold);
                }
                treeView1.EndUpdate();
            });
        }

        private void listProcessedFiles(List<DPExtractionReport> reports)
        {
            if (reports.Count == 0 || reports[0].ExtractedFiles.Count == 0)
            {
                BeginInvoke(processedTxtBox.Clear);
                return;
            }
            var sb = new StringBuilder(reports.Count * (1 + reports[0].ExtractedFiles.Last().Path.Length));
            foreach (var report in reports)
            {
                foreach (var file in report.ExtractedFiles)
                {
                    sb.Append(file.AssociatedArchive!.FileName).Append(':').AppendLine(file.Path);
                }
            }
            var str = sb.ToString();
            BeginInvoke(() => processedTxtBox.Text = str);
        }

        #region Tasks
        private void peekTask()
        {
            lastRootArchive = setupArchive();
            var settings = new
            {
                ContentFolders = this.settings.ContentFolders,
                ContentRedirectFolders = this.settings.ContentRedirectFolders,
                InstallOption = this.settings.InstallOption,
                OverwriteFiles = this.settings.OverwriteFiles,
                ForceFileToDest = this.settings.ForceFileToDest,
                TempPath = this.settings.TempPath,
                DestinationPath = this.settings.DestinationPath
            };
            Log.Information("Beginning to peek at archive contents.");
            Log.Information("Archive File: {lastRootArchive}", lastRootArchive.FileName);
            Log.Information("Settings to use: \n{@Settings}", JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            try
            {
                lastRootArchive.Extractor!.CancellationToken = tokenSource.Token;
                lastRootArchive.PeekContents();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while peeking at archive contents");
            }
            Log.Information("Finished peeking at archive contents.");
            BeginInvoke(treeView1.Nodes.Clear);
            buildTree(lastRootArchive);
        }

        private void determineDestinationsTask()
        {
            lastRootArchive ??= setupArchive();
            var settings = new
            {
                ContentFolders = this.settings.ContentFolders,
                ContentRedirectFolders = this.settings.ContentRedirectFolders,
                InstallOption = this.settings.InstallOption,
                OverwriteFiles = this.settings.OverwriteFiles,
                ForceFileToDest = this.settings.ForceFileToDest,
                TempPath = this.settings.TempPath,
                DestinationPath = this.settings.DestinationPath
            };
            Log.Information("Beginning to determine destinations.");
            Log.Information("Archive File: {lastRootArchive}", lastRootArchive.FileName);
            Log.Information("Settings to use: \n{@Settings}", JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            HashSet<DPFile> determinedFiles = new();
            try
            {
                if (lastRootArchive.Contents.Count == 0) peekRecursivelyTask();
                determinedFiles = new RecursiveDestinationDeterminer().DetermineDestinations(lastRootArchive, this.settings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while determining destinations");
            }
            Log.Information("Finished determining destinations.");
            BeginInvoke(treeView1.Nodes.Clear);
            buildTree(lastRootArchive);
            if (determinedFiles.Count != 0) colorDetermined(determinedFiles);
        }

        private void extractTask()
        {
            lastRootArchive = setupArchive();
            var settings = new
            {
                ContentFolders = this.settings.ContentFolders,
                ContentRedirectFolders = this.settings.ContentRedirectFolders,
                InstallOption = this.settings.InstallOption,
                OverwriteFiles = this.settings.OverwriteFiles,
                ForceFileToDest = this.settings.ForceFileToDest,
                TempPath = this.settings.TempPath,
                DestinationPath = this.settings.DestinationPath
            };
            Log.Information("Beginning to extract archive.");
            Log.Information("Archive File: {lastRootArchive}", lastRootArchive.FileName);
            Log.Information("Settings to use: \n{@Settings}", JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            try
            {
                lastRootArchive.Extractor!.CancellationToken = tokenSource.Token;
                lastRootArchive.PeekContents();
                var extractSettings = new DPExtractSettings(settings.TempPath, lastRootArchive.Contents.Values);

                // Set the TargetPath for each file.
                foreach (var file in lastRootArchive.Contents.Values)
                {
                    file.TargetPath = Path.Combine(settings.DestinationPath, file.Path);
                }

                lastRootArchive.ExtractContents(extractSettings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while extracting archive");
            }
            Log.Information("Finished extracting archive.");
            BeginInvoke(treeView1.Nodes.Clear);
            buildTree(lastRootArchive);
            colorExtractedToTarget(lastRootArchive);
        }

        private void processTask()
        {
            var autoSave = autoSaveBtn.Checked;
            var arcs = new List<DPArchive>();
            var records = new List<DPExtractionReport>();
            var settings = new
            {
                ContentFolders = this.settings.ContentFolders,
                ContentRedirectFolders = this.settings.ContentRedirectFolders,
                InstallOption = this.settings.InstallOption,
                OverwriteFiles = this.settings.OverwriteFiles,
                ForceFileToDest = this.settings.ForceFileToDest,
                TempPath = this.settings.TempPath,
                DestinationPath = this.settings.DestinationPath
            };
            Log.Information("Beginning to process archive.");
            Log.Information("Archive File: {lastRootArchive}", archiveTxtBox.Text);
            Log.Information("Settings to use: \n{@Settings}", JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            var processor = currentProcessor = new DPProcessor();
            try
            {
                processor.ArchiveEnter += (_, a) => arcs.Add(a.Archive);
                processor.ArchiveExit += (_, a) =>
                {
                    if (a.Processed) records.Add(a.Report!);
                };
                processor.ProcessArchive(archiveTxtBox.Text, this.settings);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while extracting archive");
            }
            Log.Information("Finished processing archive.");
            BeginInvoke(treeView1.Nodes.Clear);
            currentProcessor = null;
            if (arcs.Count == 0) return;
            lastRootArchive = arcs[0];
            lastSession = new(processor, lastRootArchive, this.settings, records);
            if (autoSave) saveProcess();
            buildTree(arcs[0]);
            colorExtractedToTarget(arcs[0]);
            listProcessedFiles(records);
        }

        private void peekRecursivelyTask()
        {
            lastRootArchive = setupArchive();
            var settings = new
            {
                ContentFolders = this.settings.ContentFolders,
                ContentRedirectFolders = this.settings.ContentRedirectFolders,
                InstallOption = this.settings.InstallOption,
                OverwriteFiles = this.settings.OverwriteFiles,
                ForceFileToDest = this.settings.ForceFileToDest,
                TempPath = this.settings.TempPath,
                DestinationPath = this.settings.DestinationPath
            };
            Log.Information("Beginning to peek recursively.");
            Log.Information("Archive File: {lastRootArchive}", lastRootArchive.FileName);
            Log.Information("Settings to use: \n{@Settings}", JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
            var queue = new Queue<DPArchive>();
            var arcTrees = new List<DPArchive>();
            queue.Enqueue(lastRootArchive);
            try
            {
                var token = tokenSource.Token;
                while (queue.Count != 0)
                {
                    if (token.IsCancellationRequested) break;
                    var arc = queue.Dequeue();
                    arc.Extractor!.CancellationToken = tokenSource.Token;
                    arc.PeekContents(settings.TempPath);
                    arcTrees.Add(arc);
                    foreach (var nestedArc in arc.Subarchives)
                    {
                        queue.Enqueue(nestedArc);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while peeking at archive contents");
            }
            Log.Information("Finished recursive peek.");
            BeginInvoke(treeView1.Nodes.Clear);
            buildTree(lastRootArchive);
        }

        #endregion

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data is not null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data is null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length != 1) return;
            archiveTxtBox.Text = files[0];
        }

        private void cancelBtn_Click(object sender, EventArgs e)
        {
            currentProcessor?.CancelProcessing();
            lastSession?.Processor.CancelProcessing();
            tokenSource.Cancel();
            tokenSource = new();
        }
    }
}
