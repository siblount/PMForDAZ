
namespace DAZ_Installer
{
    partial class ProductRecordForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductRecordForm));
            this.thumbnailBox = new System.Windows.Forms.PictureBox();
            this.thumbnailStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyImagePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInFileExplorerToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.removeImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browseImageBtn = new System.Windows.Forms.Button();
            this.contentFoldersList = new System.Windows.Forms.ListView();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.genericStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.copyPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInFileExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filesExtractedList = new System.Windows.Forms.ListView();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.dateExtractedLbl = new System.Windows.Forms.Label();
            this.erroredFilesList = new System.Windows.Forms.ListView();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.errorMessagesList = new System.Windows.Forms.ListView();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.destinationPathLbl = new System.Windows.Forms.Label();
            this.tagsView = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.tagsStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.editTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteNewTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applyChangesBtn = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tagsPage = new System.Windows.Forms.TabPage();
            this.fileHierachyPage = new System.Windows.Forms.TabPage();
            this.fileTreeView = new System.Windows.Forms.TreeView();
            this.contentFoldersPage = new System.Windows.Forms.TabPage();
            this.fileListPage = new System.Windows.Forms.TabPage();
            this.erroredFilesPage = new System.Windows.Forms.TabPage();
            this.errorMessagesPage = new System.Windows.Forms.TabPage();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.deleteRecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteProductToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.productNameTxtBox = new System.Windows.Forms.TextBox();
            this.authorLbl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.thumbnailBox)).BeginInit();
            this.thumbnailStrip.SuspendLayout();
            this.genericStrip.SuspendLayout();
            this.tagsStrip.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tagsPage.SuspendLayout();
            this.fileHierachyPage.SuspendLayout();
            this.contentFoldersPage.SuspendLayout();
            this.fileListPage.SuspendLayout();
            this.erroredFilesPage.SuspendLayout();
            this.errorMessagesPage.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // thumbnailBox
            // 
            this.thumbnailBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.thumbnailBox.ContextMenuStrip = this.thumbnailStrip;
            this.thumbnailBox.Image = global::DAZ_Installer.Properties.Resources.NoImageFound;
            this.thumbnailBox.Location = new System.Drawing.Point(515, 25);
            this.thumbnailBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.thumbnailBox.Name = "thumbnailBox";
            this.thumbnailBox.Size = new System.Drawing.Size(127, 114);
            this.thumbnailBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.thumbnailBox.TabIndex = 2;
            this.thumbnailBox.TabStop = false;
            // 
            // thumbnailStrip
            // 
            this.thumbnailStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyImageToolStripMenuItem,
            this.copyImagePathToolStripMenuItem,
            this.openInFileExplorerToolStripMenuItem1,
            this.removeImageToolStripMenuItem});
            this.thumbnailStrip.Name = "thumbnailStrip";
            this.thumbnailStrip.Size = new System.Drawing.Size(182, 92);
            // 
            // copyImageToolStripMenuItem
            // 
            this.copyImageToolStripMenuItem.Name = "copyImageToolStripMenuItem";
            this.copyImageToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.copyImageToolStripMenuItem.Text = "Copy image";
            this.copyImageToolStripMenuItem.Click += new System.EventHandler(this.copyImageToolStripMenuItem_Click);
            // 
            // copyImagePathToolStripMenuItem
            // 
            this.copyImagePathToolStripMenuItem.Name = "copyImagePathToolStripMenuItem";
            this.copyImagePathToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.copyImagePathToolStripMenuItem.Text = "Copy image path";
            this.copyImagePathToolStripMenuItem.Click += new System.EventHandler(this.copyImagePathToolStripMenuItem_Click);
            // 
            // openInFileExplorerToolStripMenuItem1
            // 
            this.openInFileExplorerToolStripMenuItem1.Name = "openInFileExplorerToolStripMenuItem1";
            this.openInFileExplorerToolStripMenuItem1.Size = new System.Drawing.Size(181, 22);
            this.openInFileExplorerToolStripMenuItem1.Text = "Open in file explorer";
            this.openInFileExplorerToolStripMenuItem1.Click += new System.EventHandler(this.openInFileExplorerToolStripMenuItem1_Click);
            // 
            // removeImageToolStripMenuItem
            // 
            this.removeImageToolStripMenuItem.Name = "removeImageToolStripMenuItem";
            this.removeImageToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.removeImageToolStripMenuItem.Text = "Remove image";
            this.removeImageToolStripMenuItem.Click += new System.EventHandler(this.removeImageToolStripMenuItem_Click);
            // 
            // browseImageBtn
            // 
            this.browseImageBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseImageBtn.Location = new System.Drawing.Point(477, 142);
            this.browseImageBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.browseImageBtn.Name = "browseImageBtn";
            this.browseImageBtn.Size = new System.Drawing.Size(197, 22);
            this.browseImageBtn.TabIndex = 3;
            this.browseImageBtn.Text = "Browse Image";
            this.browseImageBtn.UseVisualStyleBackColor = true;
            this.browseImageBtn.Click += new System.EventHandler(this.browseImageBtn_Click);
            // 
            // contentFoldersList
            // 
            this.contentFoldersList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
            this.contentFoldersList.ContextMenuStrip = this.genericStrip;
            this.contentFoldersList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.contentFoldersList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.contentFoldersList.Location = new System.Drawing.Point(0, 0);
            this.contentFoldersList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.contentFoldersList.Name = "contentFoldersList";
            this.contentFoldersList.Size = new System.Drawing.Size(660, 106);
            this.contentFoldersList.TabIndex = 7;
            this.contentFoldersList.UseCompatibleStateImageBehavior = false;
            this.contentFoldersList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Width = 320;
            // 
            // genericStrip
            // 
            this.genericStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem1,
            this.copyPathToolStripMenuItem,
            this.openInFileExplorerToolStripMenuItem});
            this.genericStrip.Name = "genericStrip";
            this.genericStrip.Size = new System.Drawing.Size(182, 70);
            this.genericStrip.Opening += new System.ComponentModel.CancelEventHandler(this.genericStrip_Opening);
            // 
            // copyToolStripMenuItem1
            // 
            this.copyToolStripMenuItem1.Enabled = false;
            this.copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            this.copyToolStripMenuItem1.Size = new System.Drawing.Size(181, 22);
            this.copyToolStripMenuItem1.Text = "Copy";
            this.copyToolStripMenuItem1.Click += new System.EventHandler(this.copyToolStripMenuItem1_Click);
            // 
            // copyPathToolStripMenuItem
            // 
            this.copyPathToolStripMenuItem.Enabled = false;
            this.copyPathToolStripMenuItem.Name = "copyPathToolStripMenuItem";
            this.copyPathToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.copyPathToolStripMenuItem.Text = "Copy path";
            this.copyPathToolStripMenuItem.Click += new System.EventHandler(this.copyPathToolStripMenuItem_Click);
            // 
            // openInFileExplorerToolStripMenuItem
            // 
            this.openInFileExplorerToolStripMenuItem.Enabled = false;
            this.openInFileExplorerToolStripMenuItem.Name = "openInFileExplorerToolStripMenuItem";
            this.openInFileExplorerToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.openInFileExplorerToolStripMenuItem.Text = "Open in file explorer";
            this.openInFileExplorerToolStripMenuItem.Click += new System.EventHandler(this.openInFileExplorerToolStripMenuItem_Click);
            // 
            // filesExtractedList
            // 
            this.filesExtractedList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.filesExtractedList.ContextMenuStrip = this.genericStrip;
            this.filesExtractedList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.filesExtractedList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.filesExtractedList.Location = new System.Drawing.Point(0, 0);
            this.filesExtractedList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.filesExtractedList.Name = "filesExtractedList";
            this.filesExtractedList.Size = new System.Drawing.Size(660, 106);
            this.filesExtractedList.TabIndex = 9;
            this.filesExtractedList.UseCompatibleStateImageBehavior = false;
            this.filesExtractedList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 600;
            // 
            // dateExtractedLbl
            // 
            this.dateExtractedLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dateExtractedLbl.AutoEllipsis = true;
            this.dateExtractedLbl.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.dateExtractedLbl.Location = new System.Drawing.Point(14, 71);
            this.dateExtractedLbl.Name = "dateExtractedLbl";
            this.dateExtractedLbl.Size = new System.Drawing.Size(458, 15);
            this.dateExtractedLbl.TabIndex = 10;
            this.dateExtractedLbl.Text = "Date Extracted: ";
            // 
            // erroredFilesList
            // 
            this.erroredFilesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3});
            this.erroredFilesList.ContextMenuStrip = this.genericStrip;
            this.erroredFilesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.erroredFilesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.erroredFilesList.Location = new System.Drawing.Point(0, 0);
            this.erroredFilesList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.erroredFilesList.Name = "erroredFilesList";
            this.erroredFilesList.Size = new System.Drawing.Size(660, 106);
            this.erroredFilesList.TabIndex = 12;
            this.erroredFilesList.UseCompatibleStateImageBehavior = false;
            this.erroredFilesList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Width = 600;
            // 
            // errorMessagesList
            // 
            this.errorMessagesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5});
            this.errorMessagesList.ContextMenuStrip = this.genericStrip;
            this.errorMessagesList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorMessagesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.errorMessagesList.Location = new System.Drawing.Point(0, 0);
            this.errorMessagesList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.errorMessagesList.Name = "errorMessagesList";
            this.errorMessagesList.Size = new System.Drawing.Size(660, 106);
            this.errorMessagesList.TabIndex = 16;
            this.errorMessagesList.UseCompatibleStateImageBehavior = false;
            this.errorMessagesList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Width = 600;
            // 
            // destinationPathLbl
            // 
            this.destinationPathLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.destinationPathLbl.AutoEllipsis = true;
            this.destinationPathLbl.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.destinationPathLbl.Location = new System.Drawing.Point(14, 86);
            this.destinationPathLbl.Name = "destinationPathLbl";
            this.destinationPathLbl.Size = new System.Drawing.Size(458, 78);
            this.destinationPathLbl.TabIndex = 14;
            this.destinationPathLbl.Text = "Destination Path: ";
            // 
            // tagsView
            // 
            this.tagsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.tagsView.ContextMenuStrip = this.tagsStrip;
            this.tagsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tagsView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.tagsView.Location = new System.Drawing.Point(3, 2);
            this.tagsView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tagsView.Name = "tagsView";
            this.tagsView.Size = new System.Drawing.Size(654, 102);
            this.tagsView.TabIndex = 13;
            this.tagsView.UseCompatibleStateImageBehavior = false;
            this.tagsView.View = System.Windows.Forms.View.Details;
            this.tagsView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tagsView_KeyUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 280;
            // 
            // tagsStrip
            // 
            this.tagsStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editTagsToolStripMenuItem,
            this.removeTagToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteNewTagsToolStripMenuItem,
            this.replaceToolStripMenuItem});
            this.tagsStrip.Name = "tagsStrip";
            this.tagsStrip.Size = new System.Drawing.Size(161, 114);
            this.tagsStrip.Opening += new System.ComponentModel.CancelEventHandler(this.tagsStrip_Opening);
            // 
            // editTagsToolStripMenuItem
            // 
            this.editTagsToolStripMenuItem.Name = "editTagsToolStripMenuItem";
            this.editTagsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.editTagsToolStripMenuItem.Text = "Edit tags";
            this.editTagsToolStripMenuItem.Click += new System.EventHandler(this.editTagsToolStripMenuItem_Click);
            // 
            // removeTagToolStripMenuItem
            // 
            this.removeTagToolStripMenuItem.Enabled = false;
            this.removeTagToolStripMenuItem.Name = "removeTagToolStripMenuItem";
            this.removeTagToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.removeTagToolStripMenuItem.Text = "Remove";
            this.removeTagToolStripMenuItem.Click += new System.EventHandler(this.removeTagToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Enabled = false;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteNewTagsToolStripMenuItem
            // 
            this.pasteNewTagsToolStripMenuItem.Enabled = false;
            this.pasteNewTagsToolStripMenuItem.Name = "pasteNewTagsToolStripMenuItem";
            this.pasteNewTagsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.pasteNewTagsToolStripMenuItem.Text = "Paste new tag(s)";
            this.pasteNewTagsToolStripMenuItem.Click += new System.EventHandler(this.pasteNewTagsToolStripMenuItem_Click);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.Enabled = false;
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.replaceToolStripMenuItem.Text = "Replace";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.replaceToolStripMenuItem_Click);
            // 
            // applyChangesBtn
            // 
            this.applyChangesBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyChangesBtn.Location = new System.Drawing.Point(562, 310);
            this.applyChangesBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.applyChangesBtn.Name = "applyChangesBtn";
            this.applyChangesBtn.Size = new System.Drawing.Size(112, 22);
            this.applyChangesBtn.TabIndex = 14;
            this.applyChangesBtn.Text = "Apply Changes";
            this.applyChangesBtn.UseVisualStyleBackColor = true;
            this.applyChangesBtn.Click += new System.EventHandler(this.applyChangesBtn_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tagsPage);
            this.tabControl1.Controls.Add(this.fileHierachyPage);
            this.tabControl1.Controls.Add(this.contentFoldersPage);
            this.tabControl1.Controls.Add(this.fileListPage);
            this.tabControl1.Controls.Add(this.erroredFilesPage);
            this.tabControl1.Controls.Add(this.errorMessagesPage);
            this.tabControl1.Location = new System.Drawing.Point(10, 169);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(668, 134);
            this.tabControl1.TabIndex = 17;
            // 
            // tagsPage
            // 
            this.tagsPage.Controls.Add(this.tagsView);
            this.tagsPage.Location = new System.Drawing.Point(4, 24);
            this.tagsPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tagsPage.Name = "tagsPage";
            this.tagsPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tagsPage.Size = new System.Drawing.Size(660, 106);
            this.tagsPage.TabIndex = 0;
            this.tagsPage.Text = "Tags";
            this.tagsPage.UseVisualStyleBackColor = true;
            // 
            // fileHierachyPage
            // 
            this.fileHierachyPage.Controls.Add(this.fileTreeView);
            this.fileHierachyPage.Location = new System.Drawing.Point(4, 24);
            this.fileHierachyPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fileHierachyPage.Name = "fileHierachyPage";
            this.fileHierachyPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fileHierachyPage.Size = new System.Drawing.Size(660, 106);
            this.fileHierachyPage.TabIndex = 1;
            this.fileHierachyPage.Text = "File Hierachy";
            this.fileHierachyPage.UseVisualStyleBackColor = true;
            // 
            // fileTreeView
            // 
            this.fileTreeView.ContextMenuStrip = this.genericStrip;
            this.fileTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTreeView.Location = new System.Drawing.Point(3, 2);
            this.fileTreeView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fileTreeView.Name = "fileTreeView";
            this.fileTreeView.Size = new System.Drawing.Size(654, 102);
            this.fileTreeView.TabIndex = 0;
            // 
            // contentFoldersPage
            // 
            this.contentFoldersPage.Controls.Add(this.contentFoldersList);
            this.contentFoldersPage.Location = new System.Drawing.Point(4, 24);
            this.contentFoldersPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.contentFoldersPage.Name = "contentFoldersPage";
            this.contentFoldersPage.Size = new System.Drawing.Size(660, 106);
            this.contentFoldersPage.TabIndex = 4;
            this.contentFoldersPage.Text = "Content Folders";
            this.contentFoldersPage.UseVisualStyleBackColor = true;
            // 
            // fileListPage
            // 
            this.fileListPage.Controls.Add(this.filesExtractedList);
            this.fileListPage.Location = new System.Drawing.Point(4, 24);
            this.fileListPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fileListPage.Name = "fileListPage";
            this.fileListPage.Size = new System.Drawing.Size(660, 106);
            this.fileListPage.TabIndex = 5;
            this.fileListPage.Text = "File List";
            this.fileListPage.UseVisualStyleBackColor = true;
            // 
            // erroredFilesPage
            // 
            this.erroredFilesPage.Controls.Add(this.erroredFilesList);
            this.erroredFilesPage.Location = new System.Drawing.Point(4, 24);
            this.erroredFilesPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.erroredFilesPage.Name = "erroredFilesPage";
            this.erroredFilesPage.Size = new System.Drawing.Size(660, 106);
            this.erroredFilesPage.TabIndex = 2;
            this.erroredFilesPage.Text = "Errored Files";
            this.erroredFilesPage.UseVisualStyleBackColor = true;
            // 
            // errorMessagesPage
            // 
            this.errorMessagesPage.Controls.Add(this.errorMessagesList);
            this.errorMessagesPage.Location = new System.Drawing.Point(4, 24);
            this.errorMessagesPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.errorMessagesPage.Name = "errorMessagesPage";
            this.errorMessagesPage.Size = new System.Drawing.Size(660, 106);
            this.errorMessagesPage.TabIndex = 3;
            this.errorMessagesPage.Text = "Error Messages";
            this.errorMessagesPage.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            this.toolStrip1.AllowMerge = false;
            this.toolStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(684, 25);
            this.toolStrip1.TabIndex = 18;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteRecordToolStripMenuItem,
            this.deleteProductToolStripMenuItem});
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(53, 22);
            this.toolStripButton1.Text = "Delete";
            // 
            // deleteRecordToolStripMenuItem
            // 
            this.deleteRecordToolStripMenuItem.Name = "deleteRecordToolStripMenuItem";
            this.deleteRecordToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteRecordToolStripMenuItem.Text = "Delete record";
            this.deleteRecordToolStripMenuItem.Click += new System.EventHandler(this.deleteRecordToolStripMenuItem_Click);
            // 
            // deleteProductToolStripMenuItem
            // 
            this.deleteProductToolStripMenuItem.Name = "deleteProductToolStripMenuItem";
            this.deleteProductToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteProductToolStripMenuItem.Text = "Delete product";
            this.deleteProductToolStripMenuItem.Click += new System.EventHandler(this.deleteProductToolStripMenuItem_Click);
            // 
            // productNameTxtBox
            // 
            this.productNameTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.productNameTxtBox.BackColor = System.Drawing.SystemColors.Control;
            this.productNameTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.productNameTxtBox.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.productNameTxtBox.Location = new System.Drawing.Point(14, 24);
            this.productNameTxtBox.Name = "productNameTxtBox";
            this.productNameTxtBox.Size = new System.Drawing.Size(454, 28);
            this.productNameTxtBox.TabIndex = 19;
            this.productNameTxtBox.Text = "Product Name";
            // 
            // authorLbl
            // 
            this.authorLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.authorLbl.AutoEllipsis = true;
            this.authorLbl.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.authorLbl.Location = new System.Drawing.Point(14, 55);
            this.authorLbl.Name = "authorLbl";
            this.authorLbl.Size = new System.Drawing.Size(458, 16);
            this.authorLbl.TabIndex = 20;
            this.authorLbl.Text = "Author: ";
            // 
            // ProductRecordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 340);
            this.Controls.Add(this.authorLbl);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.applyChangesBtn);
            this.Controls.Add(this.thumbnailBox);
            this.Controls.Add(this.browseImageBtn);
            this.Controls.Add(this.dateExtractedLbl);
            this.Controls.Add(this.destinationPathLbl);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.productNameTxtBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(571, 303);
            this.Name = "ProductRecordForm";
            this.Text = "Product Record Form";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ProductRecordForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.thumbnailBox)).EndInit();
            this.thumbnailStrip.ResumeLayout(false);
            this.genericStrip.ResumeLayout(false);
            this.tagsStrip.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tagsPage.ResumeLayout(false);
            this.fileHierachyPage.ResumeLayout(false);
            this.contentFoldersPage.ResumeLayout(false);
            this.fileListPage.ResumeLayout(false);
            this.erroredFilesPage.ResumeLayout(false);
            this.errorMessagesPage.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label productNameLbl;
        private System.Windows.Forms.PictureBox thumbnailBox;
        private System.Windows.Forms.Button browseImageBtn;
        private System.Windows.Forms.ListView contentFoldersList;
        private System.Windows.Forms.ListView filesExtractedList;
        private System.Windows.Forms.Label dateExtractedLbl;
        private System.Windows.Forms.ListView erroredFilesList;
        private System.Windows.Forms.Button applyChangesBtn;
        private System.Windows.Forms.ListView tagsView;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ListView errorMessagesList;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label destinationPathLbl;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tagsPage;
        private System.Windows.Forms.TabPage fileHierachyPage;
        private System.Windows.Forms.TreeView fileTreeView;
        private System.Windows.Forms.TabPage contentFoldersPage;
        private System.Windows.Forms.TabPage fileListPage;
        private System.Windows.Forms.TabPage erroredFilesPage;
        private System.Windows.Forms.TabPage errorMessagesPage;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem deleteRecordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteProductToolStripMenuItem;
        private System.Windows.Forms.TextBox productNameTxtBox;
        private System.Windows.Forms.ContextMenuStrip tagsStrip;
        private System.Windows.Forms.ToolStripMenuItem removeTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteNewTagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editTagsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip genericStrip;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem copyPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInFileExplorerToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip thumbnailStrip;
        private System.Windows.Forms.ToolStripMenuItem copyImageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyImagePathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInFileExplorerToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem removeImageToolStripMenuItem;
        private System.Windows.Forms.Label authorLbl;
    }
}