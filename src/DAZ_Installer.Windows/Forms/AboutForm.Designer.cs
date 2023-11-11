namespace DAZ_Installer.Windows.Forms
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            titleLbl = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            mainInfoLbl = new System.Windows.Forms.Label();
            licensesRichTxtBox = new System.Windows.Forms.RichTextBox();
            licensesLbl = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // titleLbl
            // 
            titleLbl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            titleLbl.Location = new System.Drawing.Point(12, 117);
            titleLbl.Name = "titleLbl";
            titleLbl.Size = new System.Drawing.Size(313, 23);
            titleLbl.TabIndex = 0;
            titleLbl.Text = "Product Manager for DAZ Studio";
            titleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Resources.Logo2_256x;
            pictureBox1.Location = new System.Drawing.Point(84, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(164, 102);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            // 
            // mainInfoLbl
            // 
            mainInfoLbl.Location = new System.Drawing.Point(12, 149);
            mainInfoLbl.Name = "mainInfoLbl";
            mainInfoLbl.Size = new System.Drawing.Size(313, 110);
            mainInfoLbl.TabIndex = 2;
            mainInfoLbl.Text = "[TEXT DETERMINED AT FORM LOAD]";
            mainInfoLbl.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // licensesRichTxtBox
            // 
            licensesRichTxtBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            licensesRichTxtBox.Location = new System.Drawing.Point(12, 285);
            licensesRichTxtBox.Name = "licensesRichTxtBox";
            licensesRichTxtBox.Size = new System.Drawing.Size(313, 176);
            licensesRichTxtBox.TabIndex = 3;
            licensesRichTxtBox.Text = "no u";
            // 
            // licensesLbl
            // 
            licensesLbl.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            licensesLbl.Location = new System.Drawing.Point(12, 259);
            licensesLbl.Name = "licensesLbl";
            licensesLbl.Size = new System.Drawing.Size(313, 23);
            licensesLbl.TabIndex = 4;
            licensesLbl.Text = "Licenses";
            licensesLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(337, 473);
            Controls.Add(licensesLbl);
            Controls.Add(licensesRichTxtBox);
            Controls.Add(mainInfoLbl);
            Controls.Add(pictureBox1);
            Controls.Add(titleLbl);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MaximumSize = new System.Drawing.Size(353, 9999);
            MinimumSize = new System.Drawing.Size(353, 421);
            Name = "AboutForm";
            Text = "About Form";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label titleLbl;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label mainInfoLbl;
        private System.Windows.Forms.RichTextBox licensesRichTxtBox;
        private System.Windows.Forms.Label licensesLbl;
    }
}