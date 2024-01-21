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

namespace DAZ_Installer.Windows.Forms
{
    public partial class ProductRecordForm : Form
    {
        private ILogger logger = Log.Logger.ForContext<ProductRecordForm>();
        private DPProductRecord record;
        private DPProductRecordLite extractionRecord;
        private uint[] maxFontWidthPerListView = new uint[5];
        private HashSet<string> tagsSet = new();
        public ProductRecordForm()
        {
            InitializeComponent();
            fileTreeView.StateImageList = Extract.ExtractPage.archiveFolderIcons;
            if (DPGlobal.isWindows11)
                applyChangesBtn.Size = new Size(applyChangesBtn.Size.Width, applyChangesBtn.Size.Height + 2);
        }

        public ProductRecordForm(DPProductRecord productRecord) : this()
        {
            InitializeProductRecordInfo(productRecord);
            if (productRecord.EID != 0)
                Program.Database.GetExtractionRecordQ(productRecord.EID, 0, InitializeExtractionRecordInfo);
        }

        public void InitializeProductRecordInfo(DPProductRecord record)
        {
            this.record = record;
            productNameTxtBox.Text = record.ProductName;
            authorLbl.Text += string.IsNullOrEmpty(record.Author) ? "Not detected" : record.Author;
            tagsView.BeginUpdate();
            Array.ForEach(record.Tags, tag => tagsView.Items.Add(tag));
            tagsSet = new HashSet<string>(record.Tags);
            tagsView.EndUpdate();
            if (record.ThumbnailPath != null && File.Exists(record.ThumbnailPath))
            {
                thumbnailBox.Image = Library.self.AddReferenceImage(record.ThumbnailPath);
                thumbnailBox.ImageLocation = record.ThumbnailPath;
            }
            dateExtractedLbl.Text += record.Time.ToLocalTime().ToString();
            CalculateMaxWidthPerListView();
            UpdateColumnWidths();
            Program.Database.ProductRecordModified += OnProductRecordModified;

        }

        private void OnProductRecordModified(DPProductRecord newProductRecord, uint id)
        {
            if (id == record.ID)
            {
                MessageBox.Show("Product record successfully updated!", "Product record updated successfully.");
            }
            record = newProductRecord;
        }

        public void InitializeExtractionRecordInfo(DPProductRecordLite record)
        {

            if (record.PID != this.record.ID) return;
            extractionRecord = record;
            NormalizeExtractionRecord();
            contentFoldersList.BeginUpdate();
            filesExtractedList.BeginUpdate();
            erroredFilesList.BeginUpdate();
            errorMessagesList.BeginUpdate();
            fileTreeView.BeginUpdate();
            Array.ForEach(extractionRecord.Files, file => filesExtractedList.Items.Add(file));
            Array.ForEach(extractionRecord.Folders, folder => contentFoldersList.Items.Add(folder));
            Array.ForEach(extractionRecord.ErroredFiles, erroredFile => erroredFilesList.Items.Add(erroredFile));
            Array.ForEach(extractionRecord.ErrorMessages, errorMsg => errorMessagesList.Items.Add(errorMsg));

            BuildFileHierachy();
            contentFoldersList.EndUpdate();
            filesExtractedList.EndUpdate();
            erroredFilesList.EndUpdate();
            errorMessagesList.EndUpdate();
            fileTreeView.EndUpdate();
            destinationPathLbl.Text += record.DestinationPath;
            CalculateMaxWidthPerListView();
            UpdateColumnWidths();
        }

