// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Database;
using System.ComponentModel;

namespace DAZ_Installer
{
    // TO DO: Add max tag viewing limit.
    // TO DO: Show only up to the max tag limit OR until the last tag is ellipsed.
    // TO DO: Create up to max tag limit, show tags up til the ellipsed tag.
    public partial class LibraryItem : UserControl
    {
        public static Color initialColor;
        public static Color darkerColor = Color.FromArgb(60, Color.FromKnownColor(KnownColor.ForestGreen));
        protected bool initalized = false;
        [Description("Title text"), Category("Data"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string TitleText
        {
            get => label1.Text;
            set => label1.Text = value;
        }

        [Description("Holds the image inside of the imagebox."), Category("Data"), Browsable(true)]

        public Image Image
        {
            get => imageBox.Image;
            set => imageBox.Image = value;
        }

        [Description("Holds the label tags value."), Category("Data"), Browsable(true)]
        public string[] Tags
        {
            get => GetTags();
            set => UpdateTags(value);
        }

        [Description("Determines the maximum number of tags to display."), Category("Data"), Browsable(true)]
        [DefaultValue(4)]
        public uint MaxTagCount { get; set; } = 4;
        public DPDatabase? Database { get; set; }

        public DPProductRecord ProductRecord { get; set; }
        /// <summary>
        /// The product record form to 
        /// </summary>
        public Type ProductRecordFormType { get; set; }

        private readonly List<Label> labels = new();
        public LibraryItem()
        {
            InitializeComponent();
            if (!initalized)
            {
                initalized = true;
                initialColor = label1.BackColor;
            }
        }


        private string[] GetTags()
        {
            var tags = new string[labels.Count];
            for (var i = 0; i < labels.Count; i++)
            {
                tags[i] = labels[i].Text;
            }
            return tags;
        }


        private void UpdateTags(string[] tags)
        {
            ReleaseTags(false);
            labels.Clear();
            try
            {
                var foundEllipsedLabel = false;
                var t = 0;

                for (var i = 0; i < tags.Length && t < MaxTagCount; i++)
                // TODO: Use AddRange instead of Controls.Add
                {
                    var tagName = tags[i];
                    if (string.IsNullOrEmpty(tagName) || string.IsNullOrWhiteSpace(tagName))
                        continue;
                    Label lbl = CreateTag(tags[i]);
                    //// Only immediately add to the tag layout panel if it can be seen.
                    //if (!foundEllipsedLabel) tagsLayoutPanel.Controls.Add
                    //// If the label we created is ellipsed, show no more.
                    //if (lbl.PreferredWidth > lbl.Width) foundEllipsedLabel = true;
                    // But still add it to labels so we can show it when (or if) expanded.
                    labels.Add(lbl);
                    t++;
                }
                tagsLayoutPanel.Controls.AddRange(labels.ToArray());
            }
            catch (Exception e)
            {
                // DPCommon.WriteToLog($"Failed to create tags. REASON: {e}");
            }

            tagsLayoutPanel.ResumeLayout();
        }

        private Label CreateTag(string tagName = "")
        {
            var tag = new Label();
            tag.Text = tagName;
            tag.BackColor = Color.DarkSeaGreen;
            tag.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            tag.AutoSize = true;
            tag.AutoEllipsis = true;

            return tag;
        }

        private void ReleaseTags(bool restartLayout)
        {
            // Clear tags.
            tagsLayoutPanel.SuspendLayout();
            tagsLayoutPanel.Controls.Clear();

            labels.Clear();
            if (restartLayout) tagsLayoutPanel.ResumeLayout();
        }


        private void tagsLayoutPanel_ClientSizeChanged(object sender, EventArgs e)
        {
            // Re-calculate visible tags.
            tagsLayoutPanel.SuspendLayout();
            try
            {
                var foundEllipsedLabel = false;
                for (var i = 0; i < labels.Count && i < MaxTagCount; i++)
                {
                    Label workingLabel = labels[i];

                    if (workingLabel.Parent == null)
                    {
                        // Only immediately add to the tag layout panel if it can be seen.
                        if (!foundEllipsedLabel) tagsLayoutPanel.Controls.Add(workingLabel);

                        // If the label we created is ellipsed, show no more.
                        if (workingLabel.PreferredWidth > workingLabel.Width) foundEllipsedLabel = true;
                    }
                    else
                    {
                        if (!foundEllipsedLabel)
                        {
                            if (workingLabel.PreferredWidth > workingLabel.Width) foundEllipsedLabel = true;
                        }
                        else
                        {
                            tagsLayoutPanel.Controls.Remove(workingLabel);
                            workingLabel.Parent = null;
                        }
                    }
                }
            }
            catch (Exception ee)
            {
                // DPCommon.WriteToLog($"Failed to create tags. REASON: {ee}");
            }
            tagsLayoutPanel.ResumeLayout();
        }

        private void showFoldersBtn_Click(object sender, EventArgs e)
        {
            if (ProductRecord == null)
            {
                MessageBox.Show("Unable to show record due to product record not available.", "Unable to view", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            //ProductRecordForm recordForm = new ProductRecordForm(ProductRecord);
            var form = (Form)Activator.CreateInstance(ProductRecordFormType, ProductRecord)!;
            form.ShowDialog();
            //recordForm.ShowDialog();
        }

        private void removeRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to remove the record for {ProductRecord.ProductName}? " +
                "This wont remove the files on disk. Additionally, the record cannot be restored.", "Remove product record confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            Database?.RemoveProductRecord(ProductRecord);
        }

        private void removeProductToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to remove the record & product files for {ProductRecord.ProductName}? " +
                "THIS WILL PERMANENTLY REMOVE ASSOCIATED FILES ON DISK!", "Remove product confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            Database?.GetExtractionRecordQ(ProductRecord.EID, callback: OnGetExtractionRecord);
        }

        private void OnGetExtractionRecord(DPExtractionRecord record)
        {
            if (record.PID != ProductRecord.ID) return;
            var deleteCount = 0;
            // Now delete.
            foreach (var file in record.Files)
            {
                var deletePath = Path.Combine(record.DestinationPath, file);
                var info = new FileInfo(deletePath);
                try
                {
                    info.Delete();
                    deleteCount++;
                }
                catch (Exception ex)
                {
                    // DPCommon.WriteToLog($"Failed to remove product file for {ProductRecord.ProductName}, file: {file}. REASON: {ex}");
                }
            }
            var delta = record.Files.Length - deleteCount;
            if (delta == record.Files.Length)
                MessageBox.Show($"Removal of product files completely failed for {ProductRecord.ProductName}.",
                    "Removal failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else if (delta > 0)
                MessageBox.Show($"Some product files failed to be removed for {ProductRecord.ProductName}.",
                    "Some files failed to be removed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else Database?.RemoveProductRecord(ProductRecord);
        }
    }
}
