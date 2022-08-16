// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using DAZ_Installer.DP;

namespace DAZ_Installer
{

    public partial class Extract : UserControl
    {
        /// <summary> 
        /// Returns the integer of the first available slot in List<object>. Returns -1 if not available. 
        /// </summary>
        public static Extract ExtractPage;
        internal static Dictionary<ListViewItem, DPAbstractFile> associatedListItems = new(4096);
        internal static Dictionary<TreeNode, DPAbstractFile> associatedTreeNodes = new(4096);

        public void resetMainTable()
        {
            mainTableLayoutPanel.SuspendLayout();
            try
            {
                if (mainTableLayoutPanel.Controls.Count != 0)
                {
                    var arr = DPCommon.RecursivelyGetControls(mainTableLayoutPanel);
                    foreach (var control in arr)
                    {
                        control.Dispose();
                    }
                }
            }
            catch { }
            mainTableLayoutPanel.Controls.Clear();
            mainTableLayoutPanel.RowStyles.Clear();
            mainTableLayoutPanel.ColumnCount = 1;
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowCount = 1;
            updateMainTableRowSizing();
            mainTableLayoutPanel.ResumeLayout();
        }
        public void updateMainTableRowSizing()
        {
            // TO DO : Invoke.
            mainTableLayoutPanel.SuspendLayout();
            float percentageMultiplied = 1f / mainTableLayoutPanel.Controls.Count * 100f;
            for (var i = 0; i < mainTableLayoutPanel.RowStyles.Count; i++)
            {
                mainTableLayoutPanel.RowStyles[i] = new RowStyle(SizeType.Percent, percentageMultiplied);
            }
            
            mainTableLayoutPanel.ResumeLayout();
            mainTableLayoutPanel.Update();
        }

        public DialogResult DoPromptMessage(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.YesNo)
        {
            
            return MessageBox.Show(message, title, buttons, MessageBoxIcon.Hand);
        }

        public Extract()
        {
            InitializeComponent();
            ExtractPage = this;
        }

        internal void AddToList(DPAbstractArchive archive)
        {
            fileListView.BeginUpdate();
            foreach (var content in archive.Contents)
            {
                var item = fileListView.Items.Add($"{archive.FileName}\\{content.Path}");
                content.AssociatedListItem = item;
                associatedListItems.Add(item, content);
            }
            fileListView.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            fileListView.EndUpdate();
        }

        private void ProcessChildNodes(DPFolder folder, ref TreeNode parentNode)
        {
            var fileName = Path.GetFileName(folder.Path);
            TreeNode folder1 = null;
            // We don't need associations for folders.
            if (InvokeRequired)
            {
                folder1 = (TreeNode) Invoke(new Func<string, TreeNode>(parentNode.Nodes.Add), fileName);
                AddIcon(folder1, null);
            } else
            {
                folder1 = parentNode.Nodes.Add(fileName);
                AddIcon(folder1, null);
            }
            // Add the DPFiles.
            foreach (var file in folder.GetFiles())
            {
                fileName = Path.GetFileName(file.Path);
                // TO DO: Add condition if file is a DPArchive & extract == true
                if (InvokeRequired)
                {
                    var node = (TreeNode) Invoke(new Func<string, TreeNode>(folder1.Nodes.Add), fileName);
                    file.AssociatedTreeNode = node;
                    AddIcon(node, file.Ext);
                }
                else
                {
                    var node = folder1.Nodes.Add(fileName);
                    file.AssociatedTreeNode = node;
                    AddIcon(node, file.Ext);
                }
            }
            foreach (var subfolder in folder.subfolders)
            {
                ProcessChildNodes(subfolder, ref folder1);
            }
        }

