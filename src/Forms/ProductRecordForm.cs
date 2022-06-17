// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using DAZ_Installer.DP;

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
            contentFoldersList.BeginUpdate();
            filesExtractedList.BeginUpdate();
            erroredFilesList.BeginUpdate();
            errorMessagesList.BeginUpdate();
            Array.ForEach(record.Files, file => filesExtractedList.Items.Add(file));
            Array.ForEach(record.Folders, folder => contentFoldersList.Items.Add(folder));
            Array.ForEach(record.ErroredFiles, erroredFile => erroredFilesList.Items.Add(erroredFile));
            Array.ForEach(record.ErrorMessages, errorMsg => errorMessagesList.Items.Add(errorMsg)); 
            contentFoldersList.EndUpdate();
            filesExtractedList.EndUpdate();
            erroredFilesList.EndUpdate();
            errorMessagesList.EndUpdate();
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
            return maxWidth;
        }

        private void ProductRecordForm_ResizeEnd(object sender, EventArgs e)
        {

        }
    }
}
