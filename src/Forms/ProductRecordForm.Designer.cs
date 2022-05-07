
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
            this.productNameLbl = new System.Windows.Forms.Label();
            this.productNameTxtBox = new System.Windows.Forms.TextBox();
            this.thumbnailBox = new System.Windows.Forms.PictureBox();
            this.browseImageBtn = new System.Windows.Forms.Button();
            this.tagsLbl = new System.Windows.Forms.Label();
            this.contentFoldersLbl = new System.Windows.Forms.Label();
            this.contentFoldersList = new System.Windows.Forms.ListView();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.filesLbl = new System.Windows.Forms.Label();
            this.filesExtractedList = new System.Windows.Forms.ListView();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.dateExtractedLbl = new System.Windows.Forms.Label();
            this.erroredFilesLbl = new System.Windows.Forms.Label();
            this.erroredFilesList = new System.Windows.Forms.ListView();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.panel1 = new System.Windows.Forms.Panel();
            this.errorMsgsLbl = new System.Windows.Forms.Label();
            this.errorMessagesList = new System.Windows.Forms.ListView();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.destinationPathLbl = new System.Windows.Forms.Label();
            this.tagsView = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.button2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.thumbnailBox)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // productNameLbl
            // 
            this.productNameLbl.AutoSize = true;
            this.productNameLbl.Location = new System.Drawing.Point(13, 30);
            this.productNameLbl.Name = "productNameLbl";
            this.productNameLbl.Size = new System.Drawing.Size(107, 20);
            this.productNameLbl.TabIndex = 0;
            this.productNameLbl.Text = "Product Name:";
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
            // browseImageBtn
            // 
            this.browseImageBtn.Location = new System.Drawing.Point(390, 185);
            this.browseImageBtn.Name = "browseImageBtn";
            this.browseImageBtn.Size = new System.Drawing.Size(226, 29);
            this.browseImageBtn.TabIndex = 3;
            this.browseImageBtn.Text = "Browse Image";
            this.browseImageBtn.UseVisualStyleBackColor = true;
            this.browseImageBtn.Click += new System.EventHandler(this.browseImageBtn_Click);
            // 
            // tagsLbl
            // 
            this.tagsLbl.AutoSize = true;
            this.tagsLbl.Location = new System.Drawing.Point(13, 70);
            this.tagsLbl.Name = "tagsLbl";
            this.tagsLbl.Size = new System.Drawing.Size(41, 20);
            this.tagsLbl.TabIndex = 4;
            this.tagsLbl.Text = "Tags:";
            // 
            // contentFoldersLbl
            // 
            this.contentFoldersLbl.AutoSize = true;
            this.contentFoldersLbl.Location = new System.Drawing.Point(13, 168);
            this.contentFoldersLbl.Name = "contentFoldersLbl";
            this.contentFoldersLbl.Size = new System.Drawing.Size(116, 20);
            this.contentFoldersLbl.TabIndex = 6;
            this.contentFoldersLbl.Text = "Content Folders:";
            // 
            // contentFoldersList
            // 
            this.contentFoldersList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
            this.contentFoldersList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.contentFoldersList.Location = new System.Drawing.Point(13, 191);
            this.contentFoldersList.Name = "contentFoldersList";
            this.contentFoldersList.Size = new System.Drawing.Size(344, 84);
            this.contentFoldersList.TabIndex = 7;
            this.contentFoldersList.UseCompatibleStateImageBehavior = false;
            this.contentFoldersList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Width = 320;
            // 
            // filesLbl
            // 
            this.filesLbl.AutoSize = true;
            this.filesLbl.Location = new System.Drawing.Point(12, 278);
            this.filesLbl.Name = "filesLbl";
            this.filesLbl.Size = new System.Drawing.Size(41, 20);
            this.filesLbl.TabIndex = 8;
            this.filesLbl.Text = "Files:";
            // 
            // filesExtractedList
            // 
            this.filesExtractedList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.filesExtractedList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.filesExtractedList.Location = new System.Drawing.Point(13, 301);
            this.filesExtractedList.Name = "filesExtractedList";
            this.filesExtractedList.Size = new System.Drawing.Size(603, 85);
            this.filesExtractedList.TabIndex = 9;
            this.filesExtractedList.UseCompatibleStateImageBehavior = false;
            this.filesExtractedList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 600;
            // 
            // dateExtractedLbl
            // 
            this.dateExtractedLbl.AutoSize = true;
            this.dateExtractedLbl.Location = new System.Drawing.Point(15, 625);
            this.dateExtractedLbl.Name = "dateExtractedLbl";
            this.dateExtractedLbl.Size = new System.Drawing.Size(114, 20);
            this.dateExtractedLbl.TabIndex = 10;
            this.dateExtractedLbl.Text = "Date Extracted: ";
            // 
            // erroredFilesLbl
            // 
            this.erroredFilesLbl.AutoSize = true;
            this.erroredFilesLbl.Location = new System.Drawing.Point(12, 389);
            this.erroredFilesLbl.Name = "erroredFilesLbl";
            this.erroredFilesLbl.Size = new System.Drawing.Size(94, 20);
            this.erroredFilesLbl.TabIndex = 11;
            this.erroredFilesLbl.Text = "Errored Files:";
            // 
            // erroredFilesList
            // 
            this.erroredFilesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3});
            this.erroredFilesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.erroredFilesList.Location = new System.Drawing.Point(13, 412);
            this.erroredFilesList.Name = "erroredFilesList";
            this.erroredFilesList.Size = new System.Drawing.Size(603, 85);
            this.erroredFilesList.TabIndex = 12;
            this.erroredFilesList.UseCompatibleStateImageBehavior = false;
            this.erroredFilesList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Width = 600;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.errorMsgsLbl);
            this.panel1.Controls.Add(this.errorMessagesList);
            this.panel1.Controls.Add(this.destinationPathLbl);
            this.panel1.Controls.Add(this.tagsView);
            this.panel1.Controls.Add(this.erroredFilesLbl);
            this.panel1.Controls.Add(this.erroredFilesList);
            this.panel1.Controls.Add(this.filesExtractedList);
            this.panel1.Controls.Add(this.dateExtractedLbl);
            this.panel1.Controls.Add(this.filesLbl);
            this.panel1.Controls.Add(this.contentFoldersLbl);
            this.panel1.Controls.Add(this.contentFoldersList);
            this.panel1.Controls.Add(this.tagsLbl);
            this.panel1.Controls.Add(this.browseImageBtn);
            this.panel1.Controls.Add(this.productNameTxtBox);
            this.panel1.Controls.Add(this.productNameLbl);
            this.panel1.Controls.Add(this.thumbnailBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(644, 524);
            this.panel1.TabIndex = 13;
            // 
            // errorMsgsLbl
            // 
            this.errorMsgsLbl.AutoSize = true;
            this.errorMsgsLbl.Location = new System.Drawing.Point(13, 500);
            this.errorMsgsLbl.Name = "errorMsgsLbl";
            this.errorMsgsLbl.Size = new System.Drawing.Size(112, 20);
            this.errorMsgsLbl.TabIndex = 15;
            this.errorMsgsLbl.Text = "Error Messages:";
            // 
            // errorMessagesList
            // 
            this.errorMessagesList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5});
            this.errorMessagesList.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.errorMessagesList.Location = new System.Drawing.Point(14, 523);
            this.errorMessagesList.Name = "errorMessagesList";
            this.errorMessagesList.Size = new System.Drawing.Size(603, 85);
            this.errorMessagesList.TabIndex = 16;
            this.errorMessagesList.UseCompatibleStateImageBehavior = false;
            this.errorMessagesList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Width = 600;
            // 
            // destinationPathLbl
            // 
            this.destinationPathLbl.AutoSize = true;
            this.destinationPathLbl.Location = new System.Drawing.Point(15, 669);
            this.destinationPathLbl.Name = "destinationPathLbl";
            this.destinationPathLbl.Size = new System.Drawing.Size(124, 20);
            this.destinationPathLbl.TabIndex = 14;
            this.destinationPathLbl.Text = "Destination Path: ";
            // 
            // tagsView
            // 
            this.tagsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.tagsView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.tagsView.Location = new System.Drawing.Point(60, 70);
            this.tagsView.Name = "tagsView";
            this.tagsView.Size = new System.Drawing.Size(297, 84);
            this.tagsView.TabIndex = 13;
            this.tagsView.UseCompatibleStateImageBehavior = false;
            this.tagsView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 280;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(504, 535);
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
            this.ClientSize = new System.Drawing.Size(644, 575);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(662, 622);
            this.MinimizeBox = false;
            this.Name = "ProductRecordForm";
            this.Text = "Product Record Form";
            ((System.ComponentModel.ISupportInitialize)(this.thumbnailBox)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label productNameLbl;
        private System.Windows.Forms.TextBox productNameTxtBox;
        private System.Windows.Forms.PictureBox thumbnailBox;
        private System.Windows.Forms.Button browseImageBtn;
        private System.Windows.Forms.Label tagsLbl;
        private System.Windows.Forms.Label contentFoldersLbl;
        private System.Windows.Forms.ListView contentFoldersList;
        private System.Windows.Forms.Label filesLbl;
        private System.Windows.Forms.ListView filesExtractedList;
        private System.Windows.Forms.Label dateExtractedLbl;
        private System.Windows.Forms.Label erroredFilesLbl;
        private System.Windows.Forms.ListView erroredFilesList;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListView tagsView;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label errorMsgsLbl;
        private System.Windows.Forms.ListView errorMessagesList;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Label destinationPathLbl;
    }
}