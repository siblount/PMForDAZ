
namespace DAZ_Installer
{
    partial class LibraryItem
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
            this.imageBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tagsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.invisibleLabel = new System.Windows.Forms.Label();
            this.showFoldersBtn = new System.Windows.Forms.Button();
            this.libraryItemMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.removeRecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeProductToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).BeginInit();
            this.libraryItemMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageBox
            // 
            this.imageBox.Location = new System.Drawing.Point(12, 10);
            this.imageBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.imageBox.Name = "imageBox";
            this.imageBox.Size = new System.Drawing.Size(109, 94);
            this.imageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox.TabIndex = 0;
            this.imageBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoEllipsis = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(128, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(335, 31);
            this.label1.TabIndex = 1;
            this.label1.Text = "Title of Product";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tagsLayoutPanel
            // 
            this.tagsLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagsLayoutPanel.Location = new System.Drawing.Point(135, 44);
            this.tagsLayoutPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tagsLayoutPanel.Name = "tagsLayoutPanel";
            this.tagsLayoutPanel.Size = new System.Drawing.Size(328, 21);
            this.tagsLayoutPanel.TabIndex = 8;
            this.tagsLayoutPanel.WrapContents = false;
            this.tagsLayoutPanel.ClientSizeChanged += new System.EventHandler(this.tagsLayoutPanel_ClientSizeChanged);
            // 
            // invisibleLabel
            // 
            this.invisibleLabel.AutoSize = true;
            this.invisibleLabel.Font = new System.Drawing.Font("Segoe UI", 4.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.invisibleLabel.Location = new System.Drawing.Point(64, 104);
            this.invisibleLabel.Name = "invisibleLabel";
            this.invisibleLabel.Size = new System.Drawing.Size(0, 10);
            this.invisibleLabel.TabIndex = 11;
            // 
            // showFoldersBtn
            // 
            this.showFoldersBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.showFoldersBtn.FlatAppearance.BorderSize = 0;
            this.showFoldersBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.showFoldersBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.showFoldersBtn.Location = new System.Drawing.Point(135, 78);
            this.showFoldersBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.showFoldersBtn.Name = "showFoldersBtn";
            this.showFoldersBtn.Size = new System.Drawing.Size(328, 22);
            this.showFoldersBtn.TabIndex = 13;
            this.showFoldersBtn.Text = "Show more info";
            this.showFoldersBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.showFoldersBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.showFoldersBtn.UseVisualStyleBackColor = true;
            this.showFoldersBtn.Click += new System.EventHandler(this.showFoldersBtn_Click);
            // 
            // libraryItemMenuStrip
            // 
            this.libraryItemMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.removeRecordToolStripMenuItem,
            this.removeProductToolStripMenuItem});
            this.libraryItemMenuStrip.Name = "libraryItemMenuStrip";
            this.libraryItemMenuStrip.Size = new System.Drawing.Size(163, 48);
            // 
            // removeRecordToolStripMenuItem
            // 
            this.removeRecordToolStripMenuItem.Name = "removeRecordToolStripMenuItem";
            this.removeRecordToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.removeRecordToolStripMenuItem.Text = "Remove record";
            this.removeRecordToolStripMenuItem.Click += new System.EventHandler(this.removeRecordToolStripMenuItem_Click);
            // 
            // removeProductToolStripMenuItem
            // 
            this.removeProductToolStripMenuItem.Name = "removeProductToolStripMenuItem";
            this.removeProductToolStripMenuItem.Size = new System.Drawing.Size(162, 22);
            this.removeProductToolStripMenuItem.Text = "Remove product";
            this.removeProductToolStripMenuItem.Click += new System.EventHandler(this.removeProductToolStripMenuItem_Click);
            // 
            // LibraryItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.ContextMenuStrip = this.libraryItemMenuStrip;
            this.Controls.Add(this.showFoldersBtn);
            this.Controls.Add(this.invisibleLabel);
            this.Controls.Add(this.tagsLayoutPanel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.imageBox);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "LibraryItem";
            this.Size = new System.Drawing.Size(472, 116);
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).EndInit();
            this.libraryItemMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox imageBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel tagsLayoutPanel;
        private System.Windows.Forms.Label invisibleLabel;
        private System.Windows.Forms.Button showFoldersBtn;
        private System.Windows.Forms.ContextMenuStrip libraryItemMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem removeRecordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeProductToolStripMenuItem;
    }
}
