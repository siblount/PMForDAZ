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
            foreach (var folder in extractionRecord.Folders)
            {
                ReadOnlySpan<char> folderSpan = folder;
                char seperator = PathHelper.GetSeperator(folder);
                int lastIndexOf = folderSpan.Length;
                while (lastIndexOf != -1)
                {
                    var slice = folderSpan.Slice(0, lastIndexOf).ToString();
                    folderMap.TryAdd(slice, new TreeNode(Path.GetFileName(slice)));
                    treeNodes.Add(folderMap[slice]);
                    folderMap[slice].StateImageIndex = 0;
                    folderSpan = folderSpan.Slice(0, lastIndexOf);
                    lastIndexOf = folderSpan.LastIndexOf(seperator);
                }
            }

            // Now make parent-child connections.
            foreach (var folder in extractionRecord.Folders)
            {
                // If we are parented...
                if (folder.IndexOf('\\') != -1)
                {
                    // Make the upper one our parent
                    var upFolder = PathHelper.GetParent(folder);
                    if (folderMap.ContainsKey(upFolder))
                    {
                        folderMap[upFolder].Nodes.Add(folderMap[folder]);
                        treeNodes.Remove(folderMap[folder]);
                    }
                    else DPCommon.WriteToLog($"File Hierachy builder upper parent not found for {folder}.");
                }
                // Otherwise, we are the parent.
            }

            // Now add all the files to their folder.
            foreach (var file in extractionRecord.Files)
            {
                var parent = PathHelper.GetParent(file);
                var ext = Path.GetExtension(file);
                var treeNode = new TreeNode(Path.GetFileName(file));
                if (parent == null || file.IndexOf('\\') == -1) treeNodes.Add(treeNode);
                else
                {
                    if (folderMap.ContainsKey(parent)) folderMap[parent].Nodes.Add(treeNode);
                    else DPCommon.WriteToLog($"File Hierachy builder upper parent not found for {file}.");
                }

                if (string.IsNullOrEmpty(ext))
                    treeNode.StateImageIndex = 0;
                else if (ext.Contains("zip") || ext.Contains("7z"))
                    treeNode.StateImageIndex = 2;
                else if (ext.Contains("rar"))
                    treeNode.StateImageIndex = 1;
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
    }
}
