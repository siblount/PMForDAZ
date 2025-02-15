
namespace DAZ_Installer.Windows.Pages
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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("No extraction jobs processed yet", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Extract));
            tabControl1 = new System.Windows.Forms.TabControl();
            queuePage = new System.Windows.Forms.TabPage();
            queueListView = new System.Windows.Forms.ListView();
            nameHeader = new System.Windows.Forms.ColumnHeader();
            errorMessageHeader = new System.Windows.Forms.ColumnHeader();
            queueContextStrip = new System.Windows.Forms.ContextMenuStrip(components);
            skipArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cancelExtractJobToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cancelStripSeperator = new System.Windows.Forms.ToolStripSeparator();
            cancelCurrentArchiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cancelCurrentExtractJobToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewStripSeperator = new System.Windows.Forms.ToolStripSeparator();
            viewFileListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            viewFileHierachyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            statusIcons = new System.Windows.Forms.ImageList(components);
            archiveFolderIcons = new System.Windows.Forms.ImageList(components);
            fileListPage = new System.Windows.Forms.TabPage();
            fileListView = new System.Windows.Forms.ListView();
            filePathColumn = new System.Windows.Forms.ColumnHeader();
            fileListContextStrip = new System.Windows.Forms.ContextMenuStrip(components);
            inspectFileListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            selectInHierachyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openInExplorerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            noFilesSelectedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            fileHierachyPage = new System.Windows.Forms.TabPage();
            fileHierachyTree = new System.Windows.Forms.TreeView();
            fileHierachyContextStrip = new System.Windows.Forms.ContextMenuStrip(components);
            inspectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            selectInFileListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openInExplorerToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            progressCombo = new UI.ProgressCombo();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            tabControl1.SuspendLayout();
            queuePage.SuspendLayout();
            queueContextStrip.SuspendLayout();
            fileListPage.SuspendLayout();
            fileListContextStrip.SuspendLayout();
            fileHierachyPage.SuspendLayout();
            fileHierachyContextStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(queuePage);
            tabControl1.Controls.Add(fileListPage);
            tabControl1.Controls.Add(fileHierachyPage);
            tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            tabControl1.Location = new System.Drawing.Point(0, 0);
            tabControl1.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new System.Drawing.Size(493, 137);
            tabControl1.TabIndex = 1;
            // 
            // queuePage
            // 
            queuePage.Controls.Add(queueListView);
            queuePage.Location = new System.Drawing.Point(4, 24);
            queuePage.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            queuePage.Name = "queuePage";
            queuePage.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            queuePage.Size = new System.Drawing.Size(485, 109);
            queuePage.TabIndex = 2;
            queuePage.Text = "Queue";
            queuePage.UseVisualStyleBackColor = true;
            // 
            // queueListView
            // 
            queueListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { nameHeader, errorMessageHeader });
            queueListView.ContextMenuStrip = queueContextStrip;
            queueListView.Dock = System.Windows.Forms.DockStyle.Fill;
            queueListView.GroupImageList = statusIcons;
            listViewGroup1.Header = "No extraction jobs processed yet";
            listViewGroup1.Name = "defaultGroup";
            queueListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] { listViewGroup1 });
            listViewItem1.Group = listViewGroup1;
            queueListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] { listViewItem1 });
            queueListView.LabelWrap = false;
            queueListView.Location = new System.Drawing.Point(4, 2);
            queueListView.Name = "queueListView";
            queueListView.Size = new System.Drawing.Size(477, 105);
            queueListView.SmallImageList = archiveFolderIcons;
            queueListView.StateImageList = statusIcons;
            queueListView.TabIndex = 0;
            queueListView.UseCompatibleStateImageBehavior = false;
            queueListView.View = System.Windows.Forms.View.Details;
            // 
            // nameHeader
            // 
            nameHeader.Text = "Name";
            nameHeader.Width = 275;
            // 
            // errorMessageHeader
            // 
            errorMessageHeader.Text = "Message";
            errorMessageHeader.Width = 160;
            // 
            // queueContextStrip
            // 
            queueContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { skipArchiveToolStripMenuItem, cancelCurrentArchiveToolStripMenuItem, cancelStripSeperator, cancelExtractJobToolStripMenuItem, cancelCurrentExtractJobToolStripMenuItem, viewStripSeperator, viewFileListToolStripMenuItem, viewFileHierachyToolStripMenuItem });
            queueContextStrip.Name = "queueContextStrip";
            queueContextStrip.Size = new System.Drawing.Size(211, 170);
            queueContextStrip.Opening += queueContextStrip_Opening;
            // 
            // skipArchiveToolStripMenuItem
            // 
            skipArchiveToolStripMenuItem.Name = "skipArchiveToolStripMenuItem";
            skipArchiveToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            skipArchiveToolStripMenuItem.Text = "Skip archive";
            // 
            // cancelExtractJobToolStripMenuItem
            // 
            cancelExtractJobToolStripMenuItem.Name = "cancelExtractJobToolStripMenuItem";
            cancelExtractJobToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            cancelExtractJobToolStripMenuItem.Text = "Cancel extract job";
            // 
            // cancelStripSeperator
            // 
            cancelStripSeperator.Name = "cancelStripSeperator";
            cancelStripSeperator.Size = new System.Drawing.Size(207, 6);
            // 
            // cancelCurrentArchiveToolStripMenuItem
            // 
            cancelCurrentArchiveToolStripMenuItem.Name = "cancelCurrentArchiveToolStripMenuItem";
            cancelCurrentArchiveToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            cancelCurrentArchiveToolStripMenuItem.Text = "Cancel current archive";
            // 
            // cancelCurrentExtractJobToolStripMenuItem
            // 
            cancelCurrentExtractJobToolStripMenuItem.Name = "cancelCurrentExtractJobToolStripMenuItem";
            cancelCurrentExtractJobToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            cancelCurrentExtractJobToolStripMenuItem.Text = "Cancel current extract job";
            // 
            // viewStripSeperator
            // 
            viewStripSeperator.Name = "viewStripSeperator";
            viewStripSeperator.Size = new System.Drawing.Size(207, 6);
            // 
            // viewFileListToolStripMenuItem
            // 
            viewFileListToolStripMenuItem.Name = "viewFileListToolStripMenuItem";
            viewFileListToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            viewFileListToolStripMenuItem.Text = "View File List";
            // 
            // viewFileHierachyToolStripMenuItem
            // 
            viewFileHierachyToolStripMenuItem.Name = "viewFileHierachyToolStripMenuItem";
            viewFileHierachyToolStripMenuItem.Size = new System.Drawing.Size(210, 22);
            viewFileHierachyToolStripMenuItem.Text = "View File Hierachy";
            // 
            // statusIcons
            // 
            statusIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            statusIcons.ImageSize = new System.Drawing.Size(16, 16);
            statusIcons.TransparentColor = System.Drawing.SystemColors.Window;
            // 
            // archiveFolderIcons
            // 
            archiveFolderIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            archiveFolderIcons.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("archiveFolderIcons.ImageStream");
            archiveFolderIcons.TransparentColor = System.Drawing.SystemColors.Window;
            archiveFolderIcons.Images.SetKeyName(0, "FolderIcon.png");
            archiveFolderIcons.Images.SetKeyName(1, "RARIcon.png");
            archiveFolderIcons.Images.SetKeyName(2, "ZIPIcon.png");
            // 
            // fileListPage
            // 
            fileListPage.Controls.Add(fileListView);
            fileListPage.Location = new System.Drawing.Point(4, 24);
            fileListPage.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            fileListPage.Name = "fileListPage";
            fileListPage.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            fileListPage.Size = new System.Drawing.Size(485, 109);
            fileListPage.TabIndex = 0;
            fileListPage.Text = "File List";
            fileListPage.UseVisualStyleBackColor = true;
            // 
            // fileListView
            // 
            fileListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { filePathColumn });
            fileListView.ContextMenuStrip = fileListContextStrip;
            fileListView.Dock = System.Windows.Forms.DockStyle.Fill;
            fileListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            fileListView.Location = new System.Drawing.Point(4, 2);
            fileListView.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            fileListView.MultiSelect = false;
            fileListView.Name = "fileListView";
            fileListView.Size = new System.Drawing.Size(477, 105);
            fileListView.TabIndex = 0;
            fileListView.UseCompatibleStateImageBehavior = false;
            fileListView.View = System.Windows.Forms.View.Details;
            // 
            // filePathColumn
            // 
            filePathColumn.Text = "File Path";
            filePathColumn.Width = 450;
            // 
            // fileListContextStrip
            // 
            fileListContextStrip.DropShadowEnabled = false;
            fileListContextStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            fileListContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { inspectFileListMenuItem, selectInHierachyToolStripMenuItem, openInExplorerToolStripMenuItem, noFilesSelectedToolStripMenuItem });
            fileListContextStrip.Name = "contextMenuStrip1";
            fileListContextStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            fileListContextStrip.ShowImageMargin = false;
            fileListContextStrip.Size = new System.Drawing.Size(144, 92);
            fileListContextStrip.Opening += fileListContextStrip_Opening;
            // 
            // inspectFileListMenuItem
            // 
            inspectFileListMenuItem.Name = "inspectFileListMenuItem";
            inspectFileListMenuItem.Size = new System.Drawing.Size(143, 22);
            inspectFileListMenuItem.Text = "Inspect";
            inspectFileListMenuItem.Visible = false;
            // 
            // selectInHierachyToolStripMenuItem
            // 
            selectInHierachyToolStripMenuItem.Name = "selectInHierachyToolStripMenuItem";
            selectInHierachyToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            selectInHierachyToolStripMenuItem.Text = "Select in Hierachy";
            selectInHierachyToolStripMenuItem.Visible = false;
            selectInHierachyToolStripMenuItem.Click += selectInHierachyToolStripMenuItem_Click;
            // 
            // openInExplorerToolStripMenuItem
            // 
            openInExplorerToolStripMenuItem.Name = "openInExplorerToolStripMenuItem";
            openInExplorerToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            openInExplorerToolStripMenuItem.Text = "Open in Explorer";
            openInExplorerToolStripMenuItem.Visible = false;
            // 
            // noFilesSelectedToolStripMenuItem
            // 
            noFilesSelectedToolStripMenuItem.Enabled = false;
            noFilesSelectedToolStripMenuItem.Name = "noFilesSelectedToolStripMenuItem";
            noFilesSelectedToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            noFilesSelectedToolStripMenuItem.Text = "No Files Selected";
            // 
            // fileHierachyPage
            // 
            fileHierachyPage.Controls.Add(fileHierachyTree);
            fileHierachyPage.Location = new System.Drawing.Point(4, 24);
            fileHierachyPage.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            fileHierachyPage.Name = "fileHierachyPage";
            fileHierachyPage.Padding = new System.Windows.Forms.Padding(4, 2, 4, 2);
            fileHierachyPage.Size = new System.Drawing.Size(485, 109);
            fileHierachyPage.TabIndex = 1;
            fileHierachyPage.Text = "File Hierachy";
            fileHierachyPage.UseVisualStyleBackColor = true;
            // 
            // fileHierachyTree
            // 
            fileHierachyTree.Dock = System.Windows.Forms.DockStyle.Fill;
            fileHierachyTree.Indent = 21;
            fileHierachyTree.Location = new System.Drawing.Point(4, 2);
            fileHierachyTree.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            fileHierachyTree.Name = "fileHierachyTree";
            fileHierachyTree.Size = new System.Drawing.Size(477, 105);
            fileHierachyTree.StateImageList = archiveFolderIcons;
            fileHierachyTree.TabIndex = 0;
            // 
            // fileHierachyContextStrip
            // 
            fileHierachyContextStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            fileHierachyContextStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { inspectToolStripMenuItem, selectInFileListToolStripMenuItem, openInExplorerToolStripMenuItem1 });
            fileHierachyContextStrip.Name = "fileHierachyContextStrip";
            fileHierachyContextStrip.Size = new System.Drawing.Size(163, 70);
            // 
            // inspectToolStripMenuItem
            // 
            inspectToolStripMenuItem.Name = "inspectToolStripMenuItem";
            inspectToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            inspectToolStripMenuItem.Text = "Inspect";
            // 
            // selectInFileListToolStripMenuItem
            // 
            selectInFileListToolStripMenuItem.Name = "selectInFileListToolStripMenuItem";
            selectInFileListToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            selectInFileListToolStripMenuItem.Text = "Select in File List";
            selectInFileListToolStripMenuItem.Click += selectInFileListToolStripMenuItem_Click;
            // 
            // openInExplorerToolStripMenuItem1
            // 
            openInExplorerToolStripMenuItem1.Name = "openInExplorerToolStripMenuItem1";
            openInExplorerToolStripMenuItem1.Size = new System.Drawing.Size(162, 22);
            openInExplorerToolStripMenuItem1.Text = "Open in Explorer";
            // 
            // progressCombo
            // 
            progressCombo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            progressCombo.Location = new System.Drawing.Point(29, 22);
            progressCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            progressCombo.Name = "progressCombo";
            progressCombo.Size = new System.Drawing.Size(493, 145);
            progressCombo.TabIndex = 3;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            splitContainer1.Location = new System.Drawing.Point(29, 182);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tabControl1);
            splitContainer1.Panel2Collapsed = true;
            splitContainer1.Size = new System.Drawing.Size(493, 137);
            splitContainer1.SplitterDistance = 163;
            splitContainer1.TabIndex = 4;
            // 
            // Extract
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            BackColor = System.Drawing.Color.White;
            Controls.Add(splitContainer1);
            Controls.Add(progressCombo);
            Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            Name = "Extract";
            Size = new System.Drawing.Size(542, 344);
            tabControl1.ResumeLayout(false);
            queuePage.ResumeLayout(false);
            queueContextStrip.ResumeLayout(false);
            fileListPage.ResumeLayout(false);
            fileListContextStrip.ResumeLayout(false);
            fileHierachyPage.ResumeLayout(false);
            fileHierachyContextStrip.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage fileListPage;
        private System.Windows.Forms.TabPage fileHierachyPage;
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
        internal System.Windows.Forms.ImageList archiveFolderIcons;
        internal UI.ProgressCombo progressCombo;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView queueListView;
        private System.Windows.Forms.ColumnHeader nameHeader;
        private System.Windows.Forms.ColumnHeader errorMessageHeader;
        private System.Windows.Forms.ImageList statusIcons;
        private System.Windows.Forms.ContextMenuStrip queueContextStrip;
        private System.Windows.Forms.ToolStripMenuItem skipArchiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelCurrentArchiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelExtractJobToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewFileListToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewFileHierachyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator cancelStripSeperator;
        private System.Windows.Forms.ToolStripMenuItem cancelCurrentExtractJobToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator viewStripSeperator;
    }
}
