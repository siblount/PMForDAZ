namespace DAZ_Installer.Forms
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.titleLbl = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.mainInfoLbl = new System.Windows.Forms.Label();
            this.licensesRichTxtBox = new System.Windows.Forms.RichTextBox();
            this.licensesLbl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // titleLbl
            // 
            this.titleLbl.AutoSize = true;
            this.titleLbl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.titleLbl.Location = new System.Drawing.Point(84, 117);
            this.titleLbl.Name = "titleLbl";
            this.titleLbl.Size = new System.Drawing.Size(169, 21);
            this.titleLbl.TabIndex = 0;
            this.titleLbl.Text = "Daz Product Installer";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::DAZ_Installer.Properties.Resources.Logo2_256x;
            this.pictureBox1.Location = new System.Drawing.Point(84, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(164, 102);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // mainInfoLbl
            // 
            this.mainInfoLbl.Location = new System.Drawing.Point(12, 138);
            this.mainInfoLbl.Name = "mainInfoLbl";
            this.mainInfoLbl.Size = new System.Drawing.Size(313, 98);
            this.mainInfoLbl.TabIndex = 2;
            this.mainInfoLbl.Text = resources.GetString("mainInfoLbl.Text");
            this.mainInfoLbl.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // licensesRichTxtBox
            // 
            this.licensesRichTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.licensesRichTxtBox.Location = new System.Drawing.Point(12, 262);
            this.licensesRichTxtBox.Name = "licensesRichTxtBox";
            this.licensesRichTxtBox.Size = new System.Drawing.Size(313, 108);
            this.licensesRichTxtBox.TabIndex = 3;
            this.licensesRichTxtBox.Text = "no u";
            // 
            // licensesLbl
            // 
            this.licensesLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.licensesLbl.Location = new System.Drawing.Point(12, 236);
            this.licensesLbl.Name = "licensesLbl";
            this.licensesLbl.Size = new System.Drawing.Size(313, 23);
            this.licensesLbl.TabIndex = 4;
            this.licensesLbl.Text = "Licenses";
            this.licensesLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(337, 382);
            this.Controls.Add(this.licensesLbl);
            this.Controls.Add(this.licensesRichTxtBox);
            this.Controls.Add(this.mainInfoLbl);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.titleLbl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximumSize = new System.Drawing.Size(353, 9999);
            this.MinimumSize = new System.Drawing.Size(353, 421);
            this.Name = "AboutForm";
            this.Text = "About Form";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLbl;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label mainInfoLbl;
        private System.Windows.Forms.RichTextBox licensesRichTxtBox;
        private System.Windows.Forms.Label licensesLbl;
    }
}