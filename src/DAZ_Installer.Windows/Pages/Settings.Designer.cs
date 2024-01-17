
namespace DAZ_Installer.Windows.Pages
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
            titleLbl = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            destinationPathCombo = new System.Windows.Forms.ComboBox();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            downloadThumbnailsComboBox = new System.Windows.Forms.ComboBox();
            fileHandlingCombo = new System.Windows.Forms.ComboBox();
            panel1 = new System.Windows.Forms.Panel();
            openDatabaseBtn = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            removeActionCombo = new System.Windows.Forms.ComboBox();
            label10 = new System.Windows.Forms.Label();
            allowOverwritingCombo = new System.Windows.Forms.ComboBox();
            label9 = new System.Windows.Forms.Label();
            chooseTempPathBtn = new System.Windows.Forms.Button();
            chooseDestPathBtn = new System.Windows.Forms.Button();
            installPrevProductsCombo = new System.Windows.Forms.ComboBox();
            label8 = new System.Windows.Forms.Label();
            removeSourceFilesCombo = new System.Windows.Forms.ComboBox();
            label7 = new System.Windows.Forms.Label();
            modifyContentRedirectsBtn = new System.Windows.Forms.Button();
            modifyContentFoldersBtn = new System.Windows.Forms.Button();
            tempTxtBox = new System.Windows.Forms.TextBox();
            contentFolderRedirectsListBox = new System.Windows.Forms.ListBox();
            contentFoldersListBox = new System.Windows.Forms.ListBox();
            label5 = new System.Windows.Forms.Label();
            loadingPanel = new System.Windows.Forms.Panel();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            loadingLbl = new System.Windows.Forms.Label();
            applySettingsBtn = new System.Windows.Forms.Button();
            panel1.SuspendLayout();
            loadingPanel.SuspendLayout();
            SuspendLayout();
            // 
            // titleLbl
            // 
            titleLbl.AutoSize = true;
            titleLbl.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 17.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            titleLbl.Location = new System.Drawing.Point(27, 18);
            titleLbl.Name = "titleLbl";
            titleLbl.Size = new System.Drawing.Size(93, 31);
            titleLbl.TabIndex = 2;
            titleLbl.Text = "Settings";
            titleLbl.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label6.Location = new System.Drawing.Point(45, 62);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(106, 19);
            label6.TabIndex = 10;
            label6.Text = "Temporary Path";
            // 
            // label4
            // 
            label4.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label4.Location = new System.Drawing.Point(1, 176);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(150, 46);
            label4.TabIndex = 6;
            label4.Text = "Content Folder Redirects";
            label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label3.Location = new System.Drawing.Point(11, 274);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(141, 19);
            label3.TabIndex = 4;
            label3.Text = "File Handling Method";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // destinationPathCombo
            // 
            destinationPathCombo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            destinationPathCombo.FormattingEnabled = true;
            destinationPathCombo.Location = new System.Drawing.Point(187, 35);
            destinationPathCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            destinationPathCombo.Name = "destinationPathCombo";
            destinationPathCombo.Size = new System.Drawing.Size(209, 23);
            destinationPathCombo.TabIndex = 3;
            destinationPathCombo.TextChanged += destinationPathCombo_TextChanged;
            destinationPathCombo.Leave += destinationPathCombo_Leave;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label2.Location = new System.Drawing.Point(38, 36);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(111, 19);
            label2.TabIndex = 2;
            label2.Text = "Destination Path";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(7, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(143, 19);
            label1.TabIndex = 0;
            label1.Text = "Download thumbnails";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // downloadThumbnailsComboBox
            // 
            downloadThumbnailsComboBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            downloadThumbnailsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            downloadThumbnailsComboBox.FormattingEnabled = true;
            downloadThumbnailsComboBox.Location = new System.Drawing.Point(187, 8);
            downloadThumbnailsComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            downloadThumbnailsComboBox.Name = "downloadThumbnailsComboBox";
            downloadThumbnailsComboBox.Size = new System.Drawing.Size(244, 23);
            downloadThumbnailsComboBox.TabIndex = 1;
            downloadThumbnailsComboBox.TextChanged += downloadThumbnailsComboBox_TextChanged;
            // 
            // fileHandlingCombo
            // 
            fileHandlingCombo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            fileHandlingCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            fileHandlingCombo.FormattingEnabled = true;
            fileHandlingCombo.Location = new System.Drawing.Point(187, 274);
            fileHandlingCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            fileHandlingCombo.Name = "fileHandlingCombo";
            fileHandlingCombo.Size = new System.Drawing.Size(244, 23);
            fileHandlingCombo.TabIndex = 5;
            fileHandlingCombo.TextChanged += fileHandlingCombo_TextChanged;
            // 
            // panel1
            // 
            panel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            panel1.AutoScroll = true;
            panel1.Controls.Add(openDatabaseBtn);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(removeActionCombo);
            panel1.Controls.Add(label10);
            panel1.Controls.Add(allowOverwritingCombo);
            panel1.Controls.Add(label9);
            panel1.Controls.Add(chooseTempPathBtn);
            panel1.Controls.Add(chooseDestPathBtn);
            panel1.Controls.Add(installPrevProductsCombo);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(removeSourceFilesCombo);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(modifyContentRedirectsBtn);
            panel1.Controls.Add(modifyContentFoldersBtn);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(tempTxtBox);
            panel1.Controls.Add(contentFolderRedirectsListBox);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(fileHandlingCombo);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(contentFoldersListBox);
            panel1.Controls.Add(destinationPathCombo);
            panel1.Controls.Add(downloadThumbnailsComboBox);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label5);
            panel1.Location = new System.Drawing.Point(27, 49);
            panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(503, 685);
            panel1.TabIndex = 7;
            // 
            // openDatabaseBtn
            // 
            openDatabaseBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            openDatabaseBtn.Location = new System.Drawing.Point(186, 475);
            openDatabaseBtn.Name = "openDatabaseBtn";
            openDatabaseBtn.Size = new System.Drawing.Size(244, 23);
            openDatabaseBtn.TabIndex = 25;
            openDatabaseBtn.Text = "Open Database";
            openDatabaseBtn.UseVisualStyleBackColor = true;
            openDatabaseBtn.Click += openDatabaseBtn_Click;
            // 
            // button2
            // 
            button2.Location = new System.Drawing.Point(187, 515);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(100, 23);
            button2.TabIndex = 24;
            button2.Text = "Documentation";
            button2.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(33, 515);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(75, 23);
            button1.TabIndex = 23;
            button1.Text = "Licenses";
            button1.UseVisualStyleBackColor = true;
            // 
            // removeActionCombo
            // 
            removeActionCombo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            removeActionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            removeActionCombo.FormattingEnabled = true;
            removeActionCombo.Location = new System.Drawing.Point(187, 434);
            removeActionCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            removeActionCombo.Name = "removeActionCombo";
            removeActionCombo.Size = new System.Drawing.Size(243, 23);
            removeActionCombo.TabIndex = 22;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label10.Location = new System.Drawing.Point(29, 438);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(123, 19);
            label10.TabIndex = 21;
            label10.Text = "I/O Remove action";
            label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // allowOverwritingCombo
            // 
            allowOverwritingCombo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            allowOverwritingCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            allowOverwritingCombo.FormattingEnabled = true;
            allowOverwritingCombo.Location = new System.Drawing.Point(187, 398);
            allowOverwritingCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            allowOverwritingCombo.Name = "allowOverwritingCombo";
            allowOverwritingCombo.Size = new System.Drawing.Size(243, 23);
            allowOverwritingCombo.TabIndex = 20;
            allowOverwritingCombo.TextChanged += allowOverwritingCombo_TextChanged;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label9.Location = new System.Drawing.Point(36, 398);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(115, 19);
            label9.TabIndex = 19;
            label9.Text = "Allow overwriting";
            label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chooseTempPathBtn
            // 
            chooseTempPathBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            chooseTempPathBtn.Location = new System.Drawing.Point(401, 59);
            chooseTempPathBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            chooseTempPathBtn.Name = "chooseTempPathBtn";
            chooseTempPathBtn.Size = new System.Drawing.Size(30, 20);
            chooseTempPathBtn.TabIndex = 18;
            chooseTempPathBtn.Text = "...";
            chooseTempPathBtn.UseVisualStyleBackColor = true;
            chooseTempPathBtn.Click += chooseTempPathBtn_Click;
            // 
            // chooseDestPathBtn
            // 
            chooseDestPathBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            chooseDestPathBtn.Location = new System.Drawing.Point(401, 35);
            chooseDestPathBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            chooseDestPathBtn.Name = "chooseDestPathBtn";
            chooseDestPathBtn.Size = new System.Drawing.Size(30, 21);
            chooseDestPathBtn.TabIndex = 17;
            chooseDestPathBtn.Text = "...";
            chooseDestPathBtn.UseVisualStyleBackColor = true;
            chooseDestPathBtn.Click += chooseDestPathBtn_Click;
            // 
            // installPrevProductsCombo
            // 
            installPrevProductsCombo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            installPrevProductsCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            installPrevProductsCombo.FormattingEnabled = true;
            installPrevProductsCombo.Location = new System.Drawing.Point(188, 355);
            installPrevProductsCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            installPrevProductsCombo.Name = "installPrevProductsCombo";
            installPrevProductsCombo.Size = new System.Drawing.Size(243, 23);
            installPrevProductsCombo.TabIndex = 16;
            installPrevProductsCombo.TextChanged += installPrevProducts_TextChanged;
            // 
            // label8
            // 
            label8.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label8.Location = new System.Drawing.Point(11, 345);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(141, 38);
            label8.TabIndex = 15;
            label8.Text = "Install previously installed products";
            label8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // removeSourceFilesCombo
            // 
            removeSourceFilesCombo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            removeSourceFilesCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            removeSourceFilesCombo.FormattingEnabled = true;
            removeSourceFilesCombo.Location = new System.Drawing.Point(187, 311);
            removeSourceFilesCombo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            removeSourceFilesCombo.Name = "removeSourceFilesCombo";
            removeSourceFilesCombo.Size = new System.Drawing.Size(243, 23);
            removeSourceFilesCombo.TabIndex = 14;
            removeSourceFilesCombo.TextChanged += removeSourceFiles_TextChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label7.Location = new System.Drawing.Point(11, 311);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(141, 19);
            label7.TabIndex = 13;
            label7.Text = "Remove Source File(s)";
            label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // modifyContentRedirectsBtn
            // 
            modifyContentRedirectsBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            modifyContentRedirectsBtn.Location = new System.Drawing.Point(372, 248);
            modifyContentRedirectsBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            modifyContentRedirectsBtn.Name = "modifyContentRedirectsBtn";
            modifyContentRedirectsBtn.Size = new System.Drawing.Size(59, 22);
            modifyContentRedirectsBtn.TabIndex = 12;
            modifyContentRedirectsBtn.Text = "Modify";
            modifyContentRedirectsBtn.UseVisualStyleBackColor = true;
            modifyContentRedirectsBtn.Click += modifyContentRedirectsBtn_Click_1;
            // 
            // modifyContentFoldersBtn
            // 
            modifyContentFoldersBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            modifyContentFoldersBtn.Location = new System.Drawing.Point(372, 154);
            modifyContentFoldersBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            modifyContentFoldersBtn.Name = "modifyContentFoldersBtn";
            modifyContentFoldersBtn.Size = new System.Drawing.Size(59, 22);
            modifyContentFoldersBtn.TabIndex = 1;
            modifyContentFoldersBtn.Text = "Modify";
            modifyContentFoldersBtn.UseVisualStyleBackColor = true;
            modifyContentFoldersBtn.Click += modifyContentFoldersBtn_Click;
            // 
            // tempTxtBox
            // 
            tempTxtBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tempTxtBox.Location = new System.Drawing.Point(187, 59);
            tempTxtBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tempTxtBox.Name = "tempTxtBox";
            tempTxtBox.Size = new System.Drawing.Size(209, 23);
            tempTxtBox.TabIndex = 11;
            tempTxtBox.KeyUp += tempTxtBox_KeyUp;
            tempTxtBox.Leave += tempTxtBox_Leave;
            // 
            // contentFolderRedirectsListBox
            // 
            contentFolderRedirectsListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            contentFolderRedirectsListBox.FormattingEnabled = true;
            contentFolderRedirectsListBox.ItemHeight = 15;
            contentFolderRedirectsListBox.Location = new System.Drawing.Point(187, 180);
            contentFolderRedirectsListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            contentFolderRedirectsListBox.Name = "contentFolderRedirectsListBox";
            contentFolderRedirectsListBox.Size = new System.Drawing.Size(244, 64);
            contentFolderRedirectsListBox.TabIndex = 0;
            // 
            // contentFoldersListBox
            // 
            contentFoldersListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            contentFoldersListBox.FormattingEnabled = true;
            contentFoldersListBox.ItemHeight = 15;
            contentFoldersListBox.Location = new System.Drawing.Point(187, 86);
            contentFoldersListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            contentFoldersListBox.Name = "contentFoldersListBox";
            contentFoldersListBox.Size = new System.Drawing.Size(244, 64);
            contentFoldersListBox.TabIndex = 1;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label5.Location = new System.Drawing.Point(45, 86);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(107, 19);
            label5.TabIndex = 8;
            label5.Text = "Content Folders";
            label5.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // loadingPanel
            // 
            loadingPanel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            loadingPanel.Controls.Add(progressBar1);
            loadingPanel.Controls.Add(loadingLbl);
            loadingPanel.Location = new System.Drawing.Point(0, 49);
            loadingPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            loadingPanel.Name = "loadingPanel";
            loadingPanel.Size = new System.Drawing.Size(541, 724);
            loadingPanel.TabIndex = 4;
            loadingPanel.Visible = false;
            // 
            // progressBar1
            // 
            progressBar1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            progressBar1.Location = new System.Drawing.Point(60, 204);
            progressBar1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            progressBar1.MarqueeAnimationSpeed = 25;
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(423, 429);
            progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            progressBar1.TabIndex = 1;
            progressBar1.Value = 5;
            // 
            // loadingLbl
            // 
            loadingLbl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            loadingLbl.AutoSize = true;
            loadingLbl.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            loadingLbl.Location = new System.Drawing.Point(150, 130);
            loadingLbl.Name = "loadingLbl";
            loadingLbl.Size = new System.Drawing.Size(228, 65);
            loadingLbl.TabIndex = 0;
            loadingLbl.Text = "Loading...";
            // 
            // applySettingsBtn
            // 
            applySettingsBtn.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            applySettingsBtn.Enabled = false;
            applySettingsBtn.Location = new System.Drawing.Point(400, 738);
            applySettingsBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            applySettingsBtn.Name = "applySettingsBtn";
            applySettingsBtn.Size = new System.Drawing.Size(130, 24);
            applySettingsBtn.TabIndex = 2;
            applySettingsBtn.Text = "Apply Changes";
            applySettingsBtn.UseVisualStyleBackColor = true;
            applySettingsBtn.Click += applySettingsBtn_Click;
            // 
            // Settings
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            BackColor = System.Drawing.Color.FromArgb(192, 255, 192);
            Controls.Add(panel1);
            Controls.Add(applySettingsBtn);
            Controls.Add(titleLbl);
            Controls.Add(loadingPanel);
            Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            Name = "Settings";
            Size = new System.Drawing.Size(542, 773);
            Load += Settings_Load;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            loadingPanel.ResumeLayout(false);
            loadingPanel.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.ComboBox removeActionCombo;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button openDatabaseBtn;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
    }
}
