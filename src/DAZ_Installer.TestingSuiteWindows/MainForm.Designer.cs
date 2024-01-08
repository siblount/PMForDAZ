namespace DAZ_Installer.TestingSuiteWindows
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            archiveToTestLbl = new Label();
            archiveTxtBox = new TextBox();
            browseArchiveBtn = new Button();
            openFileDialog1 = new OpenFileDialog();
            folderBrowserDialog1 = new FolderBrowserDialog();
            destPathTxtBox = new TextBox();
            browseDestBtn = new Button();
            treeView1 = new TreeView();
            treeViewMnuStrip = new ContextMenuStrip(components);
            copyNameToolStripMenuItem = new ToolStripMenuItem();
            copyFullPathToolStripMenuItem = new ToolStripMenuItem();
            markAsShouldProcessToolStripMenuItem = new ToolStripMenuItem();
            markAsShouldntProcessToolStripMenuItem = new ToolStripMenuItem();
            logOutputTxtBox = new RichTextBox();
            processBtn = new Button();
            toolTip1 = new ToolTip(components);
            destPathLbl = new Label();
            tempLbl = new Label();
            tempPathTxtBox = new TextBox();
            browseTempBtn = new Button();
            changeProcessBtn = new Button();
            peekBtn = new Button();
            determineBtn = new Button();
            extractBtn = new Button();
            peekRecursivelyBtn = new Button();
            tabControl1 = new TabControl();
            logsTab = new TabPage();
            filesExtractedTab = new TabPage();
            processedTxtBox = new RichTextBox();
            extractRecursivelyBtn = new Button();
            cancelBtn = new Button();
            deleteFilesChkBox = new CheckBox();
            clearLogsChkBox = new CheckBox();
            disableWarningChkBox = new CheckBox();
            saveBtn = new Button();
            label1 = new Label();
            saveBrowseBtn = new Button();
            saveTxtBox = new TextBox();
            autoSaveBtn = new CheckBox();
            treeViewMnuStrip.SuspendLayout();
            tabControl1.SuspendLayout();
            logsTab.SuspendLayout();
            filesExtractedTab.SuspendLayout();
            SuspendLayout();
            // 
            // archiveToTestLbl
            // 
            archiveToTestLbl.AutoSize = true;
            archiveToTestLbl.Location = new Point(12, 9);
            archiveToTestLbl.Name = "archiveToTestLbl";
            archiveToTestLbl.Size = new Size(152, 15);
            archiveToTestLbl.TabIndex = 0;
            archiveToTestLbl.Text = "Archive to test (rar, 7z, zip): ";
            // 
            // archiveTxtBox
            // 
            archiveTxtBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            archiveTxtBox.Location = new Point(12, 27);
            archiveTxtBox.Name = "archiveTxtBox";
            archiveTxtBox.Size = new Size(431, 23);
            archiveTxtBox.TabIndex = 1;
            // 
            // browseArchiveBtn
            // 
            browseArchiveBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseArchiveBtn.Location = new Point(449, 27);
            browseArchiveBtn.Name = "browseArchiveBtn";
            browseArchiveBtn.Size = new Size(31, 23);
            browseArchiveBtn.TabIndex = 2;
            browseArchiveBtn.Text = "...";
            browseArchiveBtn.UseVisualStyleBackColor = true;
            browseArchiveBtn.Click += browseArchiveBtn_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.FileName = "openFileDialog1";
            // 
            // folderBrowserDialog1
            // 
            folderBrowserDialog1.UseDescriptionForTitle = true;
            // 
            // destPathTxtBox
            // 
            destPathTxtBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            destPathTxtBox.Location = new Point(115, 58);
            destPathTxtBox.Name = "destPathTxtBox";
            destPathTxtBox.Size = new Size(328, 23);
            destPathTxtBox.TabIndex = 6;
            // 
            // browseDestBtn
            // 
            browseDestBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseDestBtn.Location = new Point(449, 59);
            browseDestBtn.Name = "browseDestBtn";
            browseDestBtn.Size = new Size(31, 23);
            browseDestBtn.TabIndex = 7;
            browseDestBtn.Text = "...";
            browseDestBtn.UseVisualStyleBackColor = true;
            browseDestBtn.Click += browseDestBtn_Click;
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            treeView1.ContextMenuStrip = treeViewMnuStrip;
            treeView1.Location = new Point(13, 169);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(468, 318);
            treeView1.TabIndex = 8;
            // 
            // treeViewMnuStrip
            // 
            treeViewMnuStrip.Items.AddRange(new ToolStripItem[] { copyNameToolStripMenuItem, copyFullPathToolStripMenuItem, markAsShouldProcessToolStripMenuItem, markAsShouldntProcessToolStripMenuItem });
            treeViewMnuStrip.Name = "treeViewMnuStrip";
            treeViewMnuStrip.Size = new Size(212, 92);
            treeViewMnuStrip.Opening += treeViewMnuStrip_Opening;
            // 
            // copyNameToolStripMenuItem
            // 
            copyNameToolStripMenuItem.Name = "copyNameToolStripMenuItem";
            copyNameToolStripMenuItem.Size = new Size(211, 22);
            copyNameToolStripMenuItem.Text = "Copy name";
            copyNameToolStripMenuItem.Click += copyNameToolStripMenuItem_Click;
            // 
            // copyFullPathToolStripMenuItem
            // 
            copyFullPathToolStripMenuItem.Name = "copyFullPathToolStripMenuItem";
            copyFullPathToolStripMenuItem.Size = new Size(211, 22);
            copyFullPathToolStripMenuItem.Text = "Copy full path";
            copyFullPathToolStripMenuItem.Click += copyFullPathToolStripMenuItem_Click;
            // 
            // markAsShouldProcessToolStripMenuItem
            // 
            markAsShouldProcessToolStripMenuItem.Enabled = false;
            markAsShouldProcessToolStripMenuItem.Name = "markAsShouldProcessToolStripMenuItem";
            markAsShouldProcessToolStripMenuItem.Size = new Size(211, 22);
            markAsShouldProcessToolStripMenuItem.Text = "Mark as should process";
            markAsShouldProcessToolStripMenuItem.Click += markAsShouldProcessToolStripMenuItem_Click;
            // 
            // markAsShouldntProcessToolStripMenuItem
            // 
            markAsShouldntProcessToolStripMenuItem.Name = "markAsShouldntProcessToolStripMenuItem";
            markAsShouldntProcessToolStripMenuItem.Size = new Size(211, 22);
            markAsShouldntProcessToolStripMenuItem.Text = "Mark as shouldn't process";
            markAsShouldntProcessToolStripMenuItem.Click += markAsShouldntProcessToolStripMenuItem_Click;
            // 
            // logOutputTxtBox
            // 
            logOutputTxtBox.Dock = DockStyle.Fill;
            logOutputTxtBox.Location = new Point(3, 3);
            logOutputTxtBox.Name = "logOutputTxtBox";
            logOutputTxtBox.Size = new Size(454, 123);
            logOutputTxtBox.TabIndex = 10;
            logOutputTxtBox.Text = "";
            // 
            // processBtn
            // 
            processBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            processBtn.Location = new Point(382, 686);
            processBtn.Name = "processBtn";
            processBtn.Size = new Size(98, 33);
            processBtn.TabIndex = 11;
            processBtn.Text = "Process Archive";
            processBtn.UseVisualStyleBackColor = true;
            processBtn.Click += processBtn_Click;
            // 
            // destPathLbl
            // 
            destPathLbl.AutoSize = true;
            destPathLbl.Location = new Point(13, 62);
            destPathLbl.Name = "destPathLbl";
            destPathLbl.Size = new Size(97, 15);
            destPathLbl.TabIndex = 5;
            destPathLbl.Text = "Destination Path:";
            // 
            // tempLbl
            // 
            tempLbl.AutoSize = true;
            tempLbl.Location = new Point(44, 91);
            tempLbl.Name = "tempLbl";
            tempLbl.Size = new Size(66, 15);
            tempLbl.TabIndex = 12;
            tempLbl.Text = "Temp Path:";
            // 
            // tempPathTxtBox
            // 
            tempPathTxtBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tempPathTxtBox.Location = new Point(115, 88);
            tempPathTxtBox.Name = "tempPathTxtBox";
            tempPathTxtBox.Size = new Size(328, 23);
            tempPathTxtBox.TabIndex = 13;
            // 
            // browseTempBtn
            // 
            browseTempBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            browseTempBtn.Location = new Point(449, 87);
            browseTempBtn.Name = "browseTempBtn";
            browseTempBtn.Size = new Size(31, 23);
            browseTempBtn.TabIndex = 14;
            browseTempBtn.Text = "...";
            browseTempBtn.UseVisualStyleBackColor = true;
            browseTempBtn.Click += browseTempBtn_Click;
            // 
            // changeProcessBtn
            // 
            changeProcessBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            changeProcessBtn.Location = new Point(13, 115);
            changeProcessBtn.Name = "changeProcessBtn";
            changeProcessBtn.Size = new Size(467, 23);
            changeProcessBtn.TabIndex = 15;
            changeProcessBtn.Text = "Change Process Settings";
            changeProcessBtn.UseVisualStyleBackColor = true;
            changeProcessBtn.Click += changeProcessBtn_Click;
            // 
            // peekBtn
            // 
            peekBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            peekBtn.Location = new Point(12, 686);
            peekBtn.Name = "peekBtn";
            peekBtn.Size = new Size(98, 33);
            peekBtn.TabIndex = 16;
            peekBtn.Text = "Peek Only";
            peekBtn.UseVisualStyleBackColor = true;
            peekBtn.Click += peekBtn_Click;
            // 
            // determineBtn
            // 
            determineBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            determineBtn.Location = new Point(116, 686);
            determineBtn.Name = "determineBtn";
            determineBtn.Size = new Size(155, 33);
            determineBtn.TabIndex = 17;
            determineBtn.Text = "Determine Destinations";
            determineBtn.UseVisualStyleBackColor = true;
            determineBtn.Click += determineBtn_Click;
            // 
            // extractBtn
            // 
            extractBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            extractBtn.Location = new Point(277, 686);
            extractBtn.Name = "extractBtn";
            extractBtn.Size = new Size(99, 33);
            extractBtn.TabIndex = 18;
            extractBtn.Text = "Extract Only";
            extractBtn.UseVisualStyleBackColor = true;
            extractBtn.Click += extractBtn_Click;
            // 
            // peekRecursivelyBtn
            // 
            peekRecursivelyBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            peekRecursivelyBtn.Location = new Point(13, 725);
            peekRecursivelyBtn.Name = "peekRecursivelyBtn";
            peekRecursivelyBtn.Size = new Size(97, 47);
            peekRecursivelyBtn.TabIndex = 19;
            peekRecursivelyBtn.Text = "Peek Recursively";
            peekRecursivelyBtn.UseVisualStyleBackColor = true;
            peekRecursivelyBtn.Click += peekRecursivelyBtn_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl1.Controls.Add(logsTab);
            tabControl1.Controls.Add(filesExtractedTab);
            tabControl1.Location = new Point(13, 493);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(468, 157);
            tabControl1.TabIndex = 20;
            // 
            // logsTab
            // 
            logsTab.Controls.Add(logOutputTxtBox);
            logsTab.Location = new Point(4, 24);
            logsTab.Name = "logsTab";
            logsTab.Padding = new Padding(3);
            logsTab.Size = new Size(460, 129);
            logsTab.TabIndex = 0;
            logsTab.Text = "Logs";
            logsTab.UseVisualStyleBackColor = true;
            // 
            // filesExtractedTab
            // 
            filesExtractedTab.Controls.Add(processedTxtBox);
            filesExtractedTab.Location = new Point(4, 24);
            filesExtractedTab.Name = "filesExtractedTab";
            filesExtractedTab.Size = new Size(460, 129);
            filesExtractedTab.TabIndex = 2;
            filesExtractedTab.Text = "Processed Files";
            filesExtractedTab.UseVisualStyleBackColor = true;
            // 
            // processedTxtBox
            // 
            processedTxtBox.Dock = DockStyle.Fill;
            processedTxtBox.Location = new Point(0, 0);
            processedTxtBox.Name = "processedTxtBox";
            processedTxtBox.Size = new Size(460, 129);
            processedTxtBox.TabIndex = 0;
            processedTxtBox.Text = "";
            // 
            // extractRecursivelyBtn
            // 
            extractRecursivelyBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            extractRecursivelyBtn.Location = new Point(116, 725);
            extractRecursivelyBtn.Name = "extractRecursivelyBtn";
            extractRecursivelyBtn.Size = new Size(155, 47);
            extractRecursivelyBtn.TabIndex = 21;
            extractRecursivelyBtn.Text = "Extract Recursively";
            extractRecursivelyBtn.UseVisualStyleBackColor = true;
            // 
            // cancelBtn
            // 
            cancelBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            cancelBtn.Enabled = false;
            cancelBtn.Location = new Point(382, 725);
            cancelBtn.Name = "cancelBtn";
            cancelBtn.Size = new Size(98, 47);
            cancelBtn.TabIndex = 22;
            cancelBtn.Text = "Cancel";
            cancelBtn.UseVisualStyleBackColor = true;
            // 
            // deleteFilesChkBox
            // 
            deleteFilesChkBox.AutoSize = true;
            deleteFilesChkBox.Checked = true;
            deleteFilesChkBox.CheckState = CheckState.Checked;
            deleteFilesChkBox.Location = new Point(13, 144);
            deleteFilesChkBox.Name = "deleteFilesChkBox";
            deleteFilesChkBox.Size = new Size(158, 19);
            deleteFilesChkBox.TabIndex = 23;
            deleteFilesChkBox.Text = "Delete files automatically";
            deleteFilesChkBox.UseVisualStyleBackColor = true;
            // 
            // clearLogsChkBox
            // 
            clearLogsChkBox.AutoSize = true;
            clearLogsChkBox.Checked = true;
            clearLogsChkBox.CheckState = CheckState.Checked;
            clearLogsChkBox.Location = new Point(184, 144);
            clearLogsChkBox.Name = "clearLogsChkBox";
            clearLogsChkBox.Size = new Size(153, 19);
            clearLogsChkBox.TabIndex = 24;
            clearLogsChkBox.Text = "Clear logs automatically";
            clearLogsChkBox.UseVisualStyleBackColor = true;
            // 
            // disableWarningChkBox
            // 
            disableWarningChkBox.AutoSize = true;
            disableWarningChkBox.Location = new Point(349, 144);
            disableWarningChkBox.Name = "disableWarningChkBox";
            disableWarningChkBox.Size = new Size(115, 19);
            disableWarningChkBox.TabIndex = 25;
            disableWarningChkBox.Text = "Disable warnings";
            disableWarningChkBox.UseVisualStyleBackColor = true;
            // 
            // saveBtn
            // 
            saveBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            saveBtn.Location = new Point(278, 725);
            saveBtn.Name = "saveBtn";
            saveBtn.Size = new Size(98, 47);
            saveBtn.TabIndex = 26;
            saveBtn.Text = "Save output";
            saveBtn.UseVisualStyleBackColor = true;
            saveBtn.Click += saveBtn_Click;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(93, 657);
            label1.Name = "label1";
            label1.Size = new Size(119, 15);
            label1.TabIndex = 27;
            label1.Text = "Save output location:";
            // 
            // saveBrowseBtn
            // 
            saveBrowseBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            saveBrowseBtn.Location = new Point(450, 652);
            saveBrowseBtn.Name = "saveBrowseBtn";
            saveBrowseBtn.Size = new Size(31, 23);
            saveBrowseBtn.TabIndex = 29;
            saveBrowseBtn.Text = "...";
            saveBrowseBtn.UseVisualStyleBackColor = true;
            saveBrowseBtn.Click += saveBrowseBtn_Click;
            // 
            // saveTxtBox
            // 
            saveTxtBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            saveTxtBox.Location = new Point(213, 652);
            saveTxtBox.Name = "saveTxtBox";
            saveTxtBox.Size = new Size(231, 23);
            saveTxtBox.TabIndex = 28;
            // 
            // autoSaveBtn
            // 
            autoSaveBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            autoSaveBtn.AutoSize = true;
            autoSaveBtn.Location = new Point(12, 656);
            autoSaveBtn.Name = "autoSaveBtn";
            autoSaveBtn.Size = new Size(75, 19);
            autoSaveBtn.TabIndex = 30;
            autoSaveBtn.Text = "Autosave";
            autoSaveBtn.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(492, 788);
            Controls.Add(autoSaveBtn);
            Controls.Add(saveBrowseBtn);
            Controls.Add(saveTxtBox);
            Controls.Add(label1);
            Controls.Add(saveBtn);
            Controls.Add(disableWarningChkBox);
            Controls.Add(clearLogsChkBox);
            Controls.Add(deleteFilesChkBox);
            Controls.Add(cancelBtn);
            Controls.Add(extractRecursivelyBtn);
            Controls.Add(tabControl1);
            Controls.Add(peekRecursivelyBtn);
            Controls.Add(extractBtn);
            Controls.Add(determineBtn);
            Controls.Add(peekBtn);
            Controls.Add(changeProcessBtn);
            Controls.Add(browseTempBtn);
            Controls.Add(tempPathTxtBox);
            Controls.Add(tempLbl);
            Controls.Add(processBtn);
            Controls.Add(treeView1);
            Controls.Add(browseDestBtn);
            Controls.Add(destPathTxtBox);
            Controls.Add(destPathLbl);
            Controls.Add(browseArchiveBtn);
            Controls.Add(archiveTxtBox);
            Controls.Add(archiveToTestLbl);
            MinimumSize = new Size(450, 600);
            Name = "MainForm";
            Text = "Testing Suite";
            Load += MainForm_Load;
            treeViewMnuStrip.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            logsTab.ResumeLayout(false);
            filesExtractedTab.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label archiveToTestLbl;
        private TextBox archiveTxtBox;
        private Button browseArchiveBtn;
        private OpenFileDialog openFileDialog1;
        private FolderBrowserDialog folderBrowserDialog1;
        private TextBox destPathTxtBox;
        private Button browseDestBtn;
        private TreeView treeView1;
        private Button processBtn;
        private ToolTip toolTip1;
        private Label destPathLbl;
        private Label tempLbl;
        private TextBox tempPathTxtBox;
        private Button browseTempBtn;
        private Button changeProcessBtn;
        private Button peekBtn;
        private Button determineBtn;
        private Button extractBtn;
        private Button peekRecursivelyBtn;
        private TabControl tabControl1;
        private TabPage logsTab;
        private TabPage filesExtractedTab;
        private RichTextBox processedTxtBox;
        internal RichTextBox logOutputTxtBox;
        private Button extractRecursivelyBtn;
        private Button cancelBtn;
        private CheckBox deleteFilesChkBox;
        private CheckBox clearLogsChkBox;
        private CheckBox disableWarningChkBox;
        private Button saveBtn;
        private Label label1;
        private Button saveBrowseBtn;
        private TextBox saveTxtBox;
        private CheckBox autoSaveBtn;
        private ContextMenuStrip treeViewMnuStrip;
        private ToolStripMenuItem copyNameToolStripMenuItem;
        private ToolStripMenuItem copyFullPathToolStripMenuItem;
        private ToolStripMenuItem markAsShouldProcessToolStripMenuItem;
        private ToolStripMenuItem markAsShouldntProcessToolStripMenuItem;
    }
}
