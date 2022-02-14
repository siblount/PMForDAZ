// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.IO;

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
        [Description("Holds the folder relative path to content or appdata."), Category("Data"), Browsable(true)]
        public string[] Folders
        {
            get => GetFolders();
            set => UpdateFolders(value);
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

        [Description("The value of the folder label visibility."), Category("Data"), Browsable(true)]
        public bool ShowFolder
        {
            get => foldersLabel.Visible;
            set {
                foldersLabel.Visible = value;
                foldersLabel.Enabled = true;
                if (value == true) showFoldersBtn.Image = arrowDownImage;
                else showFoldersBtn.Image = arrowRightImage;
            }
        }

        [Description("Holds the arrow down image."), Category("Data"), Browsable(true)]

        public Image ArrowDownImage
        {
            get => arrowDownImage;
            set => arrowDownImage = value;
        }

        [Description("Holds the arrow right image."), Category("Data"), Browsable(true)]

        public Image ArrowRightImage
        {
            get => arrowRightImage;
            set => arrowRightImage = value;
        }

        internal DPProductRecord ProductRecord { get; set; }

        private Image arrowDownImage;
        private Image arrowRightImage;

        private readonly List<Label> folderLabels = new List<Label>();

        private readonly List<Label> labels = new List<Label>();
        public LibraryItem()
        {
            InitializeComponent();
            if (!initalized)
            {
                initalized = true;
                initialColor = label1.BackColor;
            }
        }

        
        private string[] GetFolders()
        {
            return folderLabels[0].Text.Split('\n');
        }

        private string[] GetTags()
        {
            var tags = new string[labels.Count];
            for (int i = 0; i < labels.Count; i++)
            {
                tags[i] = labels[i].Text;
            }
            return tags;
        }

        private void UpdateFolders(string[] folders)
        {
            folderLabels.Clear();
            var foldersToString = string.Join("\n", folders);
            CreateFolder(foldersToString);
        }

        private void UpdateTags(string[] tags)
        {
            ReleaseTags(false);
            labels.Clear();
            try
            {
                var foundEllipsedLabel = false;
                var t = 0;

                for (var i = 0; i < tags.Length && t < DPSettings.maxTagsToShow; i++)
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
            catch (Exception e) {
                DPCommon.WriteToLog($"Failed to create tags. REASON: {e}");
            }

            tagsLayoutPanel.ResumeLayout();
        }

        private void CreateFolder(string folderName = "")
        {
            foldersLabel.Text = folderName;
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
        
        public void HandleFolderClick(object _, EventArgs __)
        {
            ShowFolder = !ShowFolder;
        }

        private void LibraryItem_Load(object sender, EventArgs e)
        {
            showFoldersBtn.Image = arrowRightImage;
        }

        private void tagsLayoutPanel_ClientSizeChanged(object sender, EventArgs e)
        {
            // Re-calculate visible tags.
            tagsLayoutPanel.SuspendLayout();
            try
            {
                var foundEllipsedLabel = false;
                for (var i = 0; i < labels.Count && i < DPSettings.maxTagsToShow; i++)
                {
                    var workingLabel = labels[i];
                    
                    if (workingLabel.Parent == null)
                    {
                        // Only immediately add to the tag layout panel if it can be seen.
                        if (!foundEllipsedLabel) tagsLayoutPanel.Controls.Add(workingLabel);

                        // If the label we created is ellipsed, show no more.
                        if (workingLabel.PreferredWidth > workingLabel.Width) foundEllipsedLabel = true;
                    } else
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
                DPCommon.WriteToLog($"Failed to create tags. REASON: {ee}");
            }
            tagsLayoutPanel.ResumeLayout();
        }

        private void foldersLabel_VisibleChanged(object sender, EventArgs e)
        {
            foldersLabel.MaximumSize = new Size((int) ((340 / 500f) * Size.Width), 9999);
        }

        private void LibraryItem_Resize(object sender, EventArgs e)
        {
            foldersLabel.MaximumSize = new Size((int)((340 / 500f) * Size.Width), 9999);
        }
    }
}
