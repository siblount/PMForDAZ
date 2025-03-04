
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
            components = new System.ComponentModel.Container();
            imageBox = new PictureBox();
            titleLbl = new Label();
            tagsLayoutPanel = new FlowLayoutPanel();
            invisibleLabel = new Label();
            showFoldersBtn = new Button();
            libraryItemMenuStrip = new ContextMenuStrip(components);
            removeRecordToolStripMenuItem = new ToolStripMenuItem();
            removeProductToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)imageBox).BeginInit();
            libraryItemMenuStrip.SuspendLayout();
            SuspendLayout();
            // 
            // imageBox
            // 
            imageBox.Location = new Point(12, 10);
            imageBox.Margin = new Padding(3, 2, 3, 2);
            imageBox.Name = "imageBox";
            imageBox.Size = new Size(109, 94);
            imageBox.SizeMode = PictureBoxSizeMode.Zoom;
            imageBox.TabIndex = 0;
            imageBox.TabStop = false;
            // 
            // titleLbl
            // 
            titleLbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            titleLbl.AutoEllipsis = true;
            titleLbl.Font = new Font("Segoe UI Variable Display Semil", 18F);
            titleLbl.Location = new Point(128, 10);
            titleLbl.Margin = new Padding(0);
            titleLbl.Name = "titleLbl";
            titleLbl.Size = new Size(335, 31);
            titleLbl.TabIndex = 1;
            titleLbl.Text = "Title of Product";
            titleLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // tagsLayoutPanel
            // 
            tagsLayoutPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tagsLayoutPanel.Location = new Point(135, 44);
            tagsLayoutPanel.Margin = new Padding(3, 2, 3, 2);
            tagsLayoutPanel.Name = "tagsLayoutPanel";
            tagsLayoutPanel.Size = new Size(328, 21);
            tagsLayoutPanel.TabIndex = 8;
            tagsLayoutPanel.WrapContents = false;
            // 
            // invisibleLabel
            // 
            invisibleLabel.AutoSize = true;
            invisibleLabel.Font = new Font("Segoe UI", 4.8F);
            invisibleLabel.Location = new Point(64, 104);
            invisibleLabel.Name = "invisibleLabel";
            invisibleLabel.Size = new Size(0, 10);
            invisibleLabel.TabIndex = 11;
            // 
            // showFoldersBtn
            // 
            showFoldersBtn.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            showFoldersBtn.FlatAppearance.BorderSize = 0;
            showFoldersBtn.FlatStyle = FlatStyle.Flat;
            showFoldersBtn.Font = new Font("Segoe UI", 9F);
            showFoldersBtn.ImageAlign = ContentAlignment.MiddleLeft;
            showFoldersBtn.Location = new Point(135, 78);
            showFoldersBtn.Margin = new Padding(3, 2, 3, 2);
            showFoldersBtn.Name = "showFoldersBtn";
            showFoldersBtn.Size = new Size(328, 22);
            showFoldersBtn.TabIndex = 13;
            showFoldersBtn.Text = "Show more info";
            showFoldersBtn.TextAlign = ContentAlignment.MiddleLeft;
            showFoldersBtn.TextImageRelation = TextImageRelation.ImageBeforeText;
            showFoldersBtn.UseVisualStyleBackColor = true;
            showFoldersBtn.Click += showFoldersBtn_Click;
            // 
            // libraryItemMenuStrip
            // 
            libraryItemMenuStrip.Items.AddRange(new ToolStripItem[] { removeRecordToolStripMenuItem, removeProductToolStripMenuItem });
            libraryItemMenuStrip.Name = "libraryItemMenuStrip";
            libraryItemMenuStrip.Size = new Size(163, 48);
            // 
            // removeRecordToolStripMenuItem
            // 
            removeRecordToolStripMenuItem.Name = "removeRecordToolStripMenuItem";
            removeRecordToolStripMenuItem.Size = new Size(162, 22);
            removeRecordToolStripMenuItem.Text = "Remove record";
            removeRecordToolStripMenuItem.Click += removeRecordToolStripMenuItem_Click;
            // 
            // removeProductToolStripMenuItem
            // 
            removeProductToolStripMenuItem.Name = "removeProductToolStripMenuItem";
            removeProductToolStripMenuItem.Size = new Size(162, 22);
            removeProductToolStripMenuItem.Text = "Remove product";
            removeProductToolStripMenuItem.Click += removeProductToolStripMenuItem_Click;
            // 
            // LibraryItem
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            BackColor = Color.FromArgb(192, 255, 192);
            ContextMenuStrip = libraryItemMenuStrip;
            Controls.Add(showFoldersBtn);
            Controls.Add(invisibleLabel);
            Controls.Add(tagsLayoutPanel);
            Controls.Add(titleLbl);
            Controls.Add(imageBox);
            DoubleBuffered = true;
            Margin = new Padding(3, 2, 3, 2);
            Name = "LibraryItem";
            Size = new Size(472, 116);
            ((System.ComponentModel.ISupportInitialize)imageBox).EndInit();
            libraryItemMenuStrip.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox imageBox;
        private System.Windows.Forms.Label titleLbl;
        private System.Windows.Forms.FlowLayoutPanel tagsLayoutPanel;
        private System.Windows.Forms.Label invisibleLabel;
        private System.Windows.Forms.Button showFoldersBtn;
        private System.Windows.Forms.ContextMenuStrip libraryItemMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem removeRecordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeProductToolStripMenuItem;
    }
}
