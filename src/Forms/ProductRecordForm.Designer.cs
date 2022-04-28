
namespace DAZ_Installer
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
            this.label1 = new System.Windows.Forms.Label();
            this.productNameTxtBox = new System.Windows.Forms.TextBox();
            this.thumbnailBox = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tagsTxtBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.contentFoldersList = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.filesExtractedList = new System.Windows.Forms.ListView();
            this.dateExtractedLbl = new System.Windows.Forms.Label();
            this.erroredFilesLbl = new System.Windows.Forms.Label();
            this.erroredFilesList = new System.Windows.Forms.ListView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.thumbnailBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Product Name:";
            // 
            // productNameTxtBox
            // 
            this.productNameTxtBox.Location = new System.Drawing.Point(126, 27);
            this.productNameTxtBox.Name = "productNameTxtBox";
            this.productNameTxtBox.Size = new System.Drawing.Size(231, 27);
            this.productNameTxtBox.TabIndex = 1;
            // 
            // thumbnailBox
            // 
            this.thumbnailBox.Image = global::DAZ_Installer.Properties.Resources.NoImageFound;
            this.thumbnailBox.Location = new System.Drawing.Point(390, 27);
            this.thumbnailBox.Name = "thumbnailBox";
            this.thumbnailBox.Size = new System.Drawing.Size(226, 152);
            this.thumbnailBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.thumbnailBox.TabIndex = 2;
            this.thumbnailBox.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(390, 185);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(226, 29);
            this.button1.TabIndex = 3;
            this.button1.Text = "Browse Image";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Tags:";
            // 
            // tagsTxtBox
            // 
            this.tagsTxtBox.Location = new System.Drawing.Point(60, 67);
            this.tagsTxtBox.Name = "tagsTxtBox";
            this.tagsTxtBox.Size = new System.Drawing.Size(297, 27);
            this.tagsTxtBox.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Content Folders:";
            // 
            // contentFoldersList
            // 
            this.contentFoldersList.Location = new System.Drawing.Point(13, 130);
            this.contentFoldersList.Name = "contentFoldersList";
            this.contentFoldersList.Size = new System.Drawing.Size(344, 84);
            this.contentFoldersList.TabIndex = 7;
            this.contentFoldersList.UseCompatibleStateImageBehavior = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 217);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 20);
            this.label4.TabIndex = 8;
            this.label4.Text = "Files Extracted:";
            // 
            // filesExtractedList
            // 
            this.filesExtractedList.Location = new System.Drawing.Point(13, 240);
            this.filesExtractedList.Name = "filesExtractedList";
            this.filesExtractedList.Size = new System.Drawing.Size(603, 85);
            this.filesExtractedList.TabIndex = 9;
            this.filesExtractedList.UseCompatibleStateImageBehavior = false;
            // 
            // dateExtractedLbl
            // 
            this.dateExtractedLbl.AutoSize = true;
            this.dateExtractedLbl.Location = new System.Drawing.Point(15, 439);
            this.dateExtractedLbl.Name = "dateExtractedLbl";
            this.dateExtractedLbl.Size = new System.Drawing.Size(114, 20);
            this.dateExtractedLbl.TabIndex = 10;
            this.dateExtractedLbl.Text = "Date Extracted: ";
            // 
            // erroredFilesLbl
            // 
            this.erroredFilesLbl.AutoSize = true;
            this.erroredFilesLbl.Location = new System.Drawing.Point(13, 328);
            this.erroredFilesLbl.Name = "erroredFilesLbl";
            this.erroredFilesLbl.Size = new System.Drawing.Size(94, 20);
            this.erroredFilesLbl.TabIndex = 11;
            this.erroredFilesLbl.Text = "Errored Files:";
            // 
            // erroredFilesList
            // 
            this.erroredFilesList.Location = new System.Drawing.Point(13, 351);
            this.erroredFilesList.Name = "erroredFilesList";
            this.erroredFilesList.Size = new System.Drawing.Size(603, 85);
            this.erroredFilesList.TabIndex = 12;
            this.erroredFilesList.UseCompatibleStateImageBehavior = false;
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.erroredFilesLbl);
            this.panel1.Controls.Add(this.erroredFilesList);
            this.panel1.Controls.Add(this.filesExtractedList);
            this.panel1.Controls.Add(this.dateExtractedLbl);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.contentFoldersList);
            this.panel1.Controls.Add(this.tagsTxtBox);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Controls.Add(this.productNameTxtBox);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.thumbnailBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(628, 492);
            this.panel1.TabIndex = 13;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(488, 504);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(128, 29);
            this.button2.TabIndex = 14;
            this.button2.Text = "Apply Changes";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // ProductRecordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 544);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.panel1);
            this.Name = "ProductRecordForm";
            this.Text = "Product Record Form";
            ((System.ComponentModel.ISupportInitialize)(this.thumbnailBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox productNameTxtBox;
        private System.Windows.Forms.PictureBox thumbnailBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tagsTxtBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView contentFoldersList;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView filesExtractedList;
        private System.Windows.Forms.Label dateExtractedLbl;
        private System.Windows.Forms.Label erroredFilesLbl;
        private System.Windows.Forms.ListView erroredFilesList;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
    }
}