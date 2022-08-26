
namespace DAZ_Installer
{
    partial class Settings
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
            this.titleLbl = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.destinationPathCombo = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.downloadThumbnailsComboBox = new System.Windows.Forms.ComboBox();
            this.fileHandlingCombo = new System.Windows.Forms.ComboBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.allowOverwritingCombo = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.chooseTempPathBtn = new System.Windows.Forms.Button();
            this.chooseDestPathBtn = new System.Windows.Forms.Button();
            this.installPrevProductsCombo = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.removeSourceFilesCombo = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.modifyContentRedirectsBtn = new System.Windows.Forms.Button();
            this.modifyContentFoldersBtn = new System.Windows.Forms.Button();
            this.tempTxtBox = new System.Windows.Forms.TextBox();
            this.contentFolderRedirectsListBox = new System.Windows.Forms.ListBox();
            this.contentFoldersListBox = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.loadingPanel = new System.Windows.Forms.Panel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.loadingLbl = new System.Windows.Forms.Label();
            this.applySettingsBtn = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.loadingPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // titleLbl
            // 
            this.titleLbl.AutoSize = true;
            this.titleLbl.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.titleLbl.Location = new System.Drawing.Point(27, 18);
            this.titleLbl.Name = "titleLbl";
            this.titleLbl.Size = new System.Drawing.Size(90, 30);
            this.titleLbl.TabIndex = 2;
            this.titleLbl.Text = "Settings";
            this.titleLbl.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label6.Location = new System.Drawing.Point(45, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(106, 19);
            this.label6.TabIndex = 10;
            this.label6.Text = "Temporary Path";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label4.Location = new System.Drawing.Point(1, 176);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(150, 46);
            this.label4.TabIndex = 6;
            this.label4.Text = "Content Folder Redirects";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(11, 274);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(141, 19);
            this.label3.TabIndex = 4;
            this.label3.Text = "File Handling Method";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // destinationPathCombo
            // 
            this.destinationPathCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.destinationPathCombo.FormattingEnabled = true;
            this.destinationPathCombo.Location = new System.Drawing.Point(187, 35);
            this.destinationPathCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.destinationPathCombo.Name = "destinationPathCombo";
            this.destinationPathCombo.Size = new System.Drawing.Size(243, 23);
            this.destinationPathCombo.TabIndex = 3;
            this.destinationPathCombo.TextChanged += new System.EventHandler(this.destinationPathCombo_TextChanged);
            this.destinationPathCombo.Leave += new System.EventHandler(this.destinationPathCombo_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(38, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 19);
            this.label2.TabIndex = 2;
            this.label2.Text = "Destination Path";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(7, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(143, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Download thumbnails";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // downloadThumbnailsComboBox
            // 
            this.downloadThumbnailsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.downloadThumbnailsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.downloadThumbnailsComboBox.FormattingEnabled = true;
            this.downloadThumbnailsComboBox.Location = new System.Drawing.Point(187, 8);
            this.downloadThumbnailsComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.downloadThumbnailsComboBox.Name = "downloadThumbnailsComboBox";
            this.downloadThumbnailsComboBox.Size = new System.Drawing.Size(278, 23);
            this.downloadThumbnailsComboBox.TabIndex = 1;
            this.downloadThumbnailsComboBox.SelectedIndexChanged += new System.EventHandler(this.downloadThumbnailsComboBox_SelectedIndexChanged);
            this.downloadThumbnailsComboBox.TextChanged += new System.EventHandler(this.downloadThumbnailsComboBox_TextChanged);
            // 
            // fileHandlingCombo
            // 
            this.fileHandlingCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fileHandlingCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.fileHandlingCombo.FormattingEnabled = true;
            this.fileHandlingCombo.Location = new System.Drawing.Point(187, 274);
            this.fileHandlingCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.fileHandlingCombo.Name = "fileHandlingCombo";
            this.fileHandlingCombo.Size = new System.Drawing.Size(278, 23);
            this.fileHandlingCombo.TabIndex = 5;
            this.fileHandlingCombo.TextChanged += new System.EventHandler(this.fileHandlingCombo_TextChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.allowOverwritingCombo);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.chooseTempPathBtn);
            this.panel1.Controls.Add(this.chooseDestPathBtn);
            this.panel1.Controls.Add(this.installPrevProductsCombo);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.removeSourceFilesCombo);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.modifyContentRedirectsBtn);
            this.panel1.Controls.Add(this.modifyContentFoldersBtn);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.tempTxtBox);
            this.panel1.Controls.Add(this.contentFolderRedirectsListBox);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.fileHandlingCombo);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.contentFoldersListBox);
            this.panel1.Controls.Add(this.destinationPathCombo);
            this.panel1.Controls.Add(this.downloadThumbnailsComboBox);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Location = new System.Drawing.Point(27, 49);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(503, 256);
            this.panel1.TabIndex = 7;
            // 
            // allowOverwritingCombo
            // 
            this.allowOverwritingCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.allowOverwritingCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.allowOverwritingCombo.FormattingEnabled = true;
            this.allowOverwritingCombo.Location = new System.Drawing.Point(187, 398);
            this.allowOverwritingCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.allowOverwritingCombo.Name = "allowOverwritingCombo";
            this.allowOverwritingCombo.Size = new System.Drawing.Size(277, 23);
            this.allowOverwritingCombo.TabIndex = 20;
            this.allowOverwritingCombo.TextChanged += new System.EventHandler(this.allowOverwritingCombo_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label9.Location = new System.Drawing.Point(36, 398);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(115, 19);
            this.label9.TabIndex = 19;
            this.label9.Text = "Allow overwriting";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chooseTempPathBtn
            // 
            this.chooseTempPathBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chooseTempPathBtn.Location = new System.Drawing.Point(435, 59);
            this.chooseTempPathBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chooseTempPathBtn.Name = "chooseTempPathBtn";
            this.chooseTempPathBtn.Size = new System.Drawing.Size(30, 20);
            this.chooseTempPathBtn.TabIndex = 18;
            this.chooseTempPathBtn.Text = "...";
            this.chooseTempPathBtn.UseVisualStyleBackColor = true;
            this.chooseTempPathBtn.Click += new System.EventHandler(this.chooseTempPathBtn_Click);
            // 
            // chooseDestPathBtn
            // 
            this.chooseDestPathBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chooseDestPathBtn.Location = new System.Drawing.Point(435, 35);
            this.chooseDestPathBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.chooseDestPathBtn.Name = "chooseDestPathBtn";
            this.chooseDestPathBtn.Size = new System.Drawing.Size(30, 21);
            this.chooseDestPathBtn.TabIndex = 17;
            this.chooseDestPathBtn.Text = "...";
            this.chooseDestPathBtn.UseVisualStyleBackColor = true;
            this.chooseDestPathBtn.Click += new System.EventHandler(this.chooseDestPathBtn_Click);
            // 
            // installPrevProductsCombo
            // 
            this.installPrevProductsCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.installPrevProductsCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.installPrevProductsCombo.FormattingEnabled = true;
            this.installPrevProductsCombo.Location = new System.Drawing.Point(188, 355);
            this.installPrevProductsCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.installPrevProductsCombo.Name = "installPrevProductsCombo";
            this.installPrevProductsCombo.Size = new System.Drawing.Size(277, 23);
            this.installPrevProductsCombo.TabIndex = 16;
            this.installPrevProductsCombo.TextChanged += new System.EventHandler(this.installPrevProducts_TextChanged);
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label8.Location = new System.Drawing.Point(11, 345);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(141, 38);
            this.label8.TabIndex = 15;
            this.label8.Text = "Install previously installed products";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // removeSourceFilesCombo
            // 
            this.removeSourceFilesCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.removeSourceFilesCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.removeSourceFilesCombo.FormattingEnabled = true;
            this.removeSourceFilesCombo.Location = new System.Drawing.Point(187, 311);
            this.removeSourceFilesCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.removeSourceFilesCombo.Name = "removeSourceFilesCombo";
            this.removeSourceFilesCombo.Size = new System.Drawing.Size(277, 23);
            this.removeSourceFilesCombo.TabIndex = 14;
            this.removeSourceFilesCombo.TextChanged += new System.EventHandler(this.removeSourceFiles_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label7.Location = new System.Drawing.Point(11, 311);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(141, 19);
            this.label7.TabIndex = 13;
            this.label7.Text = "Remove Source File(s)";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // modifyContentRedirectsBtn
            // 
            this.modifyContentRedirectsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.modifyContentRedirectsBtn.Location = new System.Drawing.Point(406, 248);
            this.modifyContentRedirectsBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.modifyContentRedirectsBtn.Name = "modifyContentRedirectsBtn";
            this.modifyContentRedirectsBtn.Size = new System.Drawing.Size(59, 22);
            this.modifyContentRedirectsBtn.TabIndex = 12;
            this.modifyContentRedirectsBtn.Text = "Modify";
            this.modifyContentRedirectsBtn.UseVisualStyleBackColor = true;
            this.modifyContentRedirectsBtn.Click += new System.EventHandler(this.modifyContentRedirectsBtn_Click_1);
            // 
            // modifyContentFoldersBtn
            // 
            this.modifyContentFoldersBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.modifyContentFoldersBtn.Location = new System.Drawing.Point(406, 154);
            this.modifyContentFoldersBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.modifyContentFoldersBtn.Name = "modifyContentFoldersBtn";
            this.modifyContentFoldersBtn.Size = new System.Drawing.Size(59, 22);
            this.modifyContentFoldersBtn.TabIndex = 1;
            this.modifyContentFoldersBtn.Text = "Modify";
            this.modifyContentFoldersBtn.UseVisualStyleBackColor = true;
            this.modifyContentFoldersBtn.Click += new System.EventHandler(this.modifyContentFoldersBtn_Click);
            // 
            // tempTxtBox
            // 
            this.tempTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tempTxtBox.Location = new System.Drawing.Point(187, 59);
            this.tempTxtBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tempTxtBox.Name = "tempTxtBox";
            this.tempTxtBox.Size = new System.Drawing.Size(243, 23);
            this.tempTxtBox.TabIndex = 11;
            this.tempTxtBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tempTxtBox_KeyUp);
            this.tempTxtBox.Leave += new System.EventHandler(this.tempTxtBox_Leave);
            // 
            // contentFolderRedirectsListBox
            // 
            this.contentFolderRedirectsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentFolderRedirectsListBox.FormattingEnabled = true;
            this.contentFolderRedirectsListBox.ItemHeight = 15;
            this.contentFolderRedirectsListBox.Location = new System.Drawing.Point(187, 180);
            this.contentFolderRedirectsListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.contentFolderRedirectsListBox.Name = "contentFolderRedirectsListBox";
            this.contentFolderRedirectsListBox.Size = new System.Drawing.Size(278, 64);
            this.contentFolderRedirectsListBox.TabIndex = 0;
            // 
            // contentFoldersListBox
            // 
            this.contentFoldersListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.contentFoldersListBox.FormattingEnabled = true;
            this.contentFoldersListBox.ItemHeight = 15;
            this.contentFoldersListBox.Location = new System.Drawing.Point(187, 86);
            this.contentFoldersListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.contentFoldersListBox.Name = "contentFoldersListBox";
            this.contentFoldersListBox.Size = new System.Drawing.Size(278, 64);
            this.contentFoldersListBox.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label5.Location = new System.Drawing.Point(45, 86);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(107, 19);
            this.label5.TabIndex = 8;
            this.label5.Text = "Content Folders";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // loadingPanel
            // 
            this.loadingPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loadingPanel.Controls.Add(this.progressBar1);
            this.loadingPanel.Controls.Add(this.loadingLbl);
            this.loadingPanel.Location = new System.Drawing.Point(0, 49);
            this.loadingPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.loadingPanel.Name = "loadingPanel";
            this.loadingPanel.Size = new System.Drawing.Size(541, 295);
            this.loadingPanel.TabIndex = 4;
            this.loadingPanel.Visible = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(60, 204);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.progressBar1.MarqueeAnimationSpeed = 25;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(423, 9);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 1;
            this.progressBar1.Value = 5;
            // 
            // loadingLbl
            // 
            this.loadingLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.loadingLbl.AutoSize = true;
            this.loadingLbl.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.loadingLbl.Location = new System.Drawing.Point(150, 130);
            this.loadingLbl.Name = "loadingLbl";
            this.loadingLbl.Size = new System.Drawing.Size(228, 65);
            this.loadingLbl.TabIndex = 0;
            this.loadingLbl.Text = "Loading...";
            // 
            // applySettingsBtn
            // 
            this.applySettingsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applySettingsBtn.Enabled = false;
            this.applySettingsBtn.Location = new System.Drawing.Point(400, 309);
            this.applySettingsBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.applySettingsBtn.Name = "applySettingsBtn";
            this.applySettingsBtn.Size = new System.Drawing.Size(130, 24);
            this.applySettingsBtn.TabIndex = 2;
            this.applySettingsBtn.Text = "Apply Changes";
            this.applySettingsBtn.UseVisualStyleBackColor = true;
            this.applySettingsBtn.Click += new System.EventHandler(this.applySettingsBtn_Click);
            // 
            // Settings
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.applySettingsBtn);
            this.Controls.Add(this.titleLbl);
            this.Controls.Add(this.loadingPanel);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Settings";
            this.Size = new System.Drawing.Size(542, 344);
            this.Load += new System.EventHandler(this.Settings_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.loadingPanel.ResumeLayout(false);
            this.loadingPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLbl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox downloadThumbnailsComboBox;
        private System.Windows.Forms.ComboBox destinationPathCombo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox fileHandlingCombo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button modifyContentFoldersBtn;
        private System.Windows.Forms.ListBox contentFolderRedirectsListBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox contentFoldersListBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tempTxtBox;
        private System.Windows.Forms.Panel loadingPanel;
        private System.Windows.Forms.Label loadingLbl;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button modifyContentRedirectsBtn;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox removeSourceFilesCombo;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox installPrevProductsCombo;
        private System.Windows.Forms.Button chooseTempPathBtn;
        private System.Windows.Forms.Button chooseDestPathBtn;
        internal System.Windows.Forms.Button applySettingsBtn;
        private System.Windows.Forms.ComboBox allowOverwritingCombo;
        private System.Windows.Forms.Label label9;
    }
}
