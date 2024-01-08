using DAZ_Installer.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.TestingSuiteWindows
{
    public partial class ProcessSettingsDialogue : Form
    {
        public DPProcessSettings Settings;
        public static HashSet<char> InvalidChars = new(Path.GetInvalidFileNameChars());
        HashSet<string> keys = null!;
        public ProcessSettingsDialogue() => InitializeComponent();

        public ProcessSettingsDialogue(DPProcessSettings settings) : this() => Settings = settings;

        private void ProcessSettingsDialogue_Load(object sender, EventArgs e)
        {
            keys = new(Settings.ContentRedirectFolders!.Keys, StringComparer.OrdinalIgnoreCase);
            SuspendLayout();
            destTxtBox.Text = Settings.DestinationPath;
            tmpTxtBox.Text = Settings.TempPath;
            cfListBox.Items.AddRange(Settings.ContentFolders!.ToArray());
            cfaListBox.Items.AddRange(Settings.ContentRedirectFolders.Select(x => $"{x.Key} --> {x.Value}").ToArray());
            cfaComboBox.Items.AddRange(Settings.ContentFolders.ToArray());
            switch (Settings.InstallOption)
            {
                case InstallOptions.ManifestOnly:
                    installOptionComboBox.SelectedIndex = 0;
                    break;
                case InstallOptions.ManifestAndAuto:
                    installOptionComboBox.SelectedIndex = 1;
                    break;
                case InstallOptions.Automatic:
                    installOptionComboBox.SelectedIndex = 2;
                    break;
            }
            overwriteChkBox.Checked = Settings.OverwriteFiles;
            ResumeLayout();
        }

        private void installOptionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.InstallOption = installOptionComboBox.SelectedIndex switch
            {
                0 => InstallOptions.ManifestOnly,
                1 => InstallOptions.ManifestAndAuto,
                2 => InstallOptions.Automatic,
                _ => throw new NotImplementedException()
            };
        }

        private void addCFBtn_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            var txt = cfTxtBox.Text.Trim();
            if (txt.Length == 0) return;
            if (Settings.ContentFolders!.Contains(txt))
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
            cfListBox.Items.Add(cfTxtBox.Text);
            cfaComboBox.Items.Add(cfTxtBox.Text);
            Settings.ContentFolders.Add(cfTxtBox.Text);
            cfTxtBox.Text = string.Empty;
            ResumeLayout();
        }

        private void addCFABtn_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            var aliasTxt = cfATxtBox.Text.Trim();
            var comboTxt = cfaComboBox.SelectedItem.ToString()!.Trim() ?? string.Empty;
            if (cfaComboBox.SelectedIndex == -1 || aliasTxt.Length == 0 || comboTxt.Length == 0) return;

            if (keys.Contains(aliasTxt))
            {
                MessageBox.Show($"Alias {aliasTxt} already exists; cannot use duplicate aliases.",
                    "Alias already exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Settings.ContentRedirectFolders![aliasTxt] = comboTxt;
            keys.Add(aliasTxt);
            cfaListBox.BeginUpdate();
            cfaListBox.Items.Add($"{aliasTxt} --> {comboTxt}");
            cfaListBox.EndUpdate();
            ResumeLayout();
        }

        private void overwriteChkBox_CheckedChanged(object sender, EventArgs e)
        {
            Settings.OverwriteFiles = overwriteChkBox.Checked;
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListBox box = (ListBox)((sender as ToolStripMenuItem).Owner as ContextMenuStrip).SourceControl;
            var toDeleteItems = new string[box.SelectedItems.Count];
            box.SelectedItems.CopyTo(toDeleteItems, 0);
            box.BeginUpdate();
            foreach (var item in toDeleteItems)
            {
                if (box.SelectedItems[0].ToString().Contains(" --> "))
                {
                    var tokens = item.Split(" --> ");
                    var key = tokens[0];
                    keys.TryGetValue(key, out var realKey);
                    Settings.ContentRedirectFolders!.Remove(realKey);
                    keys.Remove(realKey);
                    box.Items.Remove(item);
                }
                else
                {
                    Settings.ContentFolders!.Remove(item);
                    box.Items.Remove(item);
                    cfaComboBox.Items.Remove(item);
                }
            }
            box.EndUpdate();
        }

        private void removeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListBox box = (ListBox)((sender as ToolStripMenuItem).Owner as ContextMenuStrip).SourceControl;
            box.BeginUpdate();
            if (box.SelectedItems[0].ToString().Contains(" --> "))
            {
                Settings.ContentRedirectFolders!.Clear();
                keys.Clear();
            }
            else
            {
                Settings.ContentFolders!.Clear();
                cfaComboBox.Items.Clear();
            }
            box.Items.Clear();
            box.EndUpdate();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListBox box = (ListBox)((sender as ToolStripMenuItem).Owner as ContextMenuStrip).SourceControl;
            if (box.SelectedItems.Count == 0) return;
            if (box.SelectedItems.Count == 1) Clipboard.SetText((string) box.SelectedItems[0]);
            else
            {
                var sb = new StringBuilder();
                foreach (string item in box.SelectedItems)
                {
                    sb.AppendLine(item);
                }
                Clipboard.SetText(sb.ToString());
            }
        }
    }
}
