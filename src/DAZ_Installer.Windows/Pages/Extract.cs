// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using DAZ_Installer.Windows.DP;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.Pages
{

    public partial class Extract : UserControl
    {
        public static Extract ExtractPage;
        internal static Dictionary<DPAbstractNode, ListViewItem> associatedListItems = new(2048);
        internal static Dictionary<DPAbstractNode, TreeNode> associatedTreeNodes = new(2048);

        /// <summary>
        /// Completely resets the main table layout panel by removing (and disposing) all controls and resetting the row/column count. 
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        public void ResetMainTable()
        {
            mainTableLayoutPanel.SuspendLayout();
            try
            {
                if (mainTableLayoutPanel.Controls.Count != 0)
                {
                    foreach (Control control in RecursivelyGetControls(mainTableLayoutPanel))
                        control.Dispose();
                }
            }
            catch { }
            mainTableLayoutPanel.Controls.Clear();
            mainTableLayoutPanel.RowStyles.Clear();
            mainTableLayoutPanel.ColumnCount = 1;
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowCount = 1;
            UpdateMainTableRowSizing();
            mainTableLayoutPanel.ResumeLayout();
        }

        /// <summary>
        /// Updates the main table row sizing to be equal to the amount of controls in the table.
        /// Set <paramref name="suspend"/> to true to suspend and resume the layout after updating. Default is false.
        /// </summary>
        /// <param name="suspend">Whether to suspend and resume the layout after updating.</param>
        public void UpdateMainTableRowSizing(bool suspend = false)
        {
            if (suspend) mainTableLayoutPanel.SuspendLayout();
            var percentageMultiplied = 1f / mainTableLayoutPanel.Controls.Count * 100f;
            for (var i = 0; i < mainTableLayoutPanel.RowStyles.Count; i++)
            {
                mainTableLayoutPanel.RowStyles[i] = new RowStyle(SizeType.Percent, percentageMultiplied);
            }

            if (!suspend) return;
            mainTableLayoutPanel.ResumeLayout();
            mainTableLayoutPanel.Update();
        }

        public Extract()
        {
            InitializeComponent();
            ExtractPage = this;
            tabControl1.TabPages.Remove(queuePage);
            queuePage.Dispose();
        }

        /// <summary>
        /// Adds all the contents found in <paramref name="archive"/> to the list view.
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        internal void AddToList(DPArchive archive)
        {
            fileListView.BeginUpdate();
            foreach (DPFile content in archive.Contents.Values)
            {
                ListViewItem item = fileListView.Items.Add($"{archive.FileName}\\{content.Path}");
                item.Tag = content;
                associatedListItems[content] = item;
            }
            fileListView.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            fileListView.EndUpdate();
        }

        /// <summary>
        /// Process the child nodes of <paramref name="folder"/> and add them to <paramref name="parentNode"/>.
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="parentNode"></param>
        private void ProcessChildNodes(DPFolder folder, TreeNode parentNode)
        {
            var fileName = Path.GetFileName(folder.Path);
            // We don't need associations for folders.
            var folder1 = parentNode.Nodes.Add(fileName);
            AddIcon(folder1, null);

            // Add the DPFiles.
            foreach (DPFile file in folder.Contents)
            {
                fileName = Path.GetFileName(file.Path);
                // TO DO: Add condition if file is a DPArchive & extract == true
                TreeNode node = folder1.Nodes.Add(fileName);
                node.Tag = file;
                associatedTreeNodes[file] = node;
                AddIcon(node, file.Ext);
            }
            foreach (DPFolder subfolder in folder.subfolders)
                ProcessChildNodes(subfolder, folder1);
        }

        /// <summary>
        /// Adds the contents of <paramref name="workingArchive"/> to the file hierachy tree.
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        /// <param name="workingArchive">The archive to add to the hierachy</param>
        internal void AddToHierachy(DPArchive workingArchive)
        {
            fileHierachyTree.BeginUpdate();

            // Add root node for DPArchive.
            var fileName = workingArchive.FileName;
            TreeNode rootNode = fileHierachyTree.Nodes.Add(fileName);
            rootNode.Tag = workingArchive;
            associatedTreeNodes[workingArchive] = rootNode;
            AddIcon(rootNode, workingArchive.Ext);

            // Add any files that aren't in any folder.
            foreach (DPFile file in workingArchive.RootContents)
            {
                fileName = Path.GetFileName(file.Path);
                TreeNode node = rootNode.Nodes.Add(fileName);
                node.Tag = file;
                associatedTreeNodes[file] = node;
                AddIcon(node, file.Ext);
            }

            // Recursively add files & folder within each folder.
            foreach (DPFolder folder in workingArchive.RootFolders)
                ProcessChildNodes(folder, rootNode);

            fileHierachyTree.EndUpdate();
        }

        /// <summary>
        /// Assigns an icon to the <paramref name="node"/> based on the <paramref name="ext"/> of the file.
        /// This only assigns icons for archives and folders. <br/>
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        /// <param name="node">The node to set the icon to</param>
        /// <param name="ext">The extension used to determine the icon to set (7z, zip, rar, null, or "").</param>
        private void AddIcon(TreeNode node, string? ext)
        {
            if (string.IsNullOrEmpty(ext))
                node.StateImageIndex = 0;
            else if (ext.Contains("zip") || ext.Contains("7z"))
                node.StateImageIndex = 2;
            else if (ext.Contains("rar"))
                node.StateImageIndex = 1;
        }

        public void ResetExtractPage()
        {
            // Later show nothing to extract panel.
            ResetMainTable();
            fileListView.Items.Clear();
            fileHierachyTree.Nodes.Clear();
            associatedListItems.Clear();
            associatedTreeNodes.Clear();
        }

        /// <summary>
        /// Recursively gets the controls of the <paramref name="obj"/> and returns them as an array.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable<Control> RecursivelyGetControls(Control obj)
        {
            Log.Information($"RecursivelyGetControls: {obj.Controls.Count}");
            if (obj.Controls.Count == 0) return Enumerable.Empty<Control>();
            var list = new List<Control>(obj.Controls.Count);
            var enumerator = list.AsEnumerable();
            foreach (Control control in obj.Controls)
            {
                enumerator.Concat(RecursivelyGetControls(control));
                list.Add(control);
            }
            
            return enumerator;
        }

        private void mainProcLbl_Click(object sender, EventArgs e)
        {

        }

        #region Handle DPPrecssor Events

        /// <summary>
        /// Deletes the progression combo from the main table layout panel.
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        /// <param name="combo">The DPProgressCombo to remove.</param>
        internal void DeleteProgressionCombo(DPProgressCombo combo)
        {
            mainTableLayoutPanel.SuspendLayout();
            mainTableLayoutPanel.Controls.Remove(combo.Panel);
            mainTableLayoutPanel.RowCount = Math.Max(1, mainTableLayoutPanel.Controls.Count);
            mainTableLayoutPanel.RowStyles.Clear();
            for (var i = 0; i < mainTableLayoutPanel.RowCount; i++)
                mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.ResumeLayout();
            UpdateMainTableRowSizing(true);
        }

        /// <summary>
        /// Creates a progress bar and adds it to the table.
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        /// <param name="combo">The DPProgressCombo to add to the main table layout panel.</param>
        // This function is called once on the UI thread.
        internal void AddNewProgressCombo(DPProgressCombo combo)
        {
            mainTableLayoutPanel.SuspendLayout();
            if (mainTableLayoutPanel.Controls.Count != 0)
            {
                mainTableLayoutPanel.RowCount += 1;
                mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            mainTableLayoutPanel.Controls.Add(combo.Panel);
            UpdateMainTableRowSizing();
            mainTableLayoutPanel.ResumeLayout(true);
        }

        #endregion

        #region Context Strip Events
        private void selectInHierachyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            var file = fileListView.SelectedItems[0].Tag as DPAbstractNode;

            if (file != null && associatedTreeNodes.TryGetValue(file, out TreeNode node))
                fileHierachyTree.SelectedNode = node;
            // Switch tab.
            tabControl1.SelectTab(fileHierachyPage);
        }

        private void fileListContextStrip_Opening(object sender, CancelEventArgs e)
        {
            var filesSelected = fileListView.SelectedItems.Count != 0;
            inspectFileListMenuItem.Visible = false && filesSelected;
            openInExplorerToolStripMenuItem.Visible = filesSelected;
            selectInHierachyToolStripMenuItem.Visible = filesSelected &&
                associatedTreeNodes.TryGetValue(fileListView.SelectedItems[0].Tag as DPAbstractNode, out TreeNode _);
            noFilesSelectedToolStripMenuItem.Visible = !filesSelected;
        }

        public void OpenFileInExplorer(string path) => Process.Start(@"explorer.exe", $"/select, \"{path}\"");
        #endregion

        private void selectInFileListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            var file = fileHierachyTree.SelectedNode.Tag as DPAbstractNode;

            if (file != null && associatedListItems.TryGetValue(file, out ListViewItem node))
                node.Selected = true;

            // Switch tab.
            tabControl1.SelectTab(fileListPage);
        }
    }

}


