using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace DAZ_Installer.WinApp.Forms
{
    public partial class ContentFolderAliasManager : Form
    {
        Dictionary<string, string> Aliases = new(DPSettings.CurrentSettingsObject.FolderRedirects);
        HashSet<string> keys = new(DPSettings.CurrentSettingsObject.FolderRedirects.Count,
            StringComparer.OrdinalIgnoreCase);
        public ListView AliasListView { get; init; }
        public ContentFolderAliasManager()
        {
            InitializeComponent();
            AliasListView = aliasListView;
            SetupAliasList();
            SetupComboBox();
            SetupKeys();
            aliasListView.Columns[0].Width = aliasListView.ClientSize.Width;
        }

        private void SetupKeys() => keys.UnionWith(Aliases.Keys);

        private void SetupComboBox()
        {
            contentFoldersComboBox.BeginUpdate();
            contentFoldersComboBox.Items.AddRange(DPSettings.CurrentSettingsObject.CommonContentFolderNames.ToArray());
            contentFoldersComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            contentFoldersComboBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            contentFoldersComboBox.AutoCompleteCustomSource.AddRange(DPSettings.CurrentSettingsObject.CommonContentFolderNames.ToArray());
            contentFoldersComboBox.EndUpdate();
        }

        private void SetupAliasList()
        {
            aliasListView.BeginUpdate();
            foreach (KeyValuePair<string, string> pair in Aliases)
            {
                aliasListView.Items.Add($"{pair.Key} --> {pair.Value}");
            }
            aliasListView.EndUpdate();
        }

        private void aliasListView_Resize(object _, EventArgs __) => aliasListView.Columns[0].Width = aliasListView.ClientSize.Width;

        private void contextMenuStrip1_Opening(object _, CancelEventArgs __) => removeToolStripMenuItem.Enabled = aliasListView.SelectedItems.Count != 0;

        private void addBtn_Click(object _, EventArgs __)
        {
            var aliasTxt = addAliasTxtBox.Text.Trim();
            var comboTxt = contentFoldersComboBox.SelectedItem?.ToString().Trim() ?? string.Empty;
            if (contentFoldersComboBox.SelectedIndex == -1 || aliasTxt.Length == 0
                || comboTxt.Length == 0) return;

            if (keys.Contains(aliasTxt))
            {
                MessageBox.Show($"Alias {aliasTxt} already exists; cannot use duplicate aliases.",
                    "Alias already exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Aliases[aliasTxt] = comboTxt;
            keys.Add(aliasTxt);
            aliasListView.BeginUpdate();
            aliasListView.Items.Add($"{aliasTxt} --> {comboTxt}");
            aliasListView.EndUpdate();
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var toDeleteItems = new ListViewItem[aliasListView.SelectedItems.Count];
            aliasListView.BeginUpdate();
            aliasListView.SelectedItems.CopyTo(toDeleteItems, 0);
            foreach (ListViewItem item in toDeleteItems)
            {
                var tokens = item.Text.Split(" --> ");
                var key = tokens[0];
                keys.TryGetValue(key, out var realKey);
                Aliases.Remove(realKey);
                keys.Remove(realKey);
                aliasListView.Items.Remove(item);
            }
            aliasListView.EndUpdate();
        }

        private void resetToSavedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aliasListView.BeginUpdate();
            aliasListView.Items.Clear();
            Aliases.Clear();
            Aliases = new Dictionary<string, string>(DPSettings.CurrentSettingsObject.FolderRedirects);
            foreach (KeyValuePair<string, string> pair in Aliases)
            {
                aliasListView.Items.Add($"{pair.Key} --> {pair.Value}");
            }
            aliasListView.EndUpdate();
        }
    }
}
