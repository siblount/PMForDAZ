namespace DAZ_Installer.Forms
{
    partial class ContentFolderManager
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ContentFolderManager));
            this.contentFolderTxtBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.addBtn = new System.Windows.Forms.Button();
            this.contentFoldersView = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.listViewContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.noteLbl = new System.Windows.Forms.Label();
            this.listViewContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // contentFolderTxtBox
            // 
            this.contentFolderTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentFolderTxtBox.Location = new System.Drawing.Point(137, 9);
            this.contentFolderTxtBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.contentFolderTxtBox.MaxLength = 245;
            this.contentFolderTxtBox.Name = "contentFolderTxtBox";
            this.contentFolderTxtBox.Size = new System.Drawing.Size(588, 23);
            this.contentFolderTxtBox.TabIndex = 0;
            this.contentFolderTxtBox.WordWrap = false;
            this.contentFolderTxtBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.contentFolderTxtBox_KeyUp);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Add Content Folder:";
            // 
            // addBtn
            // 
            this.addBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addBtn.Location = new System.Drawing.Point(730, 8);
            this.addBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.addBtn.Name = "addBtn";
            this.addBtn.Size = new System.Drawing.Size(59, 22);
            this.addBtn.TabIndex = 2;
            this.addBtn.Text = "Add";
            this.addBtn.UseVisualStyleBackColor = true;
            this.addBtn.Click += new System.EventHandler(this.addBtn_Click);
            // 
            // contentFoldersView
            // 
            this.contentFoldersView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentFoldersView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.contentFoldersView.ContextMenuStrip = this.listViewContextMenu;
            this.contentFoldersView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.contentFoldersView.Location = new System.Drawing.Point(10, 34);
            this.contentFoldersView.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.contentFoldersView.Name = "contentFoldersView";
            this.contentFoldersView.Size = new System.Drawing.Size(778, 234);
            this.contentFoldersView.TabIndex = 3;
            this.contentFoldersView.UseCompatibleStateImageBehavior = false;
            this.contentFoldersView.View = System.Windows.Forms.View.Details;
            this.contentFoldersView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.contentFoldersView_KeyDown);
            this.contentFoldersView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.contentFoldersView_KeyPress);
            this.contentFoldersView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.contentFoldersView_KeyUp);
            this.contentFoldersView.Resize += new System.EventHandler(this.contentFoldersView_Resize);
            // 
            // listViewContextMenu
            // 
            this.listViewContextMenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.listViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeToolStripMenuItem,
            this.resetToDefaultToolStripMenuItem,
            this.copyToolStripMenuItem});
            this.listViewContextMenu.Name = "listViewContextMenu";
            this.listViewContextMenu.Size = new System.Drawing.Size(158, 70);
            this.listViewContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.listViewContextMenu_Opening);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // resetToDefaultToolStripMenuItem
            // 
            this.resetToDefaultToolStripMenuItem.Name = "resetToDefaultToolStripMenuItem";
            this.resetToDefaultToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.resetToDefaultToolStripMenuItem.Text = "Reset to Default";
            this.resetToDefaultToolStripMenuItem.Click += new System.EventHandler(this.resetToDefaultToolStripMenuItem_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // noteLbl
            // 
            this.noteLbl.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.noteLbl.AutoSize = true;
            this.noteLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.noteLbl.Location = new System.Drawing.Point(137, 270);
            this.noteLbl.Name = "noteLbl";
            this.noteLbl.Size = new System.Drawing.Size(455, 15);
            this.noteLbl.TabIndex = 5;
            this.noteLbl.Text = "Note: You must apply changes in the Settings page in order to save your changes.";
            // 
            // ContentFolderManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(799, 292);
            this.Controls.Add(this.noteLbl);
            this.Controls.Add(this.contentFoldersView);
            this.Controls.Add(this.addBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.contentFolderTxtBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(352, 160);
            this.Name = "ContentFolderManager";
            this.Text = "Modify Content Folders";
            this.listViewContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox contentFolderTxtBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button addBtn;
        private System.Windows.Forms.ListView contentFoldersView;
        private System.Windows.Forms.ContextMenuStrip listViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetToDefaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label noteLbl;
    }
}