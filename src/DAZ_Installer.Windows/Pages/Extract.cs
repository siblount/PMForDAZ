// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using DAZ_Installer.Windows.DP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.Pages
{

    public partial class Extract : UserControl
    {
        public static Extract ExtractPage = null!;
        internal static Dictionary<IDPAbstractNode, ListViewItem> associatedListItems = new(256);
        internal static Dictionary<IDPAbstractNode, TreeNode> associatedTreeNodes = new(256);
        internal static Dictionary<string, ListViewItem> associatedQueueItems = new(64, PathComparer.Instance);
        internal static List<DPExtractJob> extractJobs = new(4);

        public Extract()
        {
            InitializeComponent();
            ExtractPage = this;
            tabControl1.TabPages.Remove(fileListPage);
            tabControl1.TabPages.Remove(fileHierachyPage);
            var errorImage = SystemIcons.Error.ToBitmap(); // Do not dispose Handle
            var warningImage = SystemIcons.Warning.ToBitmap(); // Do not dispose Handle
            statusIcons.Images.Add("error", errorImage);
            statusIcons.Images.Add("warning", warningImage);
        }

        internal void AddToQueue(DPExtractJob job)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => AddToQueue(job));
                return;
            }
            extractJobs.Add(job);
            var letter = (char)('A' - 1 + extractJobs.Count);
            queueListView.BeginUpdate();
            var groupKey = job.GetHashCode().ToString();
            queueListView.Groups.Add(groupKey, $"Extract Job {letter}");
            foreach (string file in job.InitialFilesToProcess)
            {
                ListViewItem item = queueListView.Items.Add(file);
                item.Tag = file;
                item.Group = queueListView.Groups[groupKey];
            }
            queueListView.EndUpdate();
        }

        internal void OnExtractJobStatusUpdate(DPExtractJob caller, DPArchiveInfo info)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => OnExtractJobStatusUpdate(caller, info));
                return;
            }
            queueListView.BeginUpdate();
            if (!associatedQueueItems.TryGetValue(info.FilePath, out ListViewItem? item))
                return;
            item.ForeColor = DetermineColorForArchive(info);
            item.StateImageIndex = DetermineImageIndexForArchive(info);
            DetermineErrorMessage(info, item);
        }


        private static void DetermineErrorMessage(DPArchiveInfo info, ListViewItem item)
        {
            var subItem = item.SubItems.Count == 0 ? new ListViewItem.ListViewSubItem(item, "") : item.SubItems[0];
            switch (info.Status)
            {
                case DPArchiveStatus.CompletedWithIssues:
                    subItem.Text = info.Errors.Count == 0 ? "Completed but some files were not extracted" : "Completed with errors";
                    break;
                case DPArchiveStatus.Failed:
                    if (info.Errors.Count == 0)
                        subItem.Text = "Failed to extract";
                    else if (info.Errors.Count >= 2)
                        subItem.Text = "Multiple errors occurred";
                    else if (info.Errors[0].Exception is not null || info.Errors[0].Explanation is not null)
                        subItem.Text = info.Errors[0].Exception?.Message ?? info.Errors[0].Explanation;
                    else
                        subItem.Text = "Failed to extract due to an unknown error";
                    break;
                case DPArchiveStatus.Cancelled:
                    if (info.Errors.Count != 0)
                        subItem.Text = "Cancelled with errors";
                    break;
            }
        }

        private static Color DetermineColorForArchive(DPArchiveInfo info)
        {
            if (info.Status is DPArchiveStatus.Cancelled) return Color.Gray;
            if (info.Status is DPArchiveStatus.Failed) return Color.Red;
            if (info.Status is DPArchiveStatus.Completed) return Color.Green;
            if (info.Status is DPArchiveStatus.CompletedWithIssues) return Color.Orange;
            if (info.Archive is { IsInnerArchive: true }) return Color.LightBlue;
            return Color.Black;
        }

        private int DetermineImageIndexForArchive(DPArchiveInfo info)
        {
            return info.Status switch
            {
                DPArchiveStatus.Failed => statusIcons.Images.IndexOfKey("error"),
                DPArchiveStatus.CompletedWithIssues => statusIcons.Images.IndexOfKey("warning"),
                _ => 0
            };
        }

        /// <summary>
        /// Adds all the contents found in <paramref name="archive"/> to the list view.
        /// Assure that this function is called from the UI thread with either <see cref="Control.Invoke(Delegate)"/> or <see cref="Control.BeginInvoke(Delegate)"/>.
        /// </summary>
        internal void AddToList(IDPArchive archive)
        {
            fileListView.BeginUpdate();
            foreach (IDPFile content in archive.Contents.Values)
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
        internal void AddToHierachy(IDPArchive workingArchive)
        {
            fileHierachyTree.BeginUpdate();

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
            progressCombo.EndProgress();
            fileListView.Items.Clear();
            fileHierachyTree.Nodes.Clear();
            associatedListItems.Clear();
            associatedTreeNodes.Clear();
        }

        #region Context Strip Events
        private void selectInHierachyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            var file = fileListView.SelectedItems[0].Tag as IDPAbstractNode;

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
                associatedTreeNodes.TryGetValue(fileListView.SelectedItems[0].Tag as IDPAbstractNode, out TreeNode _);
            noFilesSelectedToolStripMenuItem.Visible = !filesSelected;
        }

        public void OpenFileInExplorer(string path) => Process.Start(@"explorer.exe", $"/select, \"{path}\"");

        private void selectInFileListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            var file = fileHierachyTree.SelectedNode.Tag as IDPAbstractNode;

            if (file != null && associatedListItems.TryGetValue(file, out ListViewItem node))
                node.Selected = true;

            // Switch tab.
            tabControl1.SelectTab(fileListPage);
        }
        #endregion
    }

}


