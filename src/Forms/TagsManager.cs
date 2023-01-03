using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace DAZ_Installer.Forms
{
    public partial class TagsManager : Form
    {
        internal string[] tags;
        public TagsManager()
        {
            InitializeComponent();
        }

        public TagsManager(string[] tags) : this()
        {
            this.tags = tagsTxtBox.Lines = tags;
        }

        private void updateBtn_Click(object sender, EventArgs e)
        {
            List<string> tags = new List<string>(tagsTxtBox.Text.Split('\n'));
            int c = 0;
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                if (string.IsNullOrEmpty(tag) || string.IsNullOrWhiteSpace(tag))
                {
                    c++;
                    tags[i] = tags[tags.Count - 1 - i];
                } else if (tag.Length > 70)
                {
                    MessageBox.Show("Some lines are greater than 70 characters, please make sure each line is no more than 70 characters and try again.", 
                        "Tags too long", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            tags.RemoveRange(tags.Count - 1 - c, c);
            this.tags = tags.ToArray();
            Close();
        }

        private void restoreBtn_Click(object sender, EventArgs e)
        {
            tagsTxtBox.Lines = tags;
        }
    }
}
