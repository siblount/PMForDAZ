// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Common;
using DAZ_Installer.Database;
using Serilog;
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
        public event Action<LibraryItem>? ProductRemovalRequested;
        public event Action<LibraryItem>? ProductRecordRemovalRequested;
        private static bool initalized = false;
        [Description("Title text"), Category("Data"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string TitleText
        {
            get => titleLbl.Text;
            set => titleLbl.Text = value;
        }

        [Description("Holds the image inside of the imagebox."), Category("Data"), Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Image? Image
        {
            get => imageBox.Image;
            set => imageBox.Image = value;
        }

        [Description("Holds the label tags value."), Category("Data"), Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public IReadOnlyList<string> Tags
        {
            get => GetTags();
            set => UpdateTags(value);
        }

        [Description("Determines the maximum number of tags to display."), Category("Data"), Browsable(true)]
        [DefaultValue(4)]
        public uint MaxTagCount { get; set; } = 4;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DPDatabase? Database { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DPProductRecordLite ProductRecord { get; set; }
        /// <summary>
        /// The product record form to 
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Type ProductRecordFormType { get; set; }

        private readonly List<Label> labels = new();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ILogger Logger { get; set; } = Log.ForContext<LibraryItem>();
        public LibraryItem()
        {
            InitializeComponent();
            if (!initalized)
            {
                initalized = true;
                initialColor = titleLbl.BackColor;
            }
        }
        private byte UpdateCount = 0;

        public void BeginUpdate()
        {
            if (UpdateCount++ != 0) return;
            tagsLayoutPanel.SuspendLayout();
            SuspendLayout();
        }

        /// <summary>
        /// Ends an update operation on the LibraryItem.
        /// </summary>
        /// <remarks>
        /// If the internal update count is not zero, resume layout will not occur.
        /// Consequently, <paramref name="resumeLayout"/> will be honored only if the update count
        /// is zero.
        /// </remarks>
        /// <param name="resumeLayout">
        /// Determines whether layout operations should not render 
        /// immediately (false) or should render immediately (true).
        /// </param>
        public void EndUpdate(bool resumeLayout = true)
        {
            if (--UpdateCount != 0) return;
            tagsLayoutPanel.ResumeLayout(resumeLayout);
            ResumeLayout(resumeLayout);
        }

        public void Reset(bool resumeLayout = false)
        {
            if (InvokeRequired) Invoke(Reset);
            else
            {
                BeginUpdate();
                try
                {
                    ReleaseTags();
                    Image = null;
                    ProductRecord = new("Reset", null, [], -1);
                    
                    // Clear all events
                    ProductRemovalRequested = null;
                    ProductRecordRemovalRequested = null;
                } catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to reset a library item to completion");
                }
                EndUpdate(resumeLayout);
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


        private void UpdateTags(IReadOnlyList<string> tags)
        {
            BeginUpdate();
            ReleaseTags();
            try
            {
                var t = 0;

                for (var i = 0; i < tags.Count && t < MaxTagCount; i++)
                {
                    var tagName = tags[i];
                    if (string.IsNullOrWhiteSpace(tagName))
                        continue;
                    Label lbl = CreateTag(tags[i]);
                    labels.Add(lbl);
                    t++;
                }
                tagsLayoutPanel.Controls.AddRange([..labels]);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to update tags");
            }
            EndUpdate();
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

        private void ReleaseTags()
        {
            BeginUpdate();
            tagsLayoutPanel.Controls.Clear();
            labels.Clear();
            EndUpdate();
        }

        private void showFoldersBtn_Click(object sender, EventArgs e)
        {
            var form = (Form)Activator.CreateInstance(ProductRecordFormType, ProductRecord)!;
            form.ShowDialog();
        }

        private void removeRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to remove the record for {ProductRecord.Name}? " +
                "This won't remove the files on disk and the record cannot be restored.", "Remove product record confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            ProductRecordRemovalRequested?.Invoke(this);
        }

        private void removeProductToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Are you sure you want to remove the record & product files for {ProductRecord.Name}? " +
                "THIS CAN PERMANENTLY REMOVE ASSOCIATED FILES ON DISK (depending on Delete Action setting)!", "Remove product confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No) return;
            ProductRemovalRequested?.Invoke(this);
        }
    }
}
