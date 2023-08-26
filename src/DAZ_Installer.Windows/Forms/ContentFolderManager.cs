using DAZ_Installer.Windows.DP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.Forms
{
    public partial class ContentFolderManager : Form
    {
        public static HashSet<char> InvalidChars = new(Path.GetInvalidFileNameChars());
        public HashSet<string> ContentFolders { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public ContentFolderManager()
        {
            InitializeComponent();
            ContentFolders.UnionWith(DPSettings.CurrentSettingsObject.CommonContentFolderNames);
            SetupContentFoldersList();
            contentFoldersView.Columns[0].Width = contentFoldersView.ClientSize.Width;
        }

        private void SetupContentFoldersList()
        {
            contentFoldersView.BeginUpdate();
            foreach (var item in ContentFolders)
            {
                contentFoldersView.Items.Add(item);
            }
            contentFoldersView.EndUpdate();
        }

        private void listViewContextMenu_Opening(object sender, CancelEventArgs e)
        {
            removeToolStripMenuItem.Enabled = copyToolStripMenuItem.Enabled =
                contentFoldersView.SelectedIndices.Count != 0;
        }

        private void resetToDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            contentFoldersView.BeginUpdate();
            contentFoldersView.Items.Clear();
            ContentFolders.Clear();
            ContentFolders.UnionWith(DPSettings.CurrentSettingsObject.CommonContentFolderNames);
            foreach (var item in ContentFolders)
            {
                contentFoldersView.Items.Add(item);
            }
            contentFoldersView.EndUpdate();
        }

        // Used to update the width of the invisible column; without this items will be truncated.
        private void contentFoldersView_Resize(object sender, EventArgs e) => contentFoldersView.Columns[0].Width = contentFoldersView.ClientSize.Width;

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var count = contentFoldersView.SelectedItems.Count;
            if (count == 1)
            {
                Clipboard.SetText(contentFoldersView.SelectedItems[0].Text);
                return;
            }
            var builder = new StringBuilder(50);
            for (var i = 0; i < contentFoldersView.SelectedItems.Count; i++)
            {
                builder.AppendLine(contentFoldersView.SelectedItems[i].Text);
            }
            Clipboard.SetText(builder.ToString());
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var toDeleteItems = new ListViewItem[contentFoldersView.SelectedItems.Count];
            contentFoldersView.SelectedItems.CopyTo(toDeleteItems, 0);
            contentFoldersView.BeginUpdate();
            foreach (ListViewItem item in toDeleteItems)
            {
                contentFoldersView.Items.Remove(item);
                ContentFolders.Remove(item.Text);
            }
            contentFoldersView.EndUpdate();
        }

        private void addBtn_Click(object sender, EventArgs e)
        {
            var txt = contentFolderTxtBox.Text.Trim();
            if (txt.Length == 0) return;
            if (ContentFolders.Contains(txt))
            {
                MessageBox.Show("Cannot add duplicate content folders; a content folder with that name already exists.",
                    "Name already exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            for (var i = 0; i < txt.Length; i++)
            {
                if (InvalidChars.Contains(txt[i]))
                {
                    MessageBox.Show("Cannot add content folder due to forbidden characters in name.",
                        "Forbidden characters not allowed by OS", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            contentFoldersView.BeginUpdate();
            contentFoldersView.Items.Add(contentFolderTxtBox.Text);
            ContentFolders.Add(contentFolderTxtBox.Text);
            contentFolderTxtBox.Text = string.Empty;
            contentFoldersView.EndUpdate();
        }

        // Alias for copy.
        private void contentFoldersView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C && contentFoldersView.SelectedItems.Count != 0)
                copyToolStripMenuItem_Click(null, null);
        }

        private void contentFoldersView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Delete && contentFoldersView.SelectedItems.Count != 0)
                removeToolStripMenuItem_Click(null, null);
        }

        private void contentFoldersView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers != Keys.None || contentFoldersView.SelectedItems.Count == 0) return;
            if (e.KeyCode == Keys.Delete) removeToolStripMenuItem_Click(null, null);
        }

        private void contentFolderTxtBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Modifiers == Keys.None && e.KeyCode == Keys.Enter)
                addBtn_Click(null, null);
        }
    }
}
