
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
            this.imageBox = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tagsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.invisibleLabel = new System.Windows.Forms.Label();
            this.foldersLabel = new System.Windows.Forms.Label();
            this.showFoldersBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // imageBox
            // 
            this.imageBox.Location = new System.Drawing.Point(14, 14);
            this.imageBox.Name = "imageBox";
            this.imageBox.Size = new System.Drawing.Size(125, 119);
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
            this.label1.Location = new System.Drawing.Point(146, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(383, 41);
            this.label1.TabIndex = 1;
            this.label1.Text = "Title of Product";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tagsLayoutPanel
            // 
            this.tagsLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagsLayoutPanel.Location = new System.Drawing.Point(154, 58);
            this.tagsLayoutPanel.Name = "tagsLayoutPanel";
            this.tagsLayoutPanel.Size = new System.Drawing.Size(375, 28);
            this.tagsLayoutPanel.TabIndex = 8;
            this.tagsLayoutPanel.WrapContents = false;
            this.tagsLayoutPanel.ClientSizeChanged += new System.EventHandler(this.tagsLayoutPanel_ClientSizeChanged);
            // 
            // invisibleLabel
            // 
            this.invisibleLabel.AutoSize = true;
            this.invisibleLabel.Font = new System.Drawing.Font("Segoe UI", 4.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.invisibleLabel.Location = new System.Drawing.Point(73, 139);
            this.invisibleLabel.Name = "invisibleLabel";
            this.invisibleLabel.Size = new System.Drawing.Size(0, 11);
            this.invisibleLabel.TabIndex = 11;
            // 
            // foldersLabel
            // 
            this.foldersLabel.AutoEllipsis = true;
            this.foldersLabel.AutoSize = true;
            this.foldersLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.foldersLabel.Location = new System.Drawing.Point(177, 139);
            this.foldersLabel.Name = "foldersLabel";
            this.foldersLabel.Size = new System.Drawing.Size(120, 20);
            this.foldersLabel.TabIndex = 12;
            this.foldersLabel.Text = "Folder Label Here";
            this.foldersLabel.Visible = false;
            this.foldersLabel.VisibleChanged += new System.EventHandler(this.foldersLabel_VisibleChanged);
            // 
            // showFoldersBtn
            // 
            this.showFoldersBtn.FlatAppearance.BorderSize = 0;
            this.showFoldersBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.showFoldersBtn.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.showFoldersBtn.Location = new System.Drawing.Point(154, 104);
            this.showFoldersBtn.Name = "showFoldersBtn";
            this.showFoldersBtn.Size = new System.Drawing.Size(375, 29);
            this.showFoldersBtn.TabIndex = 13;
            this.showFoldersBtn.Text = "Folders";
            this.showFoldersBtn.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.showFoldersBtn.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.showFoldersBtn.UseVisualStyleBackColor = true;
            this.showFoldersBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.HandleFolderClick);
            // 
            // LibraryItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Controls.Add(this.showFoldersBtn);
            this.Controls.Add(this.foldersLabel);
            this.Controls.Add(this.invisibleLabel);
            this.Controls.Add(this.tagsLayoutPanel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.imageBox);
            this.Name = "LibraryItem";
            this.Size = new System.Drawing.Size(552, 159);
            this.Load += new System.EventHandler(this.LibraryItem_Load);
            this.Resize += new System.EventHandler(this.LibraryItem_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox imageBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel tagsLayoutPanel;
        private System.Windows.Forms.Label invisibleLabel;
        private System.Windows.Forms.Label foldersLabel;
        private System.Windows.Forms.Button showFoldersBtn;
    }
}
