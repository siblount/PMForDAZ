using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.Custom_Controls
{
    public partial class LibrarySearchItem : UserControl
    {
        public static Color initialColor;
        public static Color darkerColor = Color.FromArgb(60, Color.FromKnownColor(KnownColor.ForestGreen));
        protected bool initalized = false;
        [Description("Title text"), Category("Data"), Browsable(true), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string TitleText
        {
            get => titleText.Text;
            set => titleText.Text = value;
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

        [Description("Holds the arrow down image."), Category("Data"), Browsable(true)]


        internal DPSearchRecord SearchRecord { get; set; }

        private readonly List<Label> labels = new List<Label>((int) DPSettings.maxTagsToShow);

        private string[] GetTags()
        {
            var tags = new string[labels.Count];
            for (int i = 0; i < labels.Count; i++)
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
            catch (Exception e)
            {
                DPCommon.WriteToLog($"Failed to create tags. REASON: {e}");
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

        private void LibrarySearchItem_ClientSizeChanged(object sender, EventArgs e)
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
                DPCommon.WriteToLog($"Failed to create tags. REASON: {ee}");
            }
            tagsLayoutPanel.ResumeLayout();
        }
    }
    }
}
