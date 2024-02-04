
namespace DAZ_Installer.Windows.Forms
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
            components = new System.ComponentModel.Container();
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(ProductRecordForm));
            thumbnailBox = new System.Windows.Forms.PictureBox();
            thumbnailStrip = new System.Windows.Forms.ContextMenuStrip(components);
            copyImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyImagePathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openInFileExplorerToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            removeImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            browseImageBtn = new System.Windows.Forms.Button();
            genericStrip = new System.Windows.Forms.ContextMenuStrip(components);
            copyToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            copyPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openInFileExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            filesExtractedList = new System.Windows.Forms.ListView();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            dateExtractedLbl = new System.Windows.Forms.Label();
            destinationPathLbl = new System.Windows.Forms.Label();
            tagsView = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            tagsStrip = new System.Windows.Forms.ContextMenuStrip(components);
            editTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            removeTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pasteNewTagsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            applyChangesBtn = new System.Windows.Forms.Button();
            tabControl1 = new System.Windows.Forms.TabControl();
            tagsPage = new System.Windows.Forms.TabPage();
            fileHierachyPage = new System.Windows.Forms.TabPage();
            fileTreeView = new System.Windows.Forms.TreeView();
            fileListPage = new System.Windows.Forms.TabPage();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            toolStripButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            deleteRecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteProductToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            productNameTxtBox = new System.Windows.Forms.TextBox();
            authorLbl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)thumbnailBox).BeginInit();
            thumbnailStrip.SuspendLayout();
            genericStrip.SuspendLayout();
            tagsStrip.SuspendLayout();
            tabControl1.SuspendLayout();
            tagsPage.SuspendLayout();
            fileHierachyPage.SuspendLayout();
            fileListPage.SuspendLayout();
            toolStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // thumbnailBox
            // 
            thumbnailBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            thumbnailBox.ContextMenuStrip = thumbnailStrip;
            thumbnailBox.Image = Resources.NoImageFound;
            thumbnailBox.Location = new System.Drawing.Point(515, 25);
            thumbnailBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            thumbnailBox.Name = "thumbnailBox";
            thumbnailBox.Size = new System.Drawing.Size(127, 114);
            thumbnailBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            thumbnailBox.TabIndex = 2;
            thumbnailBox.TabStop = false;
            // 
            // thumbnailStrip
            // 
            thumbnailStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyImageToolStripMenuItem, copyImagePathToolStripMenuItem, openInFileExplorerToolStripMenuItem1, removeImageToolStripMenuItem });
            thumbnailStrip.Name = "thumbnailStrip";
            thumbnailStrip.Size = new System.Drawing.Size(182, 92);
            thumbnailStrip.Opening += thumbnailStrip_Opening;
            // 
            // copyImageToolStripMenuItem
            // 
            copyImageToolStripMenuItem.Enabled = false;
            copyImageToolStripMenuItem.Name = "copyImageToolStripMenuItem";
            copyImageToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            copyImageToolStripMenuItem.Text = "Copy image";
            copyImageToolStripMenuItem.Click += copyImageToolStripMenuItem_Click;
            // 
            // copyImagePathToolStripMenuItem
            // 
            copyImagePathToolStripMenuItem.Enabled = false;
            copyImagePathToolStripMenuItem.Name = "copyImagePathToolStripMenuItem";
            copyImagePathToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            copyImagePathToolStripMenuItem.Text = "Copy image path";
            copyImagePathToolStripMenuItem.Click += copyImagePathToolStripMenuItem_Click;
            // 
            // openInFileExplorerToolStripMenuItem1
            // 
            openInFileExplorerToolStripMenuItem1.Enabled = false;
            openInFileExplorerToolStripMenuItem1.Name = "openInFileExplorerToolStripMenuItem1";
            openInFileExplorerToolStripMenuItem1.Size = new System.Drawing.Size(181, 22);
            openInFileExplorerToolStripMenuItem1.Text = "Open in file explorer";
            openInFileExplorerToolStripMenuItem1.Click += openInFileExplorerToolStripMenuItem1_Click;
            // 
            // removeImageToolStripMenuItem
            // 
            removeImageToolStripMenuItem.Enabled = false;
            removeImageToolStripMenuItem.Name = "removeImageToolStripMenuItem";
            removeImageToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            removeImageToolStripMenuItem.Text = "Remove image";
            removeImageToolStripMenuItem.Click += removeImageToolStripMenuItem_Click;
            // 
            // browseImageBtn
            // 
            browseImageBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            browseImageBtn.Location = new System.Drawing.Point(477, 142);
            browseImageBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            browseImageBtn.Name = "browseImageBtn";
            browseImageBtn.Size = new System.Drawing.Size(197, 22);
            browseImageBtn.TabIndex = 3;
            browseImageBtn.Text = "Browse Image";
            browseImageBtn.UseVisualStyleBackColor = true;
            browseImageBtn.Click += browseImageBtn_Click;
            // 
            // genericStrip
            // 
            genericStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { copyToolStripMenuItem1, copyPathToolStripMenuItem, openInFileExplorerToolStripMenuItem });
            genericStrip.Name = "genericStrip";
            genericStrip.Size = new System.Drawing.Size(182, 70);
            genericStrip.Opening += genericStrip_Opening;
            // 
            // copyToolStripMenuItem1
            // 
            copyToolStripMenuItem1.Enabled = false;
            copyToolStripMenuItem1.Name = "copyToolStripMenuItem1";
            copyToolStripMenuItem1.Size = new System.Drawing.Size(181, 22);
            copyToolStripMenuItem1.Text = "Copy";
            copyToolStripMenuItem1.Click += copyToolStripMenuItem1_Click;
            // 
            // copyPathToolStripMenuItem
            // 
            copyPathToolStripMenuItem.Enabled = false;
            copyPathToolStripMenuItem.Name = "copyPathToolStripMenuItem";
            copyPathToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            copyPathToolStripMenuItem.Text = "Copy path";
            copyPathToolStripMenuItem.Click += copyPathToolStripMenuItem_Click;
            // 
            // openInFileExplorerToolStripMenuItem
            // 
            openInFileExplorerToolStripMenuItem.Enabled = false;
            openInFileExplorerToolStripMenuItem.Name = "openInFileExplorerToolStripMenuItem";
            openInFileExplorerToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            openInFileExplorerToolStripMenuItem.Text = "Open in file explorer";
            openInFileExplorerToolStripMenuItem.Click += openInFileExplorerToolStripMenuItem_Click;
            // 
            // filesExtractedList
            // 
            filesExtractedList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader2 });
            filesExtractedList.ContextMenuStrip = genericStrip;
            filesExtractedList.Dock = System.Windows.Forms.DockStyle.Fill;
            filesExtractedList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            filesExtractedList.Location = new System.Drawing.Point(0, 0);
            filesExtractedList.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            filesExtractedList.Name = "filesExtractedList";
            filesExtractedList.Size = new System.Drawing.Size(660, 106);
            filesExtractedList.TabIndex = 9;
            filesExtractedList.UseCompatibleStateImageBehavior = false;
            filesExtractedList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            columnHeader2.Width = 600;
            // 
            // dateExtractedLbl
            // 
            dateExtractedLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dateExtractedLbl.AutoEllipsis = true;
            dateExtractedLbl.Cursor = System.Windows.Forms.Cursors.IBeam;
            dateExtractedLbl.Location = new System.Drawing.Point(14, 71);
            dateExtractedLbl.Name = "dateExtractedLbl";
            dateExtractedLbl.Size = new System.Drawing.Size(458, 15);
            dateExtractedLbl.TabIndex = 10;
            dateExtractedLbl.Text = "Date Extracted: ";
            // 
            // destinationPathLbl
            // 
            destinationPathLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            destinationPathLbl.AutoEllipsis = true;
            destinationPathLbl.Cursor = System.Windows.Forms.Cursors.IBeam;
            destinationPathLbl.Location = new System.Drawing.Point(14, 86);
            destinationPathLbl.Name = "destinationPathLbl";
            destinationPathLbl.Size = new System.Drawing.Size(458, 78);
            destinationPathLbl.TabIndex = 14;
            destinationPathLbl.Text = "Destination Path: ";
            // 
            // tagsView
            // 
            tagsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1 });
            tagsView.ContextMenuStrip = tagsStrip;
            tagsView.Dock = System.Windows.Forms.DockStyle.Fill;
            tagsView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            tagsView.Location = new System.Drawing.Point(3, 2);
            tagsView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tagsView.Name = "tagsView";
            tagsView.Size = new System.Drawing.Size(654, 102);
            tagsView.TabIndex = 13;
            tagsView.UseCompatibleStateImageBehavior = false;
            tagsView.View = System.Windows.Forms.View.Details;
            tagsView.KeyUp += tagsView_KeyUp;
            // 
            // columnHeader1
            // 
            columnHeader1.Width = 280;
            // 
            // tagsStrip
            // 
            tagsStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { editTagsToolStripMenuItem, removeTagToolStripMenuItem, copyToolStripMenuItem, pasteNewTagsToolStripMenuItem, replaceToolStripMenuItem });
            tagsStrip.Name = "tagsStrip";
            tagsStrip.Size = new System.Drawing.Size(161, 114);
            tagsStrip.Opening += tagsStrip_Opening;
            // 
            // editTagsToolStripMenuItem
            // 
            editTagsToolStripMenuItem.Name = "editTagsToolStripMenuItem";
            editTagsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            editTagsToolStripMenuItem.Text = "Edit tags";
            editTagsToolStripMenuItem.Click += editTagsToolStripMenuItem_Click;
            // 
            // removeTagToolStripMenuItem
            // 
            removeTagToolStripMenuItem.Enabled = false;
            removeTagToolStripMenuItem.Name = "removeTagToolStripMenuItem";
            removeTagToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            removeTagToolStripMenuItem.Text = "Remove";
            removeTagToolStripMenuItem.Click += removeTagToolStripMenuItem_Click;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Enabled = false;
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // pasteNewTagsToolStripMenuItem
            // 
            pasteNewTagsToolStripMenuItem.Enabled = false;
            pasteNewTagsToolStripMenuItem.Name = "pasteNewTagsToolStripMenuItem";
            pasteNewTagsToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            pasteNewTagsToolStripMenuItem.Text = "Paste new tag(s)";
            pasteNewTagsToolStripMenuItem.Click += pasteNewTagsToolStripMenuItem_Click;
            // 
            // replaceToolStripMenuItem
            // 
            replaceToolStripMenuItem.Enabled = false;
            replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            replaceToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            replaceToolStripMenuItem.Text = "Replace";
            replaceToolStripMenuItem.Click += replaceToolStripMenuItem_Click;
            // 
            // applyChangesBtn
            // 
            applyChangesBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            applyChangesBtn.Location = new System.Drawing.Point(562, 310);
            applyChangesBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            applyChangesBtn.Name = "applyChangesBtn";
            applyChangesBtn.Size = new System.Drawing.Size(112, 22);
            applyChangesBtn.TabIndex = 14;
            applyChangesBtn.Text = "Apply Changes";
            applyChangesBtn.UseVisualStyleBackColor = true;
            applyChangesBtn.Click += applyChangesBtn_Click;
            // 
            // tabControl1
            // 
            tabControl1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tabControl1.Controls.Add(tagsPage);
            tabControl1.Controls.Add(fileHierachyPage);
            tabControl1.Controls.Add(fileListPage);
            tabControl1.Location = new System.Drawing.Point(10, 169);
            tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(668, 134);
            tabControl1.TabIndex = 17;
            // 
            // tagsPage
            // 
            tagsPage.Controls.Add(tagsView);
            tagsPage.Location = new System.Drawing.Point(4, 24);
            tagsPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tagsPage.Name = "tagsPage";
            tagsPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tagsPage.Size = new System.Drawing.Size(660, 106);
            tagsPage.TabIndex = 0;
            tagsPage.Text = "Tags";
            tagsPage.UseVisualStyleBackColor = true;
            // 
            // fileHierachyPage
            // 
            fileHierachyPage.Controls.Add(fileTreeView);
            fileHierachyPage.Location = new System.Drawing.Point(4, 24);
            fileHierachyPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            fileHierachyPage.Name = "fileHierachyPage";
            fileHierachyPage.Padding = new System.Windows.Forms.Padding(3, 2, 3, 2);
            fileHierachyPage.Size = new System.Drawing.Size(660, 106);
            fileHierachyPage.TabIndex = 1;
            fileHierachyPage.Text = "File Hierachy";
            fileHierachyPage.UseVisualStyleBackColor = true;
            // 
            // fileTreeView
            // 
            fileTreeView.ContextMenuStrip = genericStrip;
            fileTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            fileTreeView.Location = new System.Drawing.Point(3, 2);
            fileTreeView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            fileTreeView.Name = "fileTreeView";
            fileTreeView.Size = new System.Drawing.Size(654, 102);
            fileTreeView.TabIndex = 0;
            // 
            // fileListPage
            // 
            fileListPage.Controls.Add(filesExtractedList);
            fileListPage.Location = new System.Drawing.Point(4, 24);
            fileListPage.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            fileListPage.Name = "fileListPage";
            fileListPage.Size = new System.Drawing.Size(660, 106);
            fileListPage.TabIndex = 5;
            fileListPage.Text = "File List";
            fileListPage.UseVisualStyleBackColor = true;
            // 
            // toolStrip1
            // 
            toolStrip1.AllowMerge = false;
            toolStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripButton1 });
            toolStrip1.Location = new System.Drawing.Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(684, 25);
            toolStrip1.TabIndex = 18;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            toolStripButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { deleteRecordToolStripMenuItem, deleteProductToolStripMenuItem });
            toolStripButton1.Image = (System.Drawing.Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new System.Drawing.Size(53, 22);
            toolStripButton1.Text = "Delete";
            // 
            // deleteRecordToolStripMenuItem
            // 
            deleteRecordToolStripMenuItem.Name = "deleteRecordToolStripMenuItem";
            deleteRecordToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            deleteRecordToolStripMenuItem.Text = "Delete record";
            deleteRecordToolStripMenuItem.Click += deleteRecordToolStripMenuItem_Click;
            // 
            // deleteProductToolStripMenuItem
            // 
            deleteProductToolStripMenuItem.Name = "deleteProductToolStripMenuItem";
            deleteProductToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            deleteProductToolStripMenuItem.Text = "Delete product";
            deleteProductToolStripMenuItem.Click += deleteProductToolStripMenuItem_Click;
            // 
            // productNameTxtBox
            // 
            productNameTxtBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            productNameTxtBox.BackColor = System.Drawing.SystemColors.Control;
            productNameTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            productNameTxtBox.Font = new System.Drawing.Font("Segoe UI Variable Display Semib", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            productNameTxtBox.Location = new System.Drawing.Point(14, 24);
            productNameTxtBox.Name = "productNameTxtBox";
            productNameTxtBox.Size = new System.Drawing.Size(454, 28);
            productNameTxtBox.TabIndex = 19;
            productNameTxtBox.Text = "Product Name";
            // 
            // authorLbl
            // 
            authorLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            authorLbl.AutoEllipsis = true;
            authorLbl.Cursor = System.Windows.Forms.Cursors.IBeam;
            authorLbl.Location = new System.Drawing.Point(14, 55);
            authorLbl.Name = "authorLbl";
            authorLbl.Size = new System.Drawing.Size(458, 16);
            authorLbl.TabIndex = 20;
            authorLbl.Text = "Author: ";
            // 
            // ProductRecordForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(684, 340);
            Controls.Add(authorLbl);
            Controls.Add(toolStrip1);
            Controls.Add(applyChangesBtn);
            Controls.Add(thumbnailBox);
            Controls.Add(browseImageBtn);
            Controls.Add(dateExtractedLbl);
            Controls.Add(destinationPathLbl);
            Controls.Add(tabControl1);
            Controls.Add(productNameTxtBox);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(571, 303);
            Name = "ProductRecordForm";
            Text = "Product Record Form";
            FormClosed += ProductRecordForm_FormClosed;
            ((System.ComponentModel.ISupportInitialize)thumbnailBox).EndInit();
            thumbnailStrip.ResumeLayout(false);
            genericStrip.ResumeLayout(false);
            tagsStrip.ResumeLayout(false);
            tabControl1.ResumeLayout(false);
            tagsPage.ResumeLayout(false);
            fileHierachyPage.ResumeLayout(false);
            fileListPage.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label productNameLbl;
        private System.Windows.Forms.PictureBox thumbnailBox;
        private System.Windows.Forms.Button browseImageBtn;
        private System.Windows.Forms.ListView filesExtractedList;
        private System.Windows.Forms.Label dateExtractedLbl;
        private System.Windows.Forms.Button applyChangesBtn;
        private System.Windows.Forms.ListView tagsView;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label destinationPathLbl;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tagsPage;
        private System.Windows.Forms.TabPage fileHierachyPage;
        private System.Windows.Forms.TreeView fileTreeView;
        private System.Windows.Forms.TabPage fileListPage;
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