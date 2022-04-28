namespace DAZ_Installer
{
    partial class LibrarySearchItem
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
            this.titleText = new System.Windows.Forms.Label();
            this.imageBox = new System.Windows.Forms.PictureBox();
            this.tagsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // titleText
            // 
            this.titleText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.titleText.AutoEllipsis = true;
            this.titleText.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.titleText.Location = new System.Drawing.Point(150, 11);
            this.titleText.Margin = new System.Windows.Forms.Padding(0);
            this.titleText.Name = "titleText";
            this.titleText.Size = new System.Drawing.Size(383, 41);
            this.titleText.TabIndex = 2;
            this.titleText.Text = "Title of Product";
            this.titleText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // imageBox
            // 
            this.imageBox.Location = new System.Drawing.Point(23, 11);
            this.imageBox.Name = "imageBox";
            this.imageBox.Size = new System.Drawing.Size(105, 98);
            this.imageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.imageBox.TabIndex = 3;
            this.imageBox.TabStop = false;
            // 
            // tagsLayoutPanel
            // 
            this.tagsLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagsLayoutPanel.Location = new System.Drawing.Point(158, 55);
            this.tagsLayoutPanel.Name = "tagsLayoutPanel";
            this.tagsLayoutPanel.Size = new System.Drawing.Size(375, 28);
            this.tagsLayoutPanel.TabIndex = 9;
            this.tagsLayoutPanel.WrapContents = false;
            // 
            // LibrarySearchItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Controls.Add(this.tagsLayoutPanel);
            this.Controls.Add(this.imageBox);
            this.Controls.Add(this.titleText);
            this.Name = "LibrarySearchItem";
            this.Size = new System.Drawing.Size(552, 126);
            this.ClientSizeChanged += new System.EventHandler(this.LibrarySearchItem_ClientSizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.imageBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label titleText;
        private System.Windows.Forms.PictureBox imageBox;
        private System.Windows.Forms.FlowLayoutPanel tagsLayoutPanel;
    }
}
