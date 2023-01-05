
namespace DAZ_Installer.WinApp.Pages
{
    partial class Extract
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Extract));
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.fileListPage = new System.Windows.Forms.TabPage();
            this.fileListView = new System.Windows.Forms.ListView();
            this.filePathColumn = new System.Windows.Forms.ColumnHeader();
            this.fileListContextStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.inspectFileListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInHierachyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noFilesSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileHierachyPage = new System.Windows.Forms.TabPage();
            this.fileHierachyTree = new System.Windows.Forms.TreeView();
            this.archiveFolderIcons = new System.Windows.Forms.ImageList(this.components);
            this.queuePage = new System.Windows.Forms.TabPage();
            this.queueControl1 = new DAZ_Installer.Custom_Controls.QueueControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.mainProcLbl = new System.Windows.Forms.Label();
            this.fileHierachyContextStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.inspectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectInFileListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openInExplorerToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1.SuspendLayout();
            this.fileListPage.SuspendLayout();
            this.fileListContextStrip.SuspendLayout();
            this.fileHierachyPage.SuspendLayout();
            this.queuePage.SuspendLayout();
            this.panel1.SuspendLayout();
            this.fileHierachyContextStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(31, 65);
            this.mainTableLayoutPanel.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 1;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(491, 153);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.fileListPage);
            this.tabControl1.Controls.Add(this.fileHierachyPage);
            this.tabControl1.Controls.Add(this.queuePage);
            this.tabControl1.Location = new System.Drawing.Point(31, 222);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(491, 97);
            this.tabControl1.TabIndex = 1;
            // 
            // fileListPage
            // 
            this.fileListPage.Controls.Add(this.fileListView);
            this.fileListPage.Location = new System.Drawing.Point(4, 24);
            this.fileListPage.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.fileListPage.Name = "fileListPage";
            this.fileListPage.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.fileListPage.Size = new System.Drawing.Size(483, 69);
            this.fileListPage.TabIndex = 0;
            this.fileListPage.Text = "File List";
            this.fileListPage.UseVisualStyleBackColor = true;
            // 
            // fileListView
            // 
            this.fileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.filePathColumn});
            this.fileListView.ContextMenuStrip = this.fileListContextStrip;
            this.fileListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.fileListView.Location = new System.Drawing.Point(4, 2);
            this.fileListView.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.fileListView.MultiSelect = false;
            this.fileListView.Name = "fileListView";
            this.fileListView.Size = new System.Drawing.Size(475, 65);
            this.fileListView.TabIndex = 0;
            this.fileListView.UseCompatibleStateImageBehavior = false;
            this.fileListView.View = System.Windows.Forms.View.Details;
            // 
            // filePathColumn
            // 
            this.filePathColumn.Text = "File Path";
            this.filePathColumn.Width = 530;
            // 
            // fileListContextStrip
            // 
            this.fileListContextStrip.DropShadowEnabled = false;
            this.fileListContextStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.fileListContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.inspectFileListMenuItem,
            this.selectInHierachyToolStripMenuItem,
            this.openInExplorerToolStripMenuItem,
            this.noFilesSelectedToolStripMenuItem});
            this.fileListContextStrip.Name = "contextMenuStrip1";
            this.fileListContextStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.fileListContextStrip.ShowImageMargin = false;
            this.fileListContextStrip.Size = new System.Drawing.Size(144, 92);
            this.fileListContextStrip.Opening += new System.ComponentModel.CancelEventHandler(this.fileListContextStrip_Opening);
            // 
            // inspectFileListMenuItem
            // 
            this.inspectFileListMenuItem.Name = "inspectFileListMenuItem";
            this.inspectFileListMenuItem.Size = new System.Drawing.Size(143, 22);
            this.inspectFileListMenuItem.Text = "Inspect";
            this.inspectFileListMenuItem.Visible = false;
            // 
            // selectInHierachyToolStripMenuItem
            // 
            this.selectInHierachyToolStripMenuItem.Name = "selectInHierachyToolStripMenuItem";
            this.selectInHierachyToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.selectInHierachyToolStripMenuItem.Text = "Select in Hierachy";
            this.selectInHierachyToolStripMenuItem.Visible = false;
            this.selectInHierachyToolStripMenuItem.Click += new System.EventHandler(this.selectInHierachyToolStripMenuItem_Click);
            // 
            // openInExplorerToolStripMenuItem
            // 
            this.openInExplorerToolStripMenuItem.Name = "openInExplorerToolStripMenuItem";
            this.openInExplorerToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.openInExplorerToolStripMenuItem.Text = "Open in Explorer";
            this.openInExplorerToolStripMenuItem.Visible = false;
            // 
            // noFilesSelectedToolStripMenuItem
            // 
            this.noFilesSelectedToolStripMenuItem.Enabled = false;
            this.noFilesSelectedToolStripMenuItem.Name = "noFilesSelectedToolStripMenuItem";
            this.noFilesSelectedToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            this.noFilesSelectedToolStripMenuItem.Text = "No Files Selected";
            // 
            // fileHierachyPage
            // 
            this.fileHierachyPage.Controls.Add(this.fileHierachyTree);
            this.fileHierachyPage.Location = new System.Drawing.Point(4, 24);
            this.fileHierachyPage.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.fileHierachyPage.Name = "fileHierachyPage";
            this.fileHierachyPage.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.fileHierachyPage.Size = new System.Drawing.Size(483, 69);
            this.fileHierachyPage.TabIndex = 1;
            this.fileHierachyPage.Text = "File Hierachy";
            this.fileHierachyPage.UseVisualStyleBackColor = true;
            // 
            // fileHierachyTree
            // 
            this.fileHierachyTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileHierachyTree.Indent = 21;
            this.fileHierachyTree.Location = new System.Drawing.Point(4, 2);
            this.fileHierachyTree.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.fileHierachyTree.Name = "fileHierachyTree";
            this.fileHierachyTree.Size = new System.Drawing.Size(475, 65);
            this.fileHierachyTree.StateImageList = this.archiveFolderIcons;
            this.fileHierachyTree.TabIndex = 0;
            // 
            // archiveFolderIcons
            // 
            this.archiveFolderIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.archiveFolderIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("archiveFolderIcons.ImageStream")));
            this.archiveFolderIcons.TransparentColor = System.Drawing.SystemColors.Window;
            this.archiveFolderIcons.Images.SetKeyName(0, "FolderIcon.png");
            this.archiveFolderIcons.Images.SetKeyName(1, "RARIcon.png");
            this.archiveFolderIcons.Images.SetKeyName(2, "ZIPIcon.png");
            // 
            // queuePage
            // 
            this.queuePage.Controls.Add(this.queueControl1);
            this.queuePage.Location = new System.Drawing.Point(4, 24);
            this.queuePage.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.queuePage.Name = "queuePage";
            this.queuePage.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.queuePage.Size = new System.Drawing.Size(483, 69);
            this.queuePage.TabIndex = 2;
            this.queuePage.Text = "Queue";
            this.queuePage.UseVisualStyleBackColor = true;
            // 
            // queueControl1
            // 
            this.queueControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.queueControl1.Location = new System.Drawing.Point(4, 2);
            this.queueControl1.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.queueControl1.Name = "queueControl1";
            this.queueControl1.Size = new System.Drawing.Size(475, 65);
            this.queueControl1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.mainProcLbl);
            this.panel1.Location = new System.Drawing.Point(31, 22);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(486, 40);
            this.panel1.TabIndex = 2;
            // 
            // mainProcLbl
            // 
            this.mainProcLbl.AutoEllipsis = true;
            this.mainProcLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainProcLbl.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 17.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.mainProcLbl.Location = new System.Drawing.Point(0, 0);
            this.mainProcLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.mainProcLbl.Name = "mainProcLbl";
            this.mainProcLbl.Size = new System.Drawing.Size(486, 40);
            this.mainProcLbl.TabIndex = 0;
            this.mainProcLbl.Text = "Nothing to extract.";
            this.mainProcLbl.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.mainProcLbl.Click += new System.EventHandler(this.mainProcLbl_Click);
            // 
            // fileHierachyContextStrip
            // 
            this.fileHierachyContextStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.fileHierachyContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.inspectToolStripMenuItem,
            this.selectInFileListToolStripMenuItem,
            this.openInExplorerToolStripMenuItem1});
            this.fileHierachyContextStrip.Name = "fileHierachyContextStrip";
            this.fileHierachyContextStrip.Size = new System.Drawing.Size(163, 70);
            // 
            // inspectToolStripMenuItem
            // 
            this.inspectToolStripMenuItem.Name = "inspectToolStripMenuItem";
            this.inspectToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.inspectToolStripMenuItem.Text = "Inspect";
            // 
            // selectInFileListToolStripMenuItem
            // 
            this.selectInFileListToolStripMenuItem.Name = "selectInFileListToolStripMenuItem";
            this.selectInFileListToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.selectInFileListToolStripMenuItem.Text = "Select in File List";
            // 
            // openInExplorerToolStripMenuItem1
            // 
            this.openInExplorerToolStripMenuItem1.Name = "openInExplorerToolStripMenuItem1";
            this.openInExplorerToolStripMenuItem1.Size = new System.Drawing.Size(162, 22);
            this.openInExplorerToolStripMenuItem1.Text = "Open in Explorer";
            // 
            // Extract
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Name = "Extract";
            this.Size = new System.Drawing.Size(542, 344);
            this.tabControl1.ResumeLayout(false);
            this.fileListPage.ResumeLayout(false);
            this.fileListContextStrip.ResumeLayout(false);
            this.fileHierachyPage.ResumeLayout(false);
            this.queuePage.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.fileHierachyContextStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage fileListPage;
        private System.Windows.Forms.TabPage fileHierachyPage;
        private System.Windows.Forms.Panel panel1;
        internal System.Windows.Forms.Label mainProcLbl;
        private System.Windows.Forms.ListView fileListView;
        private System.Windows.Forms.TreeView fileHierachyTree;
        private System.Windows.Forms.ColumnHeader filePathColumn;
        private System.Windows.Forms.ContextMenuStrip fileListContextStrip;
        private System.Windows.Forms.ToolStripMenuItem inspectFileListMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInHierachyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInExplorerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem noFilesSelectedToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip fileHierachyContextStrip;
        private System.Windows.Forms.ToolStripMenuItem inspectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectInFileListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openInExplorerToolStripMenuItem1;
        private System.Windows.Forms.TabPage queuePage;
        private Custom_Controls.QueueControl queueControl1;
        internal System.Windows.Forms.ImageList archiveFolderIcons;
    }
}
