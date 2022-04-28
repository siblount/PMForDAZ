// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using DAZ_Installer.DP;
using DAZ_Installer.External;

namespace DAZ_Installer
{

    public partial class extractControl : UserControl
    {
        /// <summary> 
        /// Returns the integer of the first available slot in List<object>. Returns -1 if not available. 
        /// </summary>
        internal static object[] progressStack { get; set; } =  new object[4];
        internal static Control[][] controlComboStack { get; set; } =  new Control[3][];
        public static extractControl extractPage;
        public static Dictionary<ListViewItem, IDPWorkingFile> associatedListItems = new Dictionary<ListViewItem, IDPWorkingFile>(65536);
        public static Dictionary<TreeNode, IDPWorkingFile> associatedTreeNodes = new Dictionary<TreeNode, IDPWorkingFile>(65536);

        public void resetMainTable()
        {
            mainTableLayoutPanel.SuspendLayout();
            try
            {
                if (mainTableLayoutPanel.Controls.Count != 0)
                {
                    var arr = DPCommon.RecursivelyGetControls(mainTableLayoutPanel);
                    foreach (var control in arr)
                    {
                        control.Dispose();
                    }
                }
            }
            catch { }
            mainTableLayoutPanel.Controls.Clear();
            mainTableLayoutPanel.RowStyles.Clear();
            mainTableLayoutPanel.ColumnCount = 1;
            mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            mainTableLayoutPanel.RowCount = 1;
            updateMainTableRowSizing();
            mainTableLayoutPanel.ResumeLayout();
        }
        public void updateMainTableRowSizing()
        {
            // TO DO : Invoke.
            mainTableLayoutPanel.SuspendLayout();
            float percentageMultiplied = 1f / mainTableLayoutPanel.Controls.Count * 100f;
            for (var i = 0; i < mainTableLayoutPanel.RowStyles.Count; i++)
            {
                mainTableLayoutPanel.RowStyles[i] = new RowStyle(SizeType.Percent, percentageMultiplied);
            }
            
            mainTableLayoutPanel.ResumeLayout();
            mainTableLayoutPanel.Update();
        }

        public DialogResult DoPromptMessage(string message, string title, MessageBoxButtons buttons = MessageBoxButtons.YesNo)
        {
            
            return MessageBox.Show(message, title, buttons, MessageBoxIcon.Hand);
        }

        public extractControl()
        {
            InitializeComponent();
            extractPage = this;
        }

        private string getUniqueControlName(string baseName)
        {
            var tableControls = DPCommon.RecursivelyGetControls(mainTableLayoutPanel);
            string lastMatch = null;
            foreach (var control in tableControls)
            {
                if (control.Name.Contains(baseName))
                {
                    lastMatch = control.Name;
                }
            }
            if (lastMatch == null)
            {
                return baseName;
            }
            else
            {
                // Check to see if a number is appended at the end.
                var numsOnlyString = lastMatch[baseName.Length..];
                if (int.TryParse(numsOnlyString, out int suffixNum))
                {
                    return baseName + (suffixNum + 1);
                }
                else
                {
                    if (lastMatch == baseName)
                    {
                        return baseName + "1";
                    }
                    return baseName;
                }
            }
        }

        internal void AddToList(ref DPArchive archive)
        {
            fileListView.BeginUpdate();
            foreach (var content in archive.contents)
            {
                var item = fileListView.Items.Add($"{archive.fileName}\\{content.path}");
                content.associatedListItem = item;
                associatedListItems.Add(item, content);
            }
            fileListView.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            fileListView.EndUpdate();
        }

        private void ProcessChildNodes(DPFolder folder, ref TreeNode parentNode)
        {
            var fileName = Path.GetFileName(folder.path);
            TreeNode folder1 = null;
            // We don't need associations for folders.
            if (InvokeRequired)
            {
                folder1 = (TreeNode) Invoke(new Func<string, TreeNode>(parentNode.Nodes.Add), fileName);
                AddIcon(folder1, null);
            } else
            {
                folder1 = parentNode.Nodes.Add(fileName);
                AddIcon(folder1, null);
            }
            // Add the DPFiles.
            foreach (var file in folder.GetFiles())
            {
                fileName = Path.GetFileName(file.path);
                // TO DO: Add condition if file is a DPArchive & extract == true
                if (InvokeRequired)
                {
                    var node = (TreeNode) Invoke(new Func<string, TreeNode>(folder1.Nodes.Add), fileName);
                    file.associatedTreeNode = node;
                    AddIcon(node, file.ext);
                }
                else
                {
                    var node = folder1.Nodes.Add(fileName);
                    file.associatedTreeNode = node;
                    AddIcon(node, file.ext);
                }
            }
            foreach (var subfolder in folder.subfolders)
            {
                ProcessChildNodes(subfolder, ref folder1);
            }
        }

        internal void AddToHierachy(ref DPArchive workingArchive)
        {
            fileHierachyTree.BeginUpdate();
            // Add root node for DPArchive.
            var fileName = workingArchive.hierachyName;
            TreeNode rootNode = null;
            if (InvokeRequired)
            {
                var func = new Func<string, TreeNode>(fileHierachyTree.Nodes.Add);
                rootNode = (TreeNode) Invoke(func,fileName);
                workingArchive.associatedTreeNode = rootNode;
                AddIcon(rootNode, workingArchive.ext);

            } else
            {
                rootNode = fileHierachyTree.Nodes.Add(fileName);
                workingArchive.associatedTreeNode = rootNode;
                AddIcon(rootNode, workingArchive.ext);
            }


            // Add any files that aren't in any folder.
            foreach (var file in workingArchive.rootContents)
            {
                fileName = Path.GetFileName(file.path);
                if (InvokeRequired)
                {
                    var node = (TreeNode) Invoke(new Func<string, TreeNode>(rootNode.Nodes.Add), fileName);
                    file.associatedTreeNode = node;
                    AddIcon(node, file.ext);
                } else
                {
                    var node = rootNode.Nodes.Add(fileName);
                    file.associatedTreeNode = node;
                    AddIcon(node, file.ext);
                }
            }

            // Recursively add files & folder within each folder.
            foreach (var folder in workingArchive.rootFolders)
            {
                ProcessChildNodes(folder, ref rootNode);
            }
            fileHierachyTree.ExpandAll();
            fileHierachyTree.EndUpdate();
        }

        // Object to satisfy Invoke.
        private object AddIcon(TreeNode node, string ext)
        {
            if (InvokeRequired)
            {
                return Invoke(new Func<TreeNode,string,object>(AddIcon), node, ext);
            }
            if (ext == null || ext == "")
            {
                node.StateImageIndex = 0;
            }
            else if (ext.Contains("zip") || ext.Contains("7z"))
            {
                node.StateImageIndex = 2;
            } else if (ext.Contains("rar"))
            {
               node.StateImageIndex = 1;
            }
            return null;
        }
        private void extractControl_Load(object sender, EventArgs e)
        {
            //var DSXParser = new DSXParser(@"D:\3D\DAZ3D shit\DAZ IM Manager Downloads\Manifest.dsx");
            //DPSettings.Initalize();
        }

        

