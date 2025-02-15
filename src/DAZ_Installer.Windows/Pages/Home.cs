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
using DAZ_Installer.IO;
using System.Linq;

namespace DAZ_Installer.Windows.Pages
{
    public partial class Home : UserControl
    {
        public static Home HomePage = null!;
        public DPFileSystem FileSystem = new();
        public Home()
        {
            InitializeComponent();
            HomePage = this;
            titleLbl.Text = Program.AppName;

            RegisterGlobalEvents(this);
        }

        private void RegisterGlobalEvents(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                control.AllowDrop = true;
                control.DragDrop += Home_DragDrop;
                control.DragEnter += Home_DragEnter;
                RegisterGlobalEvents(control);
            }
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
                Settings.settingsPage.SwitchToSettings(true);
                return;
            }

            // Clear everything from extract page.
            Extract.ExtractPage.ResetExtractPage();

            // Goto next page.
            MainForm.SwitchPage(Extract.ExtractPage);
            var newJob = new DPExtractJob(listView1.Items.Cast<ListViewItem>().Select(x => x.Text)); // Todo: make a list.
            newJob.DoJob();

            // Clear list and reset home.
            clearListBtn_Click(null, null);
        }

        private void dropBtn_DragEnter(object sender, DragEventArgs e) => dropBtn.Text = "Drop here!";

        private void dropBtn_DragLeave(object sender, EventArgs e) => dropBtn.Text = "Click here to select file(s) or drag them here.";

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
                HandleNewFiles(openFileDialog1.FileNames);
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

        private void Home_DragEnter(object sender, DragEventArgs e) => e.Effect = Program.DropEffect;

        private void Home_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data is null) return;
            if (e.Data.GetData(DataFormats.FileDrop, false) is not string[] draggedFiles) return;

            HandleNewFiles(draggedFiles);
            
            if (listView1.Items.Count != 0)
                dropBtn.Visible = dropBtn.Enabled = false;

            dropBtn.Text = "Click here to select file(s) or drag them here.";
        }

        private void HandleNewFiles(IList<string> paths) {
            Queue<string> invalidFiles = new();
            listView1.BeginUpdate();
            try {
                foreach (var path in paths) {
                    var fileInfo = FileSystem.CreateFileInfo(path);
                    if (DPArchive.IsValidSupportedArchive(fileInfo)) listView1.Items.Add(path);
                    else invalidFiles.Enqueue(path);
                }
            } catch (Exception ex) {
                MessageBox.Show($"An error occurred that may have prevented fully validating the new files:\n\n{ex.Message}", 
                    "Error handling new files", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } finally {
                listView1.EndUpdate();
            }
            if (invalidFiles.Count > 0)
            {
                var builder = new StringBuilder(50 * paths.Count);
                while (invalidFiles.Count != 0)
                    builder.AppendLine(" \u2022 " + invalidFiles.Dequeue());
                MessageBox.Show("Files that cannot be processed where removed from the list." +
                    "\nRemoved files:\n" + builder.ToString(), "Invalid files removed", MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
