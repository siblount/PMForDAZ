// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Core;
using DAZ_Installer.Windows.Forms;
using DAZ_Installer.Windows.DP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.Pages
{
    public partial class Home : UserControl
    {
        public static Home HomePage = null!;
        public Home()
        {
            InitializeComponent();
            HomePage = this;
        }

        private void dropBtn_Click(object sender, EventArgs e) => HandleOpenDialogue();

        internal void button1_Click(object sender, EventArgs e)
        {
            // Check that the settings are valid first. If not, do not proceed.
            if (!DPSettings.CurrentSettingsObject.Valid)
            {
                MessageBox.Show("The current settings are not valid for processing. This could be due a directory not existing, the application does not have authorized access to access the directory, or due to" +
                    "an unknown IO issue. Please check your settings.", "Settings invalid",
                                       MessageBoxButtons.OK, MessageBoxIcon.Warning);
                MainForm.SwitchPage(Settings.settingsPage);
                return;
            }

            // Clear everything from extract page.
            DPProgressCombo.RemoveAll();
            // Goto next page.
            MainForm.SwitchPage(Extract.ExtractPage);


            var newJob = new DPExtractJob(listView1.Items); // Todo: make a list.
            newJob.DoJob();

            // Clear list and reset home.
            clearListBtn_Click(null, null);
        }

        private void dropBtn_DragEnter(object sender, DragEventArgs e)
        {
            dropBtn.Text = "Drop here!";
            e.Effect = Program.DropEffect;
        }

        private void dropBtn_DragLeave(object sender, EventArgs e) => dropBtn.Text = "Click here to select file(s) or drag them here.";

        private void dropBtn_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data is null) return;
            var tmp = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            Queue<string> invalidFiles = new();
            listView1.BeginUpdate();
            // Check for string if it's valid.
            foreach (var path in tmp)
            {
                var fileInfo = new FileInfo(path);
                var ext = fileInfo.Extension;
                ext = ext.IndexOf('.') != -1 ? ext.Substring(1) : ext;
                if (fileInfo.Exists && DPFile.ValidImportExtension(ext))
                {
                    // Add to list.
                    listView1.Items.Add(path);
                }
                else
                {
                    ArchiveFormat type = DPArchive.DetermineArchiveFormatPrecise(path); // TODO: I'm pretty sure this can be removed.
                    if (type == ArchiveFormat.SevenZ && ext.EndsWith("001"))
                        listView1.Items.Add(path);
                    else invalidFiles.Enqueue(path);
                }
            }
            listView1.EndUpdate();
            if (invalidFiles.Count > 0)
            {
                var builder = new StringBuilder(50);
                while (invalidFiles.Count != 0)
                    builder.AppendLine(" \u2022 " + invalidFiles.Dequeue());
                MessageBox.Show("Files that cannot be processed where removed from the list." +
                    "\nRemoved files:\n" + builder.ToString(), "Invalid files removed", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            if (listView1.Items.Count != 0)
                dropBtn.Visible = dropBtn.Enabled = false;

            dropBtn.Text = "Click here to select file(s) or drag them here.";
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (var i = listView1.SelectedItems.Count - 1; i >= 0; i--)
            {
                listView1.Items.Remove(listView1.SelectedItems[i]);
            }
            if (listView1.Items.Count == 0) controlDragPanel(true);
        }
        private void addMoreFilesBtn_Click(object sender, EventArgs e) =>
            // Show dialogue.
            HandleOpenDialogue();

        private void controlDragPanel(bool visible)
        {
            dropBtn.Visible = dropBtn.Enabled = visible;
            if (visible) dropBtn.BringToFront();
        }

        private void HandleOpenDialogue()
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                listView1.BeginUpdate();
                foreach (var file in openFileDialog1.FileNames)
                {
                    listView1.Items.Add(file);
                }
                listView1.EndUpdate();
                listView1.BringToFront();
                controlDragPanel(false);
            }
        }

        private void clearListBtn_Click(object sender, EventArgs e)
        {
            controlDragPanel(true);
            listView1.Items.Clear();
        }

        private void homeListContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var hasSelectedItems = listView1.SelectedItems.Count != 0;
            removeToolStripMenuItem.Visible = hasSelectedItems;
        }

        private void addMoreItemsToolStripMenuItem_Click(object sender, EventArgs e) => HandleOpenDialogue();

        private void listView1_DragEnter(object sender, DragEventArgs e) => e.Effect = Program.DropEffect;
    }
}