        public void ResetExtractPage()
        {
            // Later show nothing to extract panel.
            resetMainTable();
            fileListView.Items.Clear();
            fileHierachyTree.Nodes.Clear();
            associatedListItems.Clear();
            associatedTreeNodes.Clear();
            progressStack = new object[3];
            controlComboStack = new Control[3][];
        }
        public string ShowFileDialog(string filter, string defaultExt, string defaultLocation = null)
        {
            if (InvokeRequired)
            {
                return (string) Invoke(new Func<string, string, string, string>(ShowFileDialog),filter, defaultExt, defaultLocation);
            }
            openFileDialog1.Filter = filter;
            openFileDialog1.DefaultExt = defaultExt;
            if (defaultLocation != null)
            {
                openFileDialog1.InitialDirectory = defaultLocation;
            }
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                return openFileDialog1.FileName;
            }
            return null;
        }
        /// <summary>
        /// Creates a progress bar and adds it to the table. [0] - TableLayout, [1] - label, [2] - ProgressBar
        /// </summary>
        /// <returns>An array of controls</returns>
        public Control[] createProgressCombo()
        {
            DPCommon.WriteToLog(InvokeRequired);
            mainTableLayoutPanel.SuspendLayout();
            if (mainTableLayoutPanel.Controls.Count != 0)
            {
                mainTableLayoutPanel.RowCount += 1;
                mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            // Create a table layout of 2 rows, 1 column, autosize.
            TableLayoutPanel workingPanel = new TableLayoutPanel();
            mainTableLayoutPanel.Controls.Add(workingPanel);
            workingPanel.SuspendLayout();
            workingPanel.ColumnCount = 1;
            workingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            //this.mainTableLayoutPanel.Controls.Add(this.innerTableLayoutPanel1, 0, 0);
            workingPanel.Dock = DockStyle.Fill;
            workingPanel.Name = getUniqueControlName("innerTableLayoutPanel");
            workingPanel.RowCount = 2;
            workingPanel.RowStyles.Add(new ColumnStyle(SizeType.AutoSize));
            workingPanel.RowStyles.Add(new ColumnStyle(SizeType.AutoSize));


            // Create Label
            Label workingLabel = new Label();
            workingLabel.Text = "Processing ...";
            workingLabel.Dock = DockStyle.Fill;
            workingLabel.AutoEllipsis = true;
            workingLabel.TextAlign = ContentAlignment.BottomLeft;
            workingLabel.MinimumSize = new Size(0, 25);
            workingLabel.Name = getUniqueControlName("label");
            workingPanel.Controls.Add(workingLabel, 0, 0);

            // Create new progress bar.
            ProgressBar progressBar = new ProgressBar();
            progressBar.Value = 50;
            progressBar.Dock = DockStyle.Fill;
            progressBar.Name = getUniqueControlName("progressBar");
            progressBar.MinimumSize = new Size(0, 18);
            workingPanel.Controls.Add(progressBar, 0, 1);
            workingPanel.ResumeLayout();
            mainTableLayoutPanel.ResumeLayout(true);
            updateMainTableRowSizing();

            // Return a list of controls in order.
            // [0] - TableLayout, [1] - Label, [2] - ProgressBar
            return new Control[] { workingPanel, workingLabel, progressBar };
        }
        /// <summary>
        /// Creates a progress bar and adds it to the table. ProgressBar is marquee style. [0] - TableLayout, [1] - label, [2] - ProgressBar
        /// </summary>
        /// <returns>An array of controls</returns>
        public Control[] createProgressComboMarquee()
        {
            DPCommon.WriteToLog(InvokeRequired);
            mainTableLayoutPanel.SuspendLayout();
            if (mainTableLayoutPanel.Controls.Count != 0)
            {
                mainTableLayoutPanel.RowCount += 1;
                mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            // Create a table layout of 2 rows, 1 column, autosize.
            TableLayoutPanel workingPanel = new TableLayoutPanel();
            mainTableLayoutPanel.Controls.Add(workingPanel);
            workingPanel.SuspendLayout();
            workingPanel.ColumnCount = 1;
            workingPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            //this.mainTableLayoutPanel.Controls.Add(this.innerTableLayoutPanel1, 0, 0);
            workingPanel.Dock = DockStyle.Fill;
            workingPanel.Name = getUniqueControlName("innerTableLayoutPanel");
            workingPanel.RowCount = 2;
            workingPanel.RowStyles.Add(new ColumnStyle(SizeType.AutoSize));
            workingPanel.RowStyles.Add(new ColumnStyle(SizeType.AutoSize));


            // Create Label
            Label workingLabel = new Label();
            workingLabel.Text = "Processing ...";
            workingLabel.Dock = DockStyle.Fill;
            workingLabel.AutoEllipsis = true;
            workingLabel.TextAlign = ContentAlignment.BottomLeft;
            workingLabel.MinimumSize = new Size(0, 25);
            workingLabel.Name = getUniqueControlName("label");
            workingPanel.Controls.Add(workingLabel, 0, 0);

            // Create new progress bar.
            ProgressBar progressBar = new ProgressBar();
            progressBar.Value = 10;
            progressBar.Dock = DockStyle.Fill;
            progressBar.Name = getUniqueControlName("progressBar");
            progressBar.MarqueeAnimationSpeed /= 5;
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.MinimumSize = new Size(0, 18);
            workingPanel.Controls.Add(progressBar, 0, 1);
            workingPanel.ResumeLayout();
            mainTableLayoutPanel.ResumeLayout(true);
            updateMainTableRowSizing();

            // Return a list of controls in order.
            // [0] - TableLayout, [1] - Label, [2] - ProgressBar
            return new Control[] { workingPanel, workingLabel, progressBar };
        }

        private void mainProcLbl_Click(object sender, EventArgs e)
        {
            createProgressCombo();
        }

        #region Handle DPPrecssor Events
        public void HandlePasswordProtected(RAR sender, PasswordRequiredEventArgs e)
        {
            // Create enter password dialog.
            var passDlg = (PasswordInput) Invoke(new Func<PasswordInput>(CreatePasswordInput));
            passDlg.archiveName = Path.GetFileName(sender.ArchivePathName);
            if (sender.CurrentFile == null)
            {
                passDlg.message = $"{Path.GetFileName(DPProcessor.workingArchive.path)} is encrypted. Please enter password to decrypt archive.";
            } else
            {
                if ((sender.arcData.Flags & 0x0080) != 0)
                {
                    passDlg.message = "Password was incorrect. Please re-enter password to decrypt archive.";
                }
            }
            passDlg.ShowDialog();


            if (passDlg.password != null)
            {
                e.Password = passDlg.password;
                e.ContinueOperation = true;
            }
            else
            {
                e.ContinueOperation = false;
                DPProcessor.workingArchive.cancelledOperation = true;
            }
        }

        public PasswordInput CreatePasswordInput()
        {
            return new PasswordInput();
        }


        public void HandleMissingVolume(RAR sender, MissingVolumeEventArgs e)
        {
            // Ask user for missing volume.
            var result = DoPromptMessage($"{sender.CurrentFile.FileName} is missing volume : {e.VolumeName}. Do you know where this file is? ", "Missing volume", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                string fileName = ShowFileDialog("RAR files (*.rar)|*.rar", "rar");
                if (fileName != null)
                {
                    e.VolumeName = fileName;
                    e.ContinueOperation = true;
                }
                else
                {
                    e.ContinueOperation = false;
                }
            }
            else
            {
                e.ContinueOperation = false;
            }
        }

        public void DeleteProgressionCombo(Control[] combo)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Control[]>(DeleteProgressionCombo), new object[] { combo });
                return;
            }
            // Check if combo box is in array.
            var index = ArrayHelper.GetIndex(controlComboStack, combo);
            if (index != -1)
            {
                if (progressStack[index] != null) progressStack[index] = null;
                // Call main table that we are going to make some big changes.
                mainTableLayoutPanel.SuspendLayout();
                mainTableLayoutPanel.Controls.Remove(controlComboStack[index][0]);
                // Reset row count. 
                if (mainTableLayoutPanel.Controls.Count == 0) mainTableLayoutPanel.RowCount = 1;
                else mainTableLayoutPanel.RowCount = mainTableLayoutPanel.Controls.Count;

                mainTableLayoutPanel.RowStyles.Clear();
                for (var i = 0; i < mainTableLayoutPanel.RowCount; i++) mainTableLayoutPanel.RowStyles.Add(new RowStyle());
                mainTableLayoutPanel.ResumeLayout(false);
                updateMainTableRowSizing();

                //Dispose.
                foreach (var control in controlComboStack[index])
                {
                    control.Dispose();
                }

                // OH GARBAGE COLLECTOR!!!
                //var generation = GC.GetGeneration(controlComboStack[index]);
                //GC.Collect(generation, GCCollectionMode.Forced, true, false);

            } else
            {
                throw new ArgumentNullException("Combo was not found in array.");
            }
        }

        public void HandleProgressionRAR(RAR sender, ExtractionProgressEventArgs e)
        {
            Control[] progressCombo;
            Label progressLabel;
            ProgressBar progressBar;
            var index = ArrayHelper.GetIndex(progressStack, sender);

            // If already exists, update controls.
            if (index != -1)
            {
                progressCombo = controlComboStack[index];
                progressLabel = (Label)progressCombo[1];
                progressBar = (ProgressBar)progressCombo[2];
            }
            else
            {
                DPCommon.WriteToLog("Creating new progression combo.");
                progressCombo = (Control[]) Invoke(new Func<Control[]>(createProgressCombo));
                progressLabel = (Label)progressCombo[1];
                progressBar = (ProgressBar)progressCombo[2];

                var openSlotIndex = ArrayHelper.GetNextOpenSlot(progressStack);
                if (openSlotIndex == -1)
                {
                    throw new IndexOutOfRangeException("Attempted to add more than 4 to array.");
                }
                else
                {
                    progressStack[openSlotIndex] = sender;
                    controlComboStack[openSlotIndex] = progressCombo;
                }
                DPProcessor.workingArchive.progressCombo = progressCombo;
            }
            var progress = (int)Math.Floor(e.PercentComplete);

            progressLabel.Text = $"Extracting {e.FileName}..({progress}%)";
            mainProcLbl.Text = progressLabel.Text;
            progressBar.Value = progress;
        }
        
        public void HandleProgressionZIP(ref ZipArchive sender, int i, int max)
        {
            var percentComplete = (float)i / max;
            Control[] progressCombo;
            Label progressLabel;
            ProgressBar progressBar;
            var index = ArrayHelper.GetIndex(progressStack, sender);

            // If already exists, update controls.
            if (index != -1)
            {
                progressCombo = controlComboStack[index];
                progressLabel = (Label)progressCombo[1];
                progressBar = (ProgressBar)progressCombo[2];
            }
            else
            {
                DPCommon.WriteToLog("Creating new progression combo.");
                progressCombo = (Control[])Invoke(new Func<Control[]>(createProgressCombo));
                progressLabel = (Label)progressCombo[1];
                progressBar = (ProgressBar)progressCombo[2];

                var openSlotIndex = ArrayHelper.GetNextOpenSlot(progressStack);
                if (openSlotIndex == -1)
                {
                    throw new IndexOutOfRangeException("Attempted to add more than 3 to array.");
                }
                else
                {
                    progressStack[openSlotIndex] = sender;
                    controlComboStack[openSlotIndex] = progressCombo;
                }
                DPProcessor.workingArchive.progressCombo = progressCombo;
            }
            var progress = (int)Math.Floor(percentComplete * 100);

            progressLabel.Text = $"Extracting files...({progress}%)";
            mainProcLbl.Text = progressLabel.Text;
            progressBar.Value = progress;
        }
        #endregion

        #region Context Strip Events
        private void selectInHierachyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Get the associated file with listviewitem.
            associatedListItems.TryGetValue(fileListView.SelectedItems[0], out IDPWorkingFile file);
            if (file != null)
            {
                fileHierachyTree.SelectedNode = file.associatedTreeNode;
            }
            // Switch tab.
            tabControl1.SelectTab(fileHierachyPage);

        }

        private void fileListContextStrip_Opening(object sender, CancelEventArgs e)
        {
            var filesSelected = fileListView.SelectedItems.Count != 0;
            inspectFileListMenuItem.Visible = filesSelected;
            openInExplorerToolStripMenuItem.Visible = filesSelected;
            selectInHierachyToolStripMenuItem.Visible = filesSelected;
            noFilesSelectedToolStripMenuItem.Visible = !filesSelected;
        }

        public void OpenFileInExplorer(string path)
        {
            Process.Start(@"explorer.exe", $"/select, \"{path}\"");
        }
        #endregion

        private void queueList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }

}


