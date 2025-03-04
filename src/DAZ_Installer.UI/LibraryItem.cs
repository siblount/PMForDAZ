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
        private readonly List<string> tags = new();

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

        private const int TAG_X_PADDING = 1;
        private const int TAG_Y_PADDING = 0;
        private const int TAG_SPACING = 5;
        private const float TAG_FONT_SIZE = 10.2F;
        private readonly Font tagFont = new("Segoe UI", TAG_FONT_SIZE, FontStyle.Regular, GraphicsUnit.Point);
        private readonly Color tagBackColor = Color.DarkSeaGreen;
        private readonly Color tagTextColor = Color.Black;

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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RenderTags(e.Graphics);
        }

        private void RenderTags(Graphics g)
        {
            if (tags.Count == 0) return;

            // Calculate the tags area
            var tagsArea = new Rectangle(
                135, 
                44, 
                Width - 150,  // Dynamic width based on control size, leaving some padding
                21
            );

            float currentX = tagsArea.X;
            float currentY = tagsArea.Y;

            using var tagBrush = new SolidBrush(tagBackColor);
            using var textBrush = new SolidBrush(tagTextColor);

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            for (int i = 0; i < tags.Count && i < MaxTagCount; i++)
            {
                var tagText = tags[i];
                var textSize = g.MeasureString(tagText, tagFont);
                
                // Calculate tag rectangle with padding
                var tagWidth = textSize.Width + (TAG_X_PADDING * 2);
                var tagHeight = textSize.Height + TAG_Y_PADDING;

                // If a single tag is too wide, truncate it with ellipsis
                if (tagWidth > tagsArea.Width && i == 0)
                {
                    // Draw at least one tag with truncation
                    tagWidth = tagsArea.Width - TAG_X_PADDING;
                    tagText = TruncateText(tagText, tagWidth - (TAG_X_PADDING * 2), textSize.Width);
                    textSize = new SizeF(tagWidth - (TAG_X_PADDING * 2), textSize.Height);
                }
                // For subsequent tags, check if there's enough space
                else if (currentX + tagWidth > tagsArea.Right) break;

                // Center the tag vertically in the available space
                float tagY = currentY + (tagsArea.Height - tagHeight) / 2;

                // Draw tag background
                var tagRect = new RectangleF(currentX, tagY, tagWidth, tagHeight);
                g.FillRectangle(tagBrush, tagRect);

                // Draw tag text
                var textRect = new RectangleF(
                    currentX + TAG_X_PADDING,
                    tagY + 1,
                    textSize.Width,
                    textSize.Height);
                g.DrawString(tagText, tagFont, textBrush, textRect);

                // Move to next tag position
                currentX += tagWidth + TAG_SPACING;
            }
        }

        private string TruncateText(string text, float maxWidth, float currentWidth)
        {
            string ellipsis = "...";
            if (currentWidth <= maxWidth) return text;

            // Calculate how much we need to remove based on the width ratio
            float ratio = maxWidth / currentWidth;
            int estimatedLength = (int)(text.Length * ratio) - 1; // -1 for safety
            
            // Start with estimated length to reduce iterations
            int len = Math.Max(0, Math.Min(text.Length, estimatedLength));
            return text[..len] + ellipsis;
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            Invalidate();
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


        private void UpdateTags(IReadOnlyList<string> newTags)
        {
            tags.Clear();
            tags.AddRange(newTags.Where(t => !string.IsNullOrWhiteSpace(t)));
            Invalidate(); // Trigger repaint
        }

        private void ReleaseTags()
        {
            BeginUpdate();
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