        internal void AddToHierachy(DPAbstractArchive workingArchive)
        {
            fileHierachyTree.BeginUpdate();
            // Add root node for DPArchive.
            var fileName = workingArchive.HierachyName;
            TreeNode rootNode = null;
            if (InvokeRequired)
            {
                var func = new Func<string, TreeNode>(fileHierachyTree.Nodes.Add);
                rootNode = (TreeNode) Invoke(func,fileName);
                workingArchive.AssociatedTreeNode = rootNode;
                AddIcon(rootNode, workingArchive.Ext);

            } else
            {
                rootNode = fileHierachyTree.Nodes.Add(fileName);
                workingArchive.AssociatedTreeNode = rootNode;
                AddIcon(rootNode, workingArchive.Ext);
            }


            // Add any files that aren't in any folder.
            foreach (var file in workingArchive.RootContents)
            {
                fileName = Path.GetFileName(file.Path);
                if (InvokeRequired)
                {
                    var node = (TreeNode) Invoke(new Func<string, TreeNode>(rootNode.Nodes.Add), fileName);
                    file.AssociatedTreeNode = node;
                    AddIcon(node, file.Ext);
                } else
                {
                    var node = rootNode.Nodes.Add(fileName);
                    file.AssociatedTreeNode = node;
                    AddIcon(node, file.Ext);
                }
            }

            // Recursively add files & folder within each folder.
            foreach (var folder in workingArchive.RootFolders)
            {
                ProcessChildNodes(folder, ref rootNode);
            }
            fileHierachyTree.ExpandAll();
            fileHierachyTree.EndUpdate();
        }

        // Object to satisfy Invoke.
        private void AddIcon(TreeNode node, string ext)
        {
            if (InvokeRequired)
            {
                Invoke(AddIcon, node, ext);
            }
            if (string.IsNullOrEmpty(ext))
                node.StateImageIndex = 0;
            else if (ext.Contains("zip") || ext.Contains("7z"))
                node.StateImageIndex = 2;
            else if (ext.Contains("rar"))
               node.StateImageIndex = 1;
        }
        private void extractControl_Load(object sender, EventArgs e)
        {
            //var DSXParser = new DSXParser(@"D:\3D\DAZ3D shit\DAZ IM Manager Downloads\Manifest.dsx");
            //DPSettings.Initalize();
        }

        

        public void ResetExtractPage()
        {
            // Later show nothing to extract panel.
            resetMainTable();
            fileListView.Items.Clear();
            fileHierachyTree.Nodes.Clear();
            associatedListItems.Clear();
            associatedTreeNodes.Clear();
        }
        /// <summary>
        /// Creates a progress bar and adds it to the table. [0] - TableLayout, [1] - label, [2] - ProgressBar
        /// </summary>
        /// <returns>An array of controls</returns>
        /// 
        internal void AddNewProgressCombo(DPProgressCombo combo) {
            mainTableLayoutPanel.SuspendLayout();
            if (mainTableLayoutPanel.Controls.Count != 0)
            {
                mainTableLayoutPanel.RowCount += 1;
                mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            mainTableLayoutPanel.Controls.Add(combo.Panel);
            updateMainTableRowSizing();
            mainTableLayoutPanel.ResumeLayout(true);
        }

        private void mainProcLbl_Click(object sender, EventArgs e)
        {
            
        }

        #region Handle DPPrecssor Events
        internal void DeleteProgressionCombo(DPProgressCombo combo)
        {
            if (InvokeRequired)
            {
                Invoke(DeleteProgressionCombo, combo);
                return;
            }

            mainTableLayoutPanel.SuspendLayout();
            mainTableLayoutPanel.Controls.Remove(combo.Panel);
            mainTableLayoutPanel.RowCount = Math.Max(1, mainTableLayoutPanel.Controls.Count);
            mainTableLayoutPanel.RowStyles.Clear();
            for (var i = 0; i < mainTableLayoutPanel.RowCount; i++) 
                mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.ResumeLayout();
            updateMainTableRowSizing();
        }
        
        #endregion

        #region Context Strip Events
        private void selectInHierachyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            associatedListItems.TryGetValue(fileListView.SelectedItems[0], out DPAbstractFile file);
            if (file != null)
            {
                fileHierachyTree.SelectedNode = file.AssociatedTreeNode;
            }
            // Switch tab.
            tabControl1.SelectTab(fileHierachyPage);

        }

        private void fileListContextStrip_Opening(object sender, CancelEventArgs e)
        {
            var filesSelected = fileListView.SelectedItems.Count != 0;
            inspectFileListMenuItem.Visible = filesSelected;
            openInExplorerToolStripMenuItem.Visible = filesSelected;
            selectInHierachyToolStripMenuItem.Visible = filesSelected;
            noFilesSelectedToolStripMenuItem.Visible = !filesSelected;
        }

        public void OpenFileInExplorer(string path)
        {
            Process.Start(@"explorer.exe", $"/select, \"{path}\"");
        }
        #endregion

        private void queueList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }

}


