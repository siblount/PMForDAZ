// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using DAZ_Installer.UI;
using DAZ_Installer.Windows.DP;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace DAZ_Installer.Windows.Pages
{

    public partial class Extract : UserControl, IExtractView
    {
        /// <summary>
        /// The singleton instance of this user control.
        /// </summary>
        /// <remarks>
        /// There should not be more than one instance of this component.
        /// </remarks>
        public static Extract ExtractPage = null!;
        /// <summary>
        /// The logger for this class.
        /// </summary>
        public static ILogger Logger = Log.ForContext<Extract>();
        internal static Dictionary<IDPAbstractNode, ListViewItem> associatedListItems = new(256);
        internal static Dictionary<IDPAbstractNode, TreeNode> associatedTreeNodes = new(256);
        internal static Dictionary<string, ListViewItem> associatedQueueItems = new(64, PathComparer.Instance);
        internal static List<DPExtractJob> extractJobs = new(4);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal IDPQueueController QueueController { get; init; }
        internal bool fileHierachyShowing = false;
        internal bool fileListShowing = false;
        internal bool errorListShowing = false;

        /// <summary>
        /// The constructor for the Extract component.
        /// </summary>
        public Extract()
        {
            InitializeComponent();
            QueueController = new DPQueueController(new ListViewAdapter(queueListView),
                                                    this,
                                                    viewFileHierachyToolStripMenuItem,
                                                    viewFileListToolStripMenuItem,
                                                    viewErrorsToolStripMenuItem,
                                                    viewStripSeperator,
                                                    cancelExtractJobToolStripMenuItem,
                                                    cancelCurrentExtractJobToolStripMenuItem,
                                                    skipArchiveToolStripMenuItem,
                                                    cancelCurrentArchiveToolStripMenuItem,
                                                    statusIcons);
            ExtractPage = this;
            tabControl1.TabPages.Remove(fileListPage);
            tabControl1.TabPages.Remove(fileHierachyPage);
            tabControl1.TabPages.Remove(errorsPage);
            var errorImage = SystemIcons.Error.ToBitmap(); // Do not dispose Handle
            var warningImage = SystemIcons.Warning.ToBitmap(); // Do not dispose Handle
            statusIcons.Images.Add("error", errorImage);
            statusIcons.Images.Add("warning", warningImage);

            // Enable double buffering on the queueListView to reduce flickering
            typeof(ListView).GetProperty("DoubleBuffered", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.SetValue(queueListView, true);
        }

        /// <inheritdoc/>
        public void AddToQueue(DPExtractJob job)
        {
            if (InvokeRequired) BeginInvoke(() => QueueController.AddJob(job));
            else QueueController.AddJob(job);
        }

        /// <inheritdoc/>
        public void OnExtractJobStatusUpdate(DPExtractJob caller, DPArchiveInfo info)
        {
            if (InvokeRequired) BeginInvoke(() => QueueController.UpdateView(caller, info));
            else QueueController.UpdateView(caller, info);
        }

        /// <summary>
        /// Adds all the contents found in <paramref name="archive"/> to the list view.
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        public void AddToList(IDPArchive archive)
        {
            fileListView.BeginUpdate();
            try
            {
                foreach (IDPFile content in archive.Contents.Values)
                {
                    ListViewItem item = fileListView.Items.Add($"{archive.FileName}\\{content.Path}");
                    item.Tag = content;
                    associatedListItems[content] = item;
                }
                fileListView.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occurred while adding an archive to list");
            }
            finally
            {
                fileListView.EndUpdate();
            }
        }

        /// <summary>
        /// Process the child nodes of <paramref name="folder"/> and add them to <paramref name="parentNode"/>.
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="parentNode"></param>
        private void ProcessChildNodes(IDPFolder folder, TreeNode parentNode)
        {
            var fileName = Path.GetFileName(folder.Path);
            // We don't need associations for folders.
            var folder1 = parentNode.Nodes.Add(fileName);
            AddIcon(folder1, null);

            // Add the DPFiles.
            foreach (IDPFile file in folder.Contents)
            {
                fileName = Path.GetFileName(file.Path);
                // TO DO: Add condition if file is a DPArchive & extract == true
                TreeNode node = folder1.Nodes.Add(fileName);
                node.Tag = file;
                associatedTreeNodes[file] = node;
                AddIcon(node, file.Ext);
            }
            foreach (IDPFolder subfolder in folder.Subfolders)
                ProcessChildNodes(subfolder, folder1);
        }

        /// <summary>
        /// Adds the contents of <paramref name="workingArchive"/> to the file hierachy tree.
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        /// <param name="workingArchive">The archive to add to the hierachy</param>
        public void AddToHierachy(IDPArchive workingArchive)
        {
            if (InvokeRequired)
            {
                BeginInvoke(AddToHierachy, workingArchive);
                return;
            }
            fileHierachyTree.BeginUpdate();
            try
            {
                // Add root node for DPArchive.
                var fileName = workingArchive.FileName;
                TreeNode rootNode = fileHierachyTree.Nodes.Add(fileName);
                rootNode.Tag = workingArchive;
                associatedTreeNodes[workingArchive] = rootNode;
                AddIcon(rootNode, workingArchive.Ext);

                // Add any files that aren't in any folder.
                foreach (IDPFile file in workingArchive.RootContents)
                {
                    fileName = Path.GetFileName(file.Path);
                    TreeNode node = rootNode.Nodes.Add(fileName);
                    node.Tag = file;
                    associatedTreeNodes[file] = node;
                    AddIcon(node, file.Ext);
                }

                // Recursively add files & folder within each folder.
                foreach (IDPFolder folder in workingArchive.RootFolders)
                    ProcessChildNodes(folder, rootNode);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occurred while attempting to add an archive to hieracy");
            }
            finally
            {
                fileHierachyTree.EndUpdate();
            }
        }

        /// <summary>
        /// Assigns an icon to the <paramref name="node"/> based on the <paramref name="ext"/> of the file.
        /// This only assigns icons for archives and folders. <br/>
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        /// <param name="node">The node to set the icon to</param>
        /// <param name="ext">The extension used to determine the icon to set (7z, zip, rar, null, or "").</param>
        private static void AddIcon(TreeNode node, string? ext)
        {
            if (string.IsNullOrEmpty(ext))
                node.StateImageIndex = 0;
            else if (ext.Contains("zip") || ext.Contains("7z"))
                node.StateImageIndex = 2;
            else if (ext.Contains("rar"))
                node.StateImageIndex = 1;
        }

        /// <inheritdoc/>
        public void ResetExtractPage()
        {
            // Later show nothing to extract panel.
            progressCombo.EndProgress();
            fileListView.Items.Clear();
            fileHierachyTree.Nodes.Clear();
            associatedListItems.Clear();
            associatedTreeNodes.Clear();
        }

        /// <inheritdoc/>
        public void OnProcessorStateUpdate(IDPProcessor processor)
        {
            var arc = processor.CurrentArchive;
            if (InvokeRequired) BeginInvoke(() => HandleProcessorUpdate(processor, arc));
            else HandleProcessorUpdate(processor, arc);
        }

        private void HandleProcessorUpdate(IDPProcessor processor, IDPArchive? archive)
        {
            if (archive is null)
            {
                Logger.Error("HandleProcessorUpdate got null archive, dismissing handling processor update");
                return;
            }
            SuspendLayout();
            try
            {
                switch (processor.State)
                {
                    case ProcessorState.PreparingExtraction:
                        progressCombo.ChangeProgressBarStyle(true);
                        progressCombo.SetText($"Preparing to extract contents in {archive.FileName}...");
                        progressCombo.SetProgress(0);
                        break;
                    case ProcessorState.Analyzing:
                        progressCombo.ChangeProgressBarStyle(true);
                        progressCombo.SetText($"Analyzing file contents in {archive.FileName}...");
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An error occured while handling processor update for state {state}", processor.State);
            }
            finally
            {
                ResumeLayout();
            }
        }

        /// <inheritdoc/>
        public void OnProcessorFinished(IDPExtractJob job)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnProcessorFinished(job));
                return;
            }
            QueueController.UpdateView(job);
        }

        /// <inheritdoc/>
        public void OnCreatingRecords(IDPArchive archive)
        {
            progressCombo.ChangeProgressBarStyle(true);
            progressCombo.SetText($"Creating records for {archive.FileName}...");
        }

        /// <inheritdoc/>
        public void OnExtractionProgressUpdate(IDPProcessor processor, DPExtractProgressArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnExtractionProgressUpdate(processor, e));
                return;
            }
            progressCombo.ChangeProgressBarStyle(false);
            progressCombo.SetProgress(e.ExtractionPercentage);
            progressCombo.SetText($"Extracting contents from {e.Archive.FileName}...{e.ExtractionPercentage}%");
        }

        /// <inheritdoc/>
        public void OnMoveProgressUpdate(IDPProcessor processor, DPExtractProgressArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnMoveProgressUpdate(processor, e));
                return;
            }
            progressCombo.ChangeProgressBarStyle(true);
            progressCombo.SetText($"Moving files from {e.Archive.FileName} to destination...%");
        }

        /// <inheritdoc/>
        public void ShowFileHierachyTab(IDPArchive archive)
        {
            if (archive.Contents.Count == 0) return;
            if (InvokeRequired)
            {
                BeginInvoke(() => ShowFileHierachyTab(archive));
                return;
            }

            try
            {
                fileHierachyTree.BeginUpdate();
                if (fileHierachyShowing)
                    fileHierachyTree.Nodes.Clear();
                AddToHierachy(archive);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to add the archive contents to file hierachy");
            }
            finally
            {
                fileHierachyTree.EndUpdate();
            }

            if (!fileHierachyShowing)
            {
                tabControl1.Controls.Add(fileHierachyPage);
                fileHierachyShowing = true;
            }
            tabControl1.SelectedTab = fileHierachyPage;
        }

        /// <inheritdoc/>
        public void ShowFileListTab(IDPArchive archive)
        {
            if (archive.Contents.Count == 0) return;
            if (InvokeRequired)
            {
                BeginInvoke(() => ShowFileListTab(archive));
                return;
            }

            try
            {
                fileListView.BeginUpdate();
                if (fileListShowing)
                    fileListView.Items.Clear();
                AddToList(archive);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to add the archive contents to file list");
            }
            finally
            {
                fileListView.EndUpdate();
            }

            if (!fileListShowing)
            {
                tabControl1.Controls.Add(fileListPage);
                fileListShowing = true;
            }
            tabControl1.SelectedTab = fileListPage;
        }

        /// <inheritdoc/>
        public void ShowErrorsTab(DPArchiveInfo info)
        {
            if (info.Errors.Count is 0) return;

            var uniqueErrors = DPArchiveInfo.GetUniqueErrors(info.Errors).ToArray();

            // Create a DataTable to bind to the DataGridView
            var errorTable = new DataTable();
            errorTable.Columns.Add("Error", typeof(string));
            errorTable.Columns.Add("Explanation", typeof(string));

            // Populate the DataTable with error information
            foreach (var error in uniqueErrors)
            {
                // If no exception was thrown but an explanation exists, make the explanation the 'error'
                var errorText = error.Exception is not null ? error.Exception.Message : error.Explanation;
                errorText ??= "An unknown error has occured";

                // If the explanation is the error, then don't put it in the explanation column
                string? explanationText = null;
                if (errorText == error.Exception?.Message && error.Explanation is not null)
                    explanationText = error.Explanation;

                errorTable.Rows.Add(errorText, explanationText);
            }

            void SetupDataGrid(DataTable dataTable)
            {
                try
                {
                    if (errorListShowing) errorsPage.SuspendLayout();

                    // Configure the DataGridView
                    if (errorDataGridView.DataSource is DataTable table) table.Dispose();
                    errorDataGridView.DataSource = null;
                    errorDataGridView.AutoGenerateColumns = true;

                    // Set up multi-line support
                    errorDataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                    errorDataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader;

                    // Apply the data source
                    errorDataGridView.DataSource = dataTable;

                    // Configure columns for multi-line text
                    foreach (DataGridViewColumn column in errorDataGridView.Columns)
                    {
                        column.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to add errors to error grid");
                }
                finally
                {
                    if (errorListShowing) errorsPage.ResumeLayout();
                }

                if (!errorListShowing)
                {
                    tabControl1.Controls.Add(errorsPage);
                    errorListShowing = true;
                }

                tabControl1.SelectedTab = errorsPage;
            }

            if (InvokeRequired) BeginInvoke(() => SetupDataGrid(errorTable));
            else SetupDataGrid(errorTable);
        }

        private static void OpenFileInExplorer(string path)
        {
            if (!File.Exists(path))
            {
                MessageBox.Show($"{path} does not exist or cannot be accessed due to insufficient permissions", "Failed to open in file explorer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error("Failed to open file in explorer due to file not existing (or insufficient permissions): {file}", path);
            }
            try
            {
                Process.Start(@"explorer.exe", $"/select, \"{path}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open file due to an unknown error", "Failed to open in file explorer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error(ex, "Failed to open path in explorer");
            }
        }

        #region Context Strip Events
        private void selectInHierachyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            var file = fileListView.SelectedItems[0].Tag as IDPAbstractNode;

            if (file != null && associatedTreeNodes.TryGetValue(file, out var node))
                fileHierachyTree.SelectedNode = node;
            // Switch tab.
            tabControl1.SelectTab(fileHierachyPage);
        }

        private void fileListContextStrip_Opening(object sender, CancelEventArgs e)
        {
            var count = fileListView.SelectedIndices.Count;
            if (count == 0)
            {
                noFilesSelectedToolStripMenuItem.Visible = true;
                return;
            }
            IDPAbstractNode? node = fileListView.SelectedItems[0].Tag as IDPAbstractNode;
            if (node is null)
            {
                Logger.Error("Got null abstract node tag for a selected file list view item, select in hierachy disabled");
            }
            openInExplorerToolStripMenuItem.Visible = count == 1 && node is IDPFile file && file is { FileInfo.Exists: true };
            selectInHierachyToolStripMenuItem.Visible = count == 1 && node is not null && associatedTreeNodes.ContainsKey(node);
            copyFileToolStripMenuItem.Visible = true;
            noFilesSelectedToolStripMenuItem.Visible = false;
        }

        private void selectInFileListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            if (fileHierachyTree.SelectedNode!.Tag is IDPAbstractNode file && associatedListItems.TryGetValue(file, out var node))
                node.Selected = true;

            // Switch tab.
            tabControl1.SelectTab(fileListPage);
        }

        private void openInExplorerToolStripMenuItem_FileHierachy_Click(object sender, EventArgs e)
        {
            if (fileHierachyTree.SelectedNode is not null)
            {
                if (fileHierachyTree.SelectedNode.Tag is IDPFile file)
                {
                    if (file.FileInfo is not null) OpenFileInExplorer(file.FileInfo.Path);
                    else Logger.Error("Failed to open file in explorer due to null FileInfo");
                }
                else Logger.Warning("Open in explorer was called for a non-file object (or tag is malformed).");
            }
            else Logger.Warning("Open in explorer should not have been called when selected items is not 1.");
        }

        private void openInExplorerToolStripMenuItem_FileList_Click(object sender, EventArgs e)
        {
            if (fileListView.SelectedItems.Count is 1)
            {
                if (fileListView.SelectedItems[0].Tag is IDPFile file)
                {
                    if (file.FileInfo is not null) OpenFileInExplorer(file.FileInfo.Path);
                    else Logger.Error("Failed to open file in explorer due to null FileInfo");
                }
                else Logger.Warning("Open in explorer was called for a non-file object (or tag is malformed).");
            }
            else Logger.Warning("Open in explorer should not have been called when selected items is not 1.");
        }

        private void fileHierachyContextStrip_Opening(object sender, CancelEventArgs e)
        {
            if (fileHierachyTree.SelectedNode is null)
            {
                noNodesSelectedToolStripMenuItem.Visible = true;
                return;
            }
            IDPAbstractNode? node = fileHierachyTree.SelectedNode.Tag as IDPAbstractNode;
            if (node is null)
            {
                Logger.Error("Got null abstract node tag for a selected file list view item, select in hierachy disabled");
            }
            openInExplorerToolStripMenuItem.Visible = node is IDPFile file && file is { FileInfo.Exists: true };
            selectInHierachyToolStripMenuItem.Visible = node is not null && associatedListItems.ContainsKey(node);
            noNodesSelectedToolStripMenuItem.Visible = false;
        }

        private void copyToolStripMenuItem_Errors_Click(object sender, EventArgs e)
        {
            if (errorDataGridView.SelectedCells.Count is not 1) return;
            try
            {
                if (errorDataGridView.SelectedCells[0].Value is string text)
                    CopyToClipboard(text);
                else
                {
                    Logger.Warning("The selected cell for the error data grid was not a string value type, attempting to force to string");
                    if (errorDataGridView.SelectedCells[0].Value!.ToString() is string s) CopyToClipboard(s);
                    else throw new ArgumentException("Selected cell could not be converted to a string");
                }
            } catch (Exception ex)
            {
                MessageBox.Show("Failed to copy cell data to clipboard", "Failed to copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error(ex, "Failed to copy value of selected cell in error data grid when forced");
            }
            
        }

        private void errorsMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            if (errorDataGridView.SelectedCells.Count is 0)
            {
                e.Cancel = true;
                return;
            }
            copyErrorToolStripMenuItem.Enabled = errorDataGridView.SelectedCells.Count is 1;
        }

        private void queueContextStrip_Opening(object sender, CancelEventArgs e)
        {
            QueueController.OnContextMenu(sender, e);
        }

        private void skipArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QueueController.OnSkipArchive();
        }

        private void cancelCurrentArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QueueController.OnCancelCurrentArchive();
        }

        private void cancelExtractJobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QueueController.OnCancelJob();
        }

        private void cancelCurrentExtractJobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QueueController.OnCancelCurrentJob();
        }

        private void viewFileListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QueueController.OnViewFileList();
        }

        private void viewFileHierachyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QueueController.OnViewHierachy();
        }

        private void viewErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            QueueController.OnViewErrors();
        }

        #endregion

        private void copyToolStripMenuItem_FileList_Click(object sender, EventArgs e)
        {
            if (fileListView.SelectedItems.Count is 0) return;
            try
            {
                CopyToClipboard([..fileListView.SelectedItems.OfType<ListViewItem>().Select(x => x.Text)]);
            } catch (Exception ex)
            {
                MessageBox.Show("Failed to copy cell data to clipboard", "Failed to copy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error(ex, "Failed to copy value of selected cell in error data grid when forced");
            }
        }

        /// <summary>
        /// Copies items to clipboard with a new line seperator for each item.
        /// </summary>
        /// <param name="items">The items to add to clipboard seperated by a new line.</param>
        /// <exception cref="Exception">Clipboard failed to set items</exception>
        private void CopyToClipboard(params ReadOnlySpan<string> items)
        {
            if (items.Length is 0) return;
            Clipboard.SetText(string.Join('\n', items!));
        }
    }

}


