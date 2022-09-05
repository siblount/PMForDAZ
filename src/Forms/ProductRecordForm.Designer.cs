
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductRecordForm));
            this.productNameLbl = new System.Windows.Forms.Label();
            this.thumbnailBox = new System.Windows.Forms.PictureBox();
            this.browseImageBtn = new System.Windows.Forms.Button();
            this.contentFoldersList = new System.Windows.Forms.ListView();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
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
            ((System.ComponentModel.ISupportInitialize)(this.thumbnailBox)).BeginInit();
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
            // productNameLbl
            // 
            this.productNameLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.productNameLbl.AutoEllipsis = true;
            this.productNameLbl.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.productNameLbl.Location = new System.Drawing.Point(10, 22);
            this.productNameLbl.Name = "productNameLbl";
            this.productNameLbl.Size = new System.Drawing.Size(461, 32);
            this.productNameLbl.TabIndex = 0;
            this.productNameLbl.Text = "Product Name";
            // 
            // thumbnailBox
            // 
            this.thumbnailBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.thumbnailBox.Image = global::DAZ_Installer.Properties.Resources.NoImageFound;
            this.thumbnailBox.Location = new System.Drawing.Point(477, 24);
            this.thumbnailBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.thumbnailBox.Name = "thumbnailBox";
            this.thumbnailBox.Size = new System.Drawing.Size(195, 114);
            this.thumbnailBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.thumbnailBox.TabIndex = 2;
            this.thumbnailBox.TabStop = false;
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
            // filesExtractedList
            // 
            this.filesExtractedList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
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
            this.dateExtractedLbl.Location = new System.Drawing.Point(14, 54);
            this.dateExtractedLbl.Name = "dateExtractedLbl";
            this.dateExtractedLbl.Size = new System.Drawing.Size(458, 15);
            this.dateExtractedLbl.TabIndex = 10;
            this.dateExtractedLbl.Text = "Date Extracted: ";
            // 
            // erroredFilesList
            // 
            this.erroredFilesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3});
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
            this.destinationPathLbl.Location = new System.Drawing.Point(14, 81);
            this.destinationPathLbl.Name = "destinationPathLbl";
            this.destinationPathLbl.Size = new System.Drawing.Size(458, 83);
            this.destinationPathLbl.TabIndex = 14;
            this.destinationPathLbl.Text = "Destination Path: ";
            // 
            // tagsView
            // 
            this.tagsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.tagsView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tagsView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.tagsView.Location = new System.Drawing.Point(3, 2);
            this.tagsView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tagsView.Name = "tagsView";
            this.tagsView.Size = new System.Drawing.Size(654, 102);
            this.tagsView.TabIndex = 13;
            this.tagsView.UseCompatibleStateImageBehavior = false;
            this.tagsView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 280;
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
            this.deleteRecordToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteRecordToolStripMenuItem.Text = "Delete record";
            this.deleteRecordToolStripMenuItem.Click += new System.EventHandler(this.deleteRecordToolStripMenuItem_Click);
            // 
            // deleteProductToolStripMenuItem
            // 
            this.deleteProductToolStripMenuItem.Name = "deleteProductToolStripMenuItem";
            this.deleteProductToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.deleteProductToolStripMenuItem.Text = "Delete product";
            this.deleteProductToolStripMenuItem.Click += new System.EventHandler(this.deleteProductToolStripMenuItem_Click);
            // 
            // ProductRecordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 340);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.applyChangesBtn);
            this.Controls.Add(this.thumbnailBox);
            this.Controls.Add(this.productNameLbl);
            this.Controls.Add(this.browseImageBtn);
            this.Controls.Add(this.dateExtractedLbl);
            this.Controls.Add(this.destinationPathLbl);
            this.Controls.Add(this.tabControl1);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(571, 303);
            this.Name = "ProductRecordForm";
            this.Text = "Product Record Form";
            ((System.ComponentModel.ISupportInitialize)(this.thumbnailBox)).EndInit();
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
    }
}