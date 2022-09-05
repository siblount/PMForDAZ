// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using DAZ_Installer.DP;
using System.Threading.Tasks;
using DAZ_Installer.Utilities;

namespace DAZ_Installer
{
    public partial class ProductRecordForm : Form
    {
        private DPProductRecord record;
        private DPExtractionRecord extractionRecord;
        private uint[] maxFontWidthPerListView = new uint[5];
        public ProductRecordForm()
        {
            InitializeComponent();
            fileTreeView.StateImageList = Extract.ExtractPage.archiveFolderIcons;
        }

        public ProductRecordForm(DPProductRecord productRecord) : this()
        {
            InitializeProductRecordInfo(productRecord);
            if (productRecord.EID != 0)
                DPDatabase.GetExtractionRecordQ(productRecord.EID, 0, InitializeExtractionRecordInfo);
        }

        public void InitializeProductRecordInfo(DPProductRecord record)
        {
            this.record = record;
            productNameLbl.Text = record.ProductName;
            tagsView.BeginUpdate();
            Array.ForEach(record.Tags, tag => tagsView.Items.Add(tag));
            tagsView.EndUpdate();
            if (record.ThumbnailPath != null && File.Exists(record.ThumbnailPath))
            {
                thumbnailBox.Image = Library.self.AddReferenceImage(record.ThumbnailPath);
            }
            dateExtractedLbl.Text += record.Time.ToLocalTime().ToString();
            CalculateMaxWidthPerListView();
            UpdateColumnWidths();

        }

        public void InitializeExtractionRecordInfo(DPExtractionRecord record)
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
            OpenFileDialog dlg = new OpenFileDialog();
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
                    } catch (Exception ex)
                    {
                        DPCommon.WriteToLog($"An error occurred attempting to update thumbnail iamge. REASON: {ex}");
                        MessageBox.Show($"Unable to update thumbnail image. REASON: \n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    
                } else
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
                if (width > maxWidth) maxWidth = (uint) width;
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
                var dirName = Path.GetDirectoryName(file + ".e");
                // If it does not exist, we need to create tree nodes for this and add the root tree node to treeNodes.
                if (!folderMap.ContainsKey(dirName) && dirName.Length != 0)
                {
                    // This is to ensure that the file doesn't get treated as a directory (EX: file doesn't have an ext)
                    ReadOnlySpan<char> folderSpan = dirName;
                    char seperator = PathHelper.GetSeperator(folderSpan);
                    int lastIndexOf = folderSpan.Length;
                    TreeNode lastNode = null;
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
                        if (PathHelper.GetNumOfLevels(slice) == 0) treeNodes.Add(folderMap[slice]);
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
                if (folderMap.ContainsKey(parent))
                {
                    foreach (TreeNode node in folderMap[parent].Nodes)
                    {
                        if (node.Text == parent)
                        {
                            node.ForeColor = Color.DarkRed;
                            break;
                        }
                    }
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

            extractionRecord = new DPExtractionRecord(extractionRecord.ArchiveFileName, 
                        extractionRecord.DestinationPath, 
                        normalizedFiles.GetInnerArray(), normalizedErroredFiles.GetInnerArray(), 
                        extractionRecord.ErrorMessages, normalizedFolders.GetInnerArray(), 
                        extractionRecord.PID);
        }

        private void deleteRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show($"Are you sure you want to remove the record for {record.ProductName}? " +
                "This wont remove the files on disk. Additionally, the record cannot be restored.", "Remove product record confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            DPDatabase.RemoveProductRecord(record, OnProductRecordRemoval);
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
            var result = MessageBox.Show($"Are you sure you want to remove the record & product files for {record.ProductName}? " +
                "THIS WILL PERMANENTLY REMOVE ASSOCIATED FILES ON DISK!", "Remove product confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            uint deletedFiles = 0;
            // Try deleting at the destination path.
            if (!Directory.Exists(extractionRecord.DestinationPath))
            {
                var r = MessageBox.Show("The path at which the files were extracted to no longer exists. Do you want to check on through your current content folders?",
                    "Root content folder doesn't exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.No) return;
            }
            deletedFiles = DeleteFiles(extractionRecord.DestinationPath);
            if (deletedFiles == 0)
            {
                // Quick test.
                if (File.Exists(Path.Combine(extractionRecord.DestinationPath, extractionRecord.Files[0])))
                    MessageBox.Show("None of the product files were removed due to some error.", "Failed to remove product files", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                var r = MessageBox.Show("The path at which the files were extracted to no longer exists. Do you want to check on through your current content folders?",
                    "Root content folder doesn't exist", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r == DialogResult.No) return;
                }
                
                foreach (var contentFolder in DPSettings.currentSettingsObject.detectedDazContentPaths)
                {
                    deletedFiles = DeleteFiles(contentFolder);
                    if (deletedFiles > 0) break;
                }
            }

            var delta = extractionRecord.Files.Length - deletedFiles;
            if (delta == extractionRecord.Files.Length)
                MessageBox.Show($"Failed to remove any product files.", "Removal failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (delta > 0)
                MessageBox.Show($"Some product files failed to be removed.",
                    "Some files failed to be removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else DPDatabase.RemoveProductRecord(record);
        }

        private uint DeleteFiles(string destinationPath)
        {
            var deleteCount = 0;
            foreach (var file in extractionRecord.Files)
            {
                var deletePath = Path.Combine(extractionRecord.DestinationPath, file);
                var info = new FileInfo(deletePath);
                try
                {
                    info.Delete();
                    deleteCount++;
                }
                catch (Exception ex)
                {
                    DPCommon.WriteToLog($"Failed to remove product file for {record.ProductName}, file: {file}. REASON: {ex}");
                }
            }
            return deleteCount;
        }
    }
}
