// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using DAZ_Installer.Database;
using DAZ_Installer.Windows.Pages;
using DAZ_Installer.Windows.DP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DAZ_Installer.IO;
using Serilog;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Windows.Forms
{
    public partial class ProductRecordForm : Form
    {
        private ILogger logger = Log.Logger.ForContext<ProductRecordForm>();
        private DPProductRecord record;
        private DPProductRecordLite liteRecord;
        private uint[] maxFontWidthPerListView = new uint[5];
        private HashSet<string> tagsSet = new();
        public ProductRecordForm()
        {
            InitializeComponent();
            fileTreeView.StateImageList = Extract.ExtractPage.archiveFolderIcons;
            if (DPGlobal.isWindows11)
                applyChangesBtn.Size = new Size(applyChangesBtn.Size.Width, applyChangesBtn.Size.Height + 2);
        }

        public ProductRecordForm(DPProductRecordLite productRecord) : this()
        {
            InitializeProductRecordInfo(productRecord);
            Program.Database.GetFullProductRecord(productRecord.ID, InitializeRecord).ConfigureAwait(false);
        }

        public void InitializeProductRecordInfo(DPProductRecordLite record)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => InitializeProductRecordInfo(record));
                return;
            }
            liteRecord = record;
            productNameTxtBox.Text = record.Name;
            tagsView.BeginUpdate();
            ListForEach(record.Tags, tag => tagsView.Items.Add(tag));
            tagsSet = new HashSet<string>(record.Tags);
            tagsView.EndUpdate();
            CalculateMaxWidthPerListView();
            UpdateColumnWidths();
            Program.Database.ProductRecordModified += OnProductRecordModified;

        }

        private void OnProductRecordModified(DPProductRecord newProductRecord, long id)
        {
            if (id == record.ID)
                MessageBox.Show("Product record successfully updated!", "Product record updated successfully.");
            record = newProductRecord;
        }

        public void InitializeRecord(DPProductRecord? fullRecord)
        {
            if (fullRecord is null)
            {
                MessageBox.Show("Failed to retrieve full product record.", "Failed to retrieve product record", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (fullRecord.ID != liteRecord.ID) return;
            if (InvokeRequired)
            {
                BeginInvoke(() => InitializeRecord(fullRecord));
                return;
            }
            record = fullRecord;
            SuspendLayout();
            normalizeRecord();

            authorLbl.Text += string.IsNullOrEmpty(record.AuthorsString) ? "Not detected" : record.AuthorsString;
            if (record.ThumbnailPath != null && File.Exists(record.ThumbnailPath))
            {
                thumbnailBox.Image = Library.self.AddReferenceImage(record.ThumbnailPath);
                thumbnailBox.ImageLocation = record.ThumbnailPath;
            }
            dateExtractedLbl.Text += record.Date.ToLocalTime().ToString();

            ListForEach(record.Files, file => filesExtractedList.Items.Add(file));
            BuildFileHierachy();
            ResumeLayout();
            destinationPathLbl.Text += fullRecord.Destination;
            CalculateMaxWidthPerListView();
            UpdateColumnWidths();
        }

        private void ListForEach<T>(IReadOnlyList<T> list, Action<T> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }

        private void browseImageBtn_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Supported Images (png, jpeg, bmp)|*.png;*.jpg;*.jpeg;*.bmp";
            dlg.Title = "Select thumbnail image";
            if (dlg.ShowDialog() != DialogResult.OK) return;
            var location = dlg.FileName;

            if (File.Exists(location))
            {
                try
                {
                    var img = Image.FromFile(location);
                    thumbnailBox.Hide();
                    thumbnailBox.ImageLocation = location;
                    thumbnailBox.Image = img;
                    thumbnailBox.Show();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to load image from file to update thumbnail image");
                    MessageBox.Show($"Unable to update thumbnail image. REASON: \n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show($"Unable to update image due to it not being found (or able to be accessed).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void UpdateColumnWidths()
        {
            tagsView.Columns[0].Width = (int)maxFontWidthPerListView[0];
            filesExtractedList.Columns[0].Width = (int)maxFontWidthPerListView[2];
        }

        private void CalculateMaxWidthPerListView()
        {
            maxFontWidthPerListView[0] = GetMaxWidth(tagsView.Items);
            maxFontWidthPerListView[2] = GetMaxWidth(filesExtractedList.Items);
        }

        private uint GetMaxWidth(ListView.ListViewItemCollection collection)
        {
            uint maxWidth = 0;
            var itemsText = new string[collection.Count];
            for (var i = 0; i < collection.Count; i++) itemsText[i] = collection[i].Text;

            Array.Sort(itemsText, (b, a) => a.Length.CompareTo(b.Length));
            for (var i = 0; i < itemsText.Length && i < 3; i++)
            {
                var width = TextRenderer.MeasureText(itemsText[i], collection[0].Font).Width;
                if (width > maxWidth) maxWidth = (uint)width;
            }
            return maxWidth + 20;
        }

        private void BuildFileHierachy()
        {
            var folderMap = SetupFolderRoots(out var rootNodes);

            foreach (var folder in folderMap.Values)
            {
                folder.StateImageIndex = 0;
            }

            var treeNodes = new HashSet<TreeNode>(rootNodes.Count * 2);
            treeNodes.UnionWith(rootNodes);
            // Initalize the map by just connecting a folder path to a tree node.
            foreach (var file in record.Files)
            {
                var dirName = Path.GetDirectoryName(file + ".e")!;

                var fileNode = new TreeNode(Path.GetFileName(file));
                var ext = Path.GetExtension(file);
                // If the file has a folder node, then add it to that node.
                if (folderMap.ContainsKey(dirName))
                    folderMap[dirName].Nodes.Add(fileNode);
                else // otherwise, it means the file is at root.
                    treeNodes.Add(fileNode);

                if (string.IsNullOrEmpty(ext))
                    fileNode.StateImageIndex = 0;
                else if (ext.EndsWith("zip") || ext.EndsWith("7z"))
                    fileNode.StateImageIndex = 2;
                else if (ext.EndsWith("rar"))
                    fileNode.StateImageIndex = 1;
            }

            var treeNodesArr = new TreeNode[treeNodes.Count];
            treeNodes.CopyTo(treeNodesArr);
            fileTreeView.Nodes.AddRange(treeNodesArr);
        }

        private Dictionary<string, TreeNode> SetupFolderRoots(out HashSet<TreeNode> rootNodes)
        {
            Dictionary<string, TreeNode> nodes = new();
            rootNodes = new(5);
            foreach (var file in record.Files)
            {
                var dirName = Path.GetDirectoryName(file + ".e")!;
                if (string.IsNullOrEmpty(dirName) || nodes.ContainsKey(dirName)) continue;
                CreateTreeNodeForFolder(dirName, nodes, out var rootNode);
                if (rootNode is not null)
                    rootNodes.Add(rootNode);
            }
            return nodes;
        }

        /// <summary>
        /// Creates a tree node and the appropriate parent tree nodes for the folder.
        /// </summary>
        /// <param name="name">The name for the immediate TreeNode.</param>
        /// <param name="folderMap">The map of folder name to folder treeNode, will be used to add tree nodes, if needed.</param>
        /// <param name="rootTreeNode">The root treenode for the folder.</param>
        /// <returns>The TreeNode of the folder</returns>
        private TreeNode CreateTreeNodeForFolder(string name, Dictionary<string, TreeNode> folderMap, out TreeNode? rootTreeNode)
        {
            var seperator = PathHelper.GetSeperator(name);
            var folders = name.Split(seperator);
            StringBuilder sb = new(name.Length);
            TreeNode? lastNode = null;
            rootTreeNode = null;
            for (int i = 0; i < folders.Length; i++)
            {
                if (i == 0) sb.Append(folders[i]);
                else sb.Append(seperator).Append(folders[i]);
                var path = sb.ToString();
                if (folderMap.ContainsKey(path)) continue;
                var node = new TreeNode(Path.GetFileName(path));
                folderMap.Add(path, node);
                lastNode?.Nodes.Add(node);
                if (i == 0) rootTreeNode = node;
                lastNode = node;
            }
            return lastNode!;
        }

        private void normalizeRecord()
        {
            var normalizedFiles = new List<string>(record.Files.Count);
            foreach (var file in record.Files)
                normalizedFiles.Add(PathHelper.NormalizePath(file));

            record = record with { Files = normalizedFiles };
        }

        private void deleteRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to remove the record for {record.Name}? " +
                "This wont remove the files on disk. Additionally, the record cannot be restored.", "Remove product record confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            Task.Run(() => handleDeletion());
        }

        private void DisableRecordForm(long id)
        {
            if (!Visible) return;
            if (InvokeRequired)
            {
                BeginInvoke(() => DisableRecordForm(id));
                return;
            }
            applyChangesBtn.Enabled = false;
            toolStrip1.Enabled = false;
        }

        private void deleteProductToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show($"Are you sure you want to remove the record & product files for {record.Name}? " +
                "THIS WILL PERMANENTLY REMOVE ASSOCIATED FILES ON DISK!", "Remove product confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            if (Directory.Exists(record.Destination))
            {
                Task.Run(() => handleDeletion(true));
                return;
            }
            var r = MessageBox.Show("The expected paths where the files are supposed to be do no exist or do not have access to it. " +
                "Do you wish to continue? If so, select the destination path for this product in the following prompt.", 
                "Destination does not exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.No) return;
            var dlg = new FolderBrowserDialog();
            dlg.Description = "Select the destination path for this product.";
            dlg.UseDescriptionForTitle = true;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                record = record with { Destination = dlg.SelectedPath };
                MessageBox.Show("The destination path has been updated. You can also update this by clicking on \"Apply changes\" button afterwards.", 
                    "Destination path updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Task.Run(() => handleDeletion(true));
            }
        }

        private async Task handleDeletion(bool productRemoval = false)
        {
            if (productRemoval)
            {
                var result = await DPProductRemover.RemoveProductAsync(record, Program.Database, DPSettings.CurrentSettingsObject, new DPFileSystem()).ConfigureAwait(false);
                if (result.Success)
                {
                    MessageBox.Show(record.Name + " was removed successfully.", "Product removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DisableRecordForm(record.ID);
                }
                else if (result.FailedFiles.Count == record.Files.Count)
                    MessageBox.Show("Failed to remove product files.", "Failed to remove product files", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show($"Some product files for {record.Name} failed to be removed.", "Some files failed to be removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else
            {
                var result = await DPProductRemover.RemoveRecordAsync(record, Program.Database).ConfigureAwait(false);
                if (result)
                {
                    MessageBox.Show(record.Name + " was removed from the database successfully. Files were not deleted.", "Record removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DisableRecordForm(record.ID);
                    return;
                }
                MessageBox.Show($"Failed to remove record: {record.Name}", "Failed to remove record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string[] CreateTagsArray()
        {
            var tags = new string[tagsView.Items.Count];
            for (var i = 0; i < tags.Length; i++)
            {
                tags[i] = tagsView.Items[i].Text;
            }
            return tags;
        }

        private string[] CreateFinalTagsArray()
        {
            var tags = new HashSet<string>(tagsView.Items.Count);
            for (var i = 0; i < tagsView.Items.Count; i++)
                tags.Add(tagsView.Items[i].Text);
            tags.Remove(record.Name);
            System.Text.RegularExpressions.MatchCollection oldProductNameRegexMatches = DPArchive.ProductNameRegex.Matches(record.Name);
            System.Text.RegularExpressions.MatchCollection newProductNameRegexMatches = DPArchive.ProductNameRegex.Matches(productNameTxtBox.Text);
            for (var i = 0; i < oldProductNameRegexMatches.Count; i++)
                tags.Remove(oldProductNameRegexMatches[i].Value);
            for (var i = 0; i < newProductNameRegexMatches.Count; i++)
                tags.Add(newProductNameRegexMatches[i].Value);
            tags.Add(productNameTxtBox.Text);
            return tags.ToArray();
        }
        private string GetThumbnailPath()
        {
            if (thumbnailBox.Image == Resources.NoImageFound) return null;
            else return thumbnailBox.ImageLocation;
        }

        private void applyChangesBtn_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("Are you sure you wish to apply changes? You cannot revert changes.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.No) return;
            var p = record with { Name = productNameTxtBox.Text, Tags = CreateFinalTagsArray(), ThumbnailPath = GetThumbnailPath() };
            Program.Database.UpdateRecordQ(record.ID, p);
        }

        private void editTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tagsManager = new TagsManager(CreateTagsArray());
            tagsManager.ShowDialog();

            tagsView.BeginUpdate();
            tagsView.Items.Clear();
            tagsSet.Clear();
            tagsSet = new HashSet<string>(tagsManager.tags);
            foreach (var tag in tagsSet)
            {
                tagsView.Items.Add(tag);
            }
            tagsView.EndUpdate();
        }

        private void pasteNewTagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var txt = Clipboard.GetText();
            var tags = new List<string>(txt.Split('\n'));
            var dismissedTags = false;
            foreach (var tag in tags)
            {
                if (string.IsNullOrWhiteSpace(tag)) continue;
                if (tag.Length > 80)
                {
                    dismissedTags = true;
                    continue;
                }
                tagsSet.Add(tag);
            }
            tagsView.BeginUpdate();
            tagsView.Items.Clear();
            foreach (var tag in tagsSet)
            {
                tagsView.Items.Add(tag);
            }
            tagsView.EndUpdate();
            if (dismissedTags)
                MessageBox.Show("Some tags were not added due to the size of the text being greater than 80 characters.",
                    "Some tags omitted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void removeTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tagsView.BeginUpdate();
            var c = new ListViewItem[tagsView.SelectedItems.Count];
            tagsView.SelectedItems.CopyTo(c, 0);
            for (var i = 0; i < c.Length; i++)
            {
                tagsSet.Remove(c[i].Text);
                tagsView.Items.Remove(c[i]);
            }
            tagsView.EndUpdate();

        }

        private void tagsStrip_Opening(object _, System.ComponentModel.CancelEventArgs __)
        {
            removeTagToolStripMenuItem.Enabled = copyToolStripMenuItem.Enabled = tagsView.SelectedItems.Count != 0;
            pasteNewTagsToolStripMenuItem.Enabled = Clipboard.ContainsText();
            replaceToolStripMenuItem.Enabled = tagsView.SelectedItems.Count == 1 && Clipboard.ContainsText();
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var txt = Clipboard.GetText().Trim().Split('\n');
            if (txt.Length > 1 && !string.IsNullOrWhiteSpace(txt[1]))
            {
                MessageBox.Show($"Replace failed. Make sure your clipboard contains only one line of text. Detected {txt.Length} lines of text in clipboard.", "Too many lines", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (txt[0].Length > 70)
            {
                MessageBox.Show("Replace failed due to text being longer than 70 characters. Make sure the text in your clipboard is no more than 70 characters.",
                    "Too many characters", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            tagsView.BeginUpdate();
            tagsSet.Remove(tagsView.SelectedItems[0].Text);
            tagsView.SelectedItems[0].Text = txt[0];
            tagsView.EndUpdate();
        }

        private void copyToolStripMenuItem_Click(object __, EventArgs _)
        {
            var a = new string[tagsView.SelectedItems.Count];
            for (var i = 0; i < a.Length; i++)
            {
                a[i] = tagsView.SelectedItems[i].Text;
            }
            try
            {
                Clipboard.SetText(string.Join('\n', a));
            }
            catch { }
        }

        private void tagsView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.Control)
            {
                if (e.KeyCode == Keys.C && tagsView.SelectedItems.Count != 0)
                    copyToolStripMenuItem_Click(null, null);
                else if (e.KeyCode == Keys.A) // Select all
                {
                    tagsView.BeginUpdate();
                    for (var i = 0; i < tagsView.Items.Count; i++)
                        tagsView.Items[i].Selected = true;
                    tagsView.EndUpdate();
                }
            }

            if (e.KeyCode == Keys.Delete && tagsView.SelectedItems.Count != 0)
                removeTagToolStripMenuItem_Click(null, null);

        }

        private void genericStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TabPage selectedTab = tabControl1.SelectedTab;
            var combinedPath = string.Empty;
            copyToolStripMenuItem1.Enabled = copyPathToolStripMenuItem.Enabled = openInFileExplorerToolStripMenuItem.Enabled = false;
            if (selectedTab == fileHierachyPage)
            {
                if (fileTreeView.SelectedNode == null) return;
                combinedPath = Path.Combine(record.Destination, fileTreeView.SelectedNode.FullPath);
                copyToolStripMenuItem1.Enabled = copyPathToolStripMenuItem.Enabled = true;
            }
            else if (selectedTab == fileListPage)
            {
                if (filesExtractedList.SelectedItems.Count == 0) return;
                combinedPath = Path.Combine(record.Destination, filesExtractedList.SelectedItems[0].Text);
                copyToolStripMenuItem1.Enabled = copyPathToolStripMenuItem.Enabled = true;
            }
            else return;

            openInFileExplorerToolStripMenuItem.Enabled = File.Exists(combinedPath) || Directory.Exists(combinedPath);
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TabPage selectedTab = tabControl1.SelectedTab;
            if (selectedTab == fileHierachyPage)
                Clipboard.SetText(fileTreeView.SelectedNode.Text);
            else if (selectedTab == fileListPage)
            {
                var list = new List<string>(filesExtractedList.SelectedItems.Count);
                for (var i = 0; i < filesExtractedList.SelectedItems.Count; i++)
                    list.Add(filesExtractedList.SelectedItems[i].Text);
                Clipboard.SetText(string.Join('\n', list));
            }
        }

        private void copyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage selectedTab = tabControl1.SelectedTab;
            if (selectedTab == fileHierachyPage)
                Clipboard.SetText(Path.Combine(record.Destination, fileTreeView.SelectedNode.FullPath));
            else if (selectedTab == fileListPage)
            {
                var list = new List<string>(filesExtractedList.SelectedItems.Count);
                for (var i = 0; i < filesExtractedList.SelectedItems.Count; i++)
                    list.Add(Path.Combine(record.Destination, filesExtractedList.SelectedItems[i].Text));
                Clipboard.SetText(string.Join('\n', list));
            }
        }

        private void openInFileExplorerToolStripMenuItem_Click(object _, EventArgs __)
        {
            TabPage selectedTab = tabControl1.SelectedTab;
            if (selectedTab == fileHierachyPage)
                Process.Start(@"explorer.exe", $"/select, \"{Path.Combine(record.Destination, fileTreeView.SelectedNode.FullPath).Replace('/', '\\')}\"");
            else if (selectedTab == fileListPage)
                Process.Start(@"explorer.exe", $"/select, \"{Path.Combine(record.Destination, filesExtractedList.SelectedItems[0].Text).Replace('/', '\\')}\"");
        }

        private void copyImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (thumbnailBox.Image == Resources.NoImageFound) return;
            try
            {
                Clipboard.SetImage(thumbnailBox.Image);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to copy image to clipboard.");
                MessageBox.Show($"Failed to copy image to clipboard. REASON: {ex}", "Copy image failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void copyImagePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (thumbnailBox.Image == Resources.NoImageFound || string.IsNullOrEmpty(thumbnailBox.ImageLocation)) return;
            try
            {
                Clipboard.SetText(thumbnailBox.ImageLocation);
            }
            catch { }
        }

        private void openInFileExplorerToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (thumbnailBox.Image == Resources.NoImageFound || string.IsNullOrEmpty(thumbnailBox.ImageLocation)) return;
            Process.Start(@"explorer.exe", $"/select, \"{thumbnailBox.ImageLocation.Replace('/', '\\')}\"");
        }

        private void removeImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            thumbnailBox.ImageLocation = null;
            thumbnailBox.Image = Resources.NoImageFound;
        }

        private void ProductRecordForm_FormClosed(object sender, FormClosedEventArgs e) => Program.Database.ProductRecordModified -= OnProductRecordModified;

        private void thumbnailStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            removeImageToolStripMenuItem.Enabled = copyImagePathToolStripMenuItem.Enabled =
                copyImageToolStripMenuItem.Enabled = openInFileExplorerToolStripMenuItem.Enabled =
                !string.IsNullOrEmpty(thumbnailBox.ImageLocation);
        }
    }
}
