// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAZ_Installer.DP;

namespace DAZ_Installer
{
    public partial class Home : UserControl
    {
        public static Color initialHomePanelColor;
        public static Home HomePage;
        public Home()
        {
            InitializeComponent();
            HomePage = this;
        }

        private void homePage_Load(object sender, EventArgs e)
        {
            initialHomePanelColor = dragHerePanel.BackColor;
        }

        private void dragHerePanel_MouseEnter(object sender, EventArgs e)
        {
            dragHerePanel.BackColor = Color.FromArgb(255, initialHomePanelColor);
        }

        private void dragHerePanel_MouseLeave(object sender, EventArgs e)
        {
            dragHerePanel.BackColor = initialHomePanelColor;
        }

        private void dragHerePanel_Click(object sender, EventArgs e)
        {
            HandleOpenDialogue();
        }

        internal void button1_Click(object sender, EventArgs e)
        {
            var eControl = Extract.ExtractPage;
            // Clear everything from extract page.
            DPProgressCombo.RemoveAll();
            // Goto next page.
            MainForm.SwitchPage(eControl);

            var newJob = new DPExtractJob(listView1.Items); // Todo: make a list.
            var newTask = new Task(newJob.DoJob);

            newTask.Start();
            // Clear list and reset home.
            clearListBtn_Click(null, null);
        }

        private void dragHerePanel_DragEnter(object sender, DragEventArgs e)
        {
            dropText.Text = "Drop here!";
            e.Effect = DPCommon.dropEffect;
        }

        private void dragHerePanel_DragLeave(object sender, EventArgs e)
        {
            dropText.Text = "Click here to select file(s) or drag them here.";
        }

        private void dragHerePanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] tmp = (string[]) e.Data.GetData(DataFormats.FileDrop, false);
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
            }
            listView1.EndUpdate();
            if (listView1.Items.Count != 0 )
            {
                dragHerePanel.Visible = false;
                dragHerePanel.Enabled = false;  
            }
            dropText.Text = "Click here to select file(s) or drag them here.";
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (var i = listView1.SelectedItems.Count - 1; i >= 0; i--)
            {
                listView1.Items.Remove(listView1.SelectedItems[i]);
            }
            if (listView1.Items.Count == 0) controlDragPanel(true);
        }
        private void addMoreFilesBtn_Click(object sender, EventArgs e)
        {
            // Show dialogue.
            HandleOpenDialogue();
        }

        private void controlDragPanel(bool visible)
        {
            dragHerePanel.Visible = visible;
            dragHerePanel.Enabled = visible;
            if (visible)
            {
                dragHerePanel.BringToFront();
            }
        }

        private void HandleOpenDialogue()
        {
            var result = openFileDialog1.ShowDialog();
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

        private void addMoreItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HandleOpenDialogue();
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DPCommon.dropEffect;
        }
    }
}