        private void browseImageBtn_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Supported Images (png, jpeg, bmp)|*.png;*.jpg;*.jpeg;*.bmp";
            dlg.Title = "Select thumbnail image";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
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
                        // DPCommon.WriteToLog($"An error occurred attempting to update thumbnail iamge. REASON: {ex}");
                        MessageBox.Show($"Unable to update thumbnail image. REASON: \n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                else
                {
                    MessageBox.Show($"Unable to update image due to it not being found (or able to be accessed).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateColumnWidths()
        {
            tagsView.Columns[0].Width = (int)maxFontWidthPerListView[0];
            contentFoldersList.Columns[0].Width = (int)maxFontWidthPerListView[1];
            filesExtractedList.Columns[0].Width = (int)maxFontWidthPerListView[2];
            erroredFilesList.Columns[0].Width = (int)maxFontWidthPerListView[3];
            errorMessagesList.Columns[0].Width = (int)maxFontWidthPerListView[4];
        }

        private void CalculateMaxWidthPerListView()
        {
            maxFontWidthPerListView[0] = GetMaxWidth(tagsView.Items);
            maxFontWidthPerListView[1] = GetMaxWidth(contentFoldersList.Items);
            maxFontWidthPerListView[2] = GetMaxWidth(filesExtractedList.Items);
            maxFontWidthPerListView[3] = GetMaxWidth(erroredFilesList.Items);
            maxFontWidthPerListView[4] = GetMaxWidth(errorMessagesList.Items);
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
            var folderMap = new Dictionary<string, TreeNode>(extractionRecord.Folders.Length);
            var treeNodes = new HashSet<TreeNode>(extractionRecord.Files.Length + extractionRecord.Folders.Length);
            // Initalize the map by just connecting a folder path to a tree node.
            foreach (var file in extractionRecord.Files)
            {
                var dirName = Path.GetDirectoryName(file + ".e")!;
                // If it does not exist, we need to create tree nodes for this and add the root tree node to treeNodes.
                if (!folderMap.ContainsKey(dirName) && dirName.Length != 0)
                {
                    // This is to ensure that the file doesn't get treated as a directory (EX: file doesn't have an ext)
                    ReadOnlySpan<char> folderSpan = dirName;
                    var seperator = PathHelper.GetSeperator(folderSpan);
                    var lastIndexOf = folderSpan.Length;
                    TreeNode? lastNode = null;
                    while (lastIndexOf != -1)
                    {
                        var slice = folderSpan.Slice(0, lastIndexOf).ToString();
                        var added = folderMap.TryAdd(slice, new TreeNode(Path.GetFileName(slice)));
                        if (!added)
                        {
                            if (lastNode == null) break;
                            folderMap[slice].Nodes.Add(lastNode);
                            break;
                        }
                        if (PathHelper.GetSubfoldersCount(slice) == 0) treeNodes.Add(folderMap[slice]);
                        if (lastNode != null) folderMap[slice].Nodes.Add(lastNode);
                        folderMap[slice].StateImageIndex = 0;
                        lastNode = folderMap[slice];
                        folderSpan = folderSpan.Slice(0, lastIndexOf);
                        lastIndexOf = folderSpan.LastIndexOf(seperator);
                    }
                }
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
            // Color red files that errored.
            foreach (var file in extractionRecord.ErroredFiles)
            {
                var parent = PathHelper.GetParent(file);
                if (!folderMap.ContainsKey(parent)) continue;
                foreach (TreeNode node in folderMap[parent].Nodes)
                {
                    if (node.Text != parent) continue;
                    node.ForeColor = Color.DarkRed;
                    break;
                }
            }
            var treeNodesArr = new TreeNode[treeNodes.Count];
            treeNodes.CopyTo(treeNodesArr);
            fileTreeView.Nodes.AddRange(treeNodesArr);
        }

        private void NormalizeExtractionRecord()
        {
            var normalizedFolders = new List<string>(extractionRecord.Folders.Length);
            foreach (var folder in extractionRecord.Folders)
                normalizedFolders.Add(PathHelper.NormalizePath(folder));

            var normalizedFiles = new List<string>(extractionRecord.Files.Length);
            foreach (var file in extractionRecord.Files)
                normalizedFiles.Add(PathHelper.NormalizePath(file));

            var normalizedErroredFiles = new List<string>(extractionRecord.ErroredFiles.Length);
            foreach (var file in extractionRecord.ErroredFiles)
                normalizedErroredFiles.Add(PathHelper.NormalizePath(file));

            extractionRecord = new DPProductRecordLite(extractionRecord.ArchiveFileName,
                        extractionRecord.DestinationPath,
                        normalizedFiles.ToArray(), normalizedErroredFiles.ToArray(),
                        extractionRecord.ErrorMessages, normalizedFolders.ToArray(),
                        extractionRecord.PID);
        }

        private void deleteRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to remove the record for {record.ProductName}? " +
                "This wont remove the files on disk. Additionally, the record cannot be restored.", "Remove product record confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            Program.Database.RemoveProductRecordQ(record, OnProductRecordRemoval);
        }

        private void OnProductRecordRemoval(uint id)
        {
            if (record.ID == id)
            {
                MessageBox.Show("This product record has been removed in the database and can no longer be updated.");
                applyChangesBtn.Enabled = false;
                toolStrip1.Enabled = false;
            }
        }

        private void deleteProductToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to remove the record & product files for {record.ProductName}? " +
                "THIS WILL PERMANENTLY REMOVE ASSOCIATED FILES ON DISK!", "Remove product confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            uint deletedFiles = 0;
            // Try deleting at the destination path.
            if (!Directory.Exists(extractionRecord.DestinationPath))
            {
                DialogResult r = MessageBox.Show("The path at which the files were extracted to no longer exists. Do you want to check on through your current content folders?",
                    "Root content folder doesn't exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.No) return;
            }
            deletedFiles = DeleteFiles();
            if (deletedFiles == 0)
            {
                // Quick test.
                if (File.Exists(Path.Combine(extractionRecord.DestinationPath, extractionRecord.Files[0])))
                    MessageBox.Show("None of the product files were removed due to some error.", "Failed to remove product files",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    DialogResult r = MessageBox.Show("The path at which the files were extracted to no longer exists. Do you want to check on through your current content folders?",
                    "Root content folder doesn't exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r == DialogResult.No) return;
                }
            }

            var delta = extractionRecord.Files.Length - deletedFiles;
            if (delta == extractionRecord.Files.Length)
                MessageBox.Show($"Failed to remove any product files.", "Removal failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (delta > 0)
                MessageBox.Show($"Some product files failed to be removed.",
                    "Some files failed to be removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else Program.Database.RemoveProductRecordQ(record);
            Program.Database.RemoveProductRecordQ(record, OnProductRecordRemoval);
        }

        private uint DeleteFiles()
        {
            var fs = new DPFileSystem(new DPFileScopeSettings(Array.Empty<string>(), new[] { extractionRecord.DestinationPath }, false));
            uint deleteCount = 0;
            foreach (var file in extractionRecord.Files)
            {
                fs.CreateFileInfo(Path.Combine(extractionRecord.DestinationPath, file)).TryAndFixDelete(out var ex);
                if (ex != null) logger.Error(ex, "Failed to remove product file {file} for {record}", file, record.ProductName);
                else deleteCount++;
            }
            return deleteCount;
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
            tags.Remove(record.ProductName);
            System.Text.RegularExpressions.MatchCollection oldProductNameRegexMatches = DPArchive.ProductNameRegex.Matches(record.ProductName);
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
            DialogResult r = MessageBox.Show("Are you sure you wish up apply changes? You cannot revert changes.", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.No) return;
            var p = new DPProductRecord(productNameTxtBox.Text, CreateFinalTagsArray(), record.Author, record.SKU, record.Time, GetThumbnailPath(), record.EID, record.ID);
            Program.Database.UpdateRecordQ(record.ID, p, extractionRecord);
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
                combinedPath = Path.Combine(extractionRecord.DestinationPath, fileTreeView.SelectedNode.FullPath);
                copyToolStripMenuItem1.Enabled = copyPathToolStripMenuItem.Enabled = true;
            }
            else if (selectedTab == contentFoldersPage)
            {
                if (contentFoldersList.SelectedItems.Count == 0) return;
                combinedPath = Path.Combine(extractionRecord.DestinationPath, contentFoldersList.SelectedItems[0].Text);
                copyToolStripMenuItem1.Enabled = copyPathToolStripMenuItem.Enabled = true;
            }
            else if (selectedTab == fileListPage)
            {
                if (filesExtractedList.SelectedItems.Count == 0) return;
                combinedPath = Path.Combine(extractionRecord.DestinationPath, filesExtractedList.SelectedItems[0].Text);
                copyToolStripMenuItem1.Enabled = copyPathToolStripMenuItem.Enabled = true;
            }
            else if (selectedTab == erroredFilesPage)
            {
                if (erroredFilesList.SelectedItems.Count == 0) return;
                combinedPath = Path.Combine(extractionRecord.DestinationPath, erroredFilesList.SelectedItems[0].Text);
                copyToolStripMenuItem.Enabled = copyPathToolStripMenuItem.Enabled = true;
            }
            else if (selectedTab == errorMessagesPage)
            {
                if (errorMessagesList.SelectedItems.Count == 0) return;
                copyToolStripMenuItem.Enabled = true;
            }
            else return;

            openInFileExplorerToolStripMenuItem.Enabled = File.Exists(combinedPath) || Directory.Exists(combinedPath);
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TabPage selectedTab = tabControl1.SelectedTab;
            if (selectedTab == fileHierachyPage)
                Clipboard.SetText(fileTreeView.SelectedNode.Text);
            else if (selectedTab == contentFoldersPage)
            {
                var list = new List<string>(contentFoldersList.SelectedItems.Count);
                for (var i = 0; i < contentFoldersList.SelectedItems.Count; i++)
                    list.Add(contentFoldersList.SelectedItems[i].Text);
                Clipboard.SetText(string.Join('\n', list));
            }
            else if (selectedTab == fileListPage)
            {
                var list = new List<string>(filesExtractedList.SelectedItems.Count);
                for (var i = 0; i < filesExtractedList.SelectedItems.Count; i++)
                    list.Add(filesExtractedList.SelectedItems[i].Text);
                Clipboard.SetText(string.Join('\n', list));
            }
            else if (selectedTab == erroredFilesPage)
            {
                var list = new List<string>(erroredFilesList.SelectedItems.Count);
                for (var i = 0; i < erroredFilesList.SelectedItems.Count; i++)
                    list.Add(erroredFilesList.SelectedItems[i].Text);
                Clipboard.SetText(string.Join('\n', list));
            }
            else if (selectedTab == errorMessagesPage)
            {
                var list = new List<string>(errorMessagesList.SelectedItems.Count);
                for (var i = 0; i < errorMessagesList.SelectedItems.Count; i++)
                    list.Add(errorMessagesList.SelectedItems[i].Text);
                Clipboard.SetText(string.Join('\n', list));
            }
        }

        private void copyPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TabPage selectedTab = tabControl1.SelectedTab;
            if (selectedTab == fileHierachyPage)
                Clipboard.SetText(Path.Combine(extractionRecord.DestinationPath, fileTreeView.SelectedNode.FullPath));
            else if (selectedTab == contentFoldersPage)
            {
                var list = new List<string>(contentFoldersList.SelectedItems.Count);
                for (var i = 0; i < contentFoldersList.SelectedItems.Count; i++)
                    list.Add(Path.Combine(extractionRecord.DestinationPath, contentFoldersList.SelectedItems[i].Text));
                Clipboard.SetText(string.Join('\n', list));
            }
            else if (selectedTab == fileListPage)
            {
                var list = new List<string>(filesExtractedList.SelectedItems.Count);
                for (var i = 0; i < filesExtractedList.SelectedItems.Count; i++)
                    list.Add(Path.Combine(extractionRecord.DestinationPath, filesExtractedList.SelectedItems[i].Text));
                Clipboard.SetText(string.Join('\n', list));
            }
            else if (selectedTab == erroredFilesPage)
            {
                var list = new List<string>(erroredFilesList.SelectedItems.Count);
                for (var i = 0; i < erroredFilesList.SelectedItems.Count; i++)
                    list.Add(Path.Combine(extractionRecord.DestinationPath, erroredFilesList.SelectedItems[i].Text));
                Clipboard.SetText(string.Join('\n', list));
            }
        }

        private void openInFileExplorerToolStripMenuItem_Click(object _, EventArgs __)
        {
            TabPage selectedTab = tabControl1.SelectedTab;
            if (selectedTab == fileHierachyPage)
                Process.Start(@"explorer.exe", $"/select, \"{Path.Combine(extractionRecord.DestinationPath, fileTreeView.SelectedNode.FullPath).Replace('/', '\\')}\"");
            else if (selectedTab == contentFoldersPage)
                Process.Start(@"explorer.exe", $"/select, \"{Path.Combine(extractionRecord.DestinationPath, contentFoldersList.SelectedItems[0].Text).Replace('/', '\\')}\"");
            else if (selectedTab == fileListPage)
                Process.Start(@"explorer.exe", $"/select, \"{Path.Combine(extractionRecord.DestinationPath, filesExtractedList.SelectedItems[0].Text).Replace('/', '\\')}\"");
            else if (selectedTab == erroredFilesPage)
                Process.Start(@"explorer.exe", $"/select, \"{Path.Combine(extractionRecord.DestinationPath, erroredFilesList.SelectedItems[0].Text).Replace('/', '\\')}\"");
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
                MessageBox.Show($"Failed to copy image to clipboard. REASON: {ex}", "Copy image failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // DPCommon.WriteToLog($"Failed to copy image to clipboard. REASON: {ex}");
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
