﻿// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using DAZ_Installer.Core;
using System.Linq;

namespace DAZ_Installer.WinApp.Pages
{

    public partial class Extract : UserControl
    {
        public static Extract ExtractPage;
        internal static Dictionary<DPAbstractFile, ListViewItem> associatedListItems = new(2048);
        internal static Dictionary<DPAbstractFile, TreeNode> associatedTreeNodes = new(2048);

        public void ResetMainTable()
        {
            mainTableLayoutPanel.SuspendLayout();
            try
            {
                if (mainTableLayoutPanel.Controls.Count != 0)
                {
                    var arr = RecursivelyGetControls(mainTableLayoutPanel);
                    foreach (var control in arr)
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
        public void UpdateMainTableRowSizing()
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
            tabControl1.TabPages.Remove(queuePage);
            queuePage.Dispose();
        }

        internal void AddToList(DPAbstractArchive archive)
        {
            fileListView.BeginUpdate();
            foreach (var content in archive.Contents)
            {
                var item = fileListView.Items.Add($"{archive.FileName}\\{content.Path}");
                item.Tag = content;
                associatedListItems[content] = item;
            }
            fileListView.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            fileListView.EndUpdate();
        }

        private void ProcessChildNodes(DPFolder folder, TreeNode parentNode)
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
                    node.Tag = file;
                    associatedTreeNodes[file] = node;
                    AddIcon(node, file.Ext);
                }
                else
                {
                    var node = folder1.Nodes.Add(fileName);
                    node.Tag = file;
                    associatedTreeNodes[file] = node;
                    AddIcon(node, file.Ext);
                }
            }
            foreach (var subfolder in folder.subfolders)
                ProcessChildNodes(subfolder, folder1);
        }

        internal void AddToHierachy(DPAbstractArchive workingArchive)
        {
            if (InvokeRequired)
            {
                Invoke(AddToHierachy, workingArchive);
                return;
            }

            fileHierachyTree.BeginUpdate();

            // Add root node for DPArchive.
            var fileName = workingArchive.HierachyName;
            TreeNode rootNode = null;
            rootNode = fileHierachyTree.Nodes.Add(fileName);
            rootNode.Tag = workingArchive;
            associatedTreeNodes[workingArchive] = rootNode;
            AddIcon(rootNode, workingArchive.Ext);


            // Add any files that aren't in any folder.
            foreach (var file in workingArchive.RootContents)
            {
                fileName = Path.GetFileName(file.Path);
                var node = rootNode.Nodes.Add(fileName);
                node.Tag = file;
                associatedTreeNodes[file] = node;
                AddIcon(node, file.Ext);
            }

            // Recursively add files & folder within each folder.
            foreach (var folder in workingArchive.RootFolders)
                ProcessChildNodes(folder, rootNode);

            fileHierachyTree.EndUpdate();
        }

        private void AddIcon(TreeNode node, string ext)
        {
            if (InvokeRequired)
            {
                Invoke(AddIcon, node, ext);
                return;
            }
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

        public static Control[] RecursivelyGetControls(Control obj)
        {
            if (obj.Controls.Count == 0) return null;
            else
            {
                var workingArr = new List<Control>(obj.Controls.Count);
                foreach (Control control in obj.Controls)
                {
                    var result = RecursivelyGetControls(control);
                    if (result != null)
                    {
                        foreach (Control childControl in result)
                            workingArr.Add(childControl);
                    }
                    workingArr.Add(control);
                }
                var controlArr = workingArr.ToArray();
                return controlArr;
            }

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
            UpdateMainTableRowSizing();
        }
        
        #endregion

        #region Context Strip Events
        private void selectInHierachyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            var file = fileListView.SelectedItems[0].Tag as DPAbstractFile;
            
            if (file != null && associatedTreeNodes.TryGetValue(file, out var node))
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
                associatedTreeNodes.TryGetValue(fileListView.SelectedItems[0].Tag as DPAbstractFile, out var _);
            noFilesSelectedToolStripMenuItem.Visible = !filesSelected;
        }

        public void OpenFileInExplorer(string path)
        {
            Process.Start(@"explorer.exe", $"/select, \"{path}\"");
        }
        #endregion

        private void selectInFileListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            var file = fileHierachyTree.SelectedNode.Tag as DPAbstractFile;

            if (file != null && associatedListItems.TryGetValue(file, out var node))
                node.Selected = true;

            // Switch tab.
            tabControl1.SelectTab(fileListPage);
        }
    }

}


