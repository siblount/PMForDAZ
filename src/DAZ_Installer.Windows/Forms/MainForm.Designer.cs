using DAZ_Installer.Windows.Pages;

namespace DAZ_Installer.Windows.Forms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            homeLabel = new System.Windows.Forms.Label();
            extractLbl = new System.Windows.Forms.Label();
            libraryLbl = new System.Windows.Forms.Label();
            settingsLbl = new System.Windows.Forms.Label();
            mainPanel = new System.Windows.Forms.Panel();
            homePage1 = new Home();
            extractControl1 = new Extract();
            library1 = new Library();
            settings1 = new Settings();
            openFileDialog = new System.Windows.Forms.OpenFileDialog();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            mainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(53, 50, 56);
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(pictureBox1, 0, 0);
            tableLayoutPanel1.Controls.Add(homeLabel, 0, 1);
            tableLayoutPanel1.Controls.Add(extractLbl, 0, 2);
            tableLayoutPanel1.Controls.Add(libraryLbl, 0, 3);
            tableLayoutPanel1.Controls.Add(settingsLbl, 0, 4);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 5;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            tableLayoutPanel1.Size = new System.Drawing.Size(117, 344);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = System.Drawing.Color.FromArgb(53, 50, 56);
            pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            pictureBox1.Image = Resources.Logo2_256x;
            pictureBox1.Location = new System.Drawing.Point(3, 2);
            pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(111, 64);
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // homeLabel
            // 
            homeLabel.AutoSize = true;
            homeLabel.BackColor = System.Drawing.Color.FromArgb(53, 50, 56);
            homeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            homeLabel.Font = new System.Drawing.Font("Segoe UI Variable Text Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            homeLabel.ForeColor = System.Drawing.Color.White;
            homeLabel.Location = new System.Drawing.Point(3, 68);
            homeLabel.Name = "homeLabel";
            homeLabel.Size = new System.Drawing.Size(111, 68);
            homeLabel.TabIndex = 1;
            homeLabel.Text = "Home";
            homeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            homeLabel.Click += homeLabel_Click;
            homeLabel.MouseEnter += sidePanelButtonMouseEnter;
            homeLabel.MouseLeave += sidePanelButtonMouseExit;
            // 
            // extractLbl
            // 
            extractLbl.AutoSize = true;
            extractLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            extractLbl.Font = new System.Drawing.Font("Segoe UI Variable Text Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            extractLbl.ForeColor = System.Drawing.Color.White;
            extractLbl.Location = new System.Drawing.Point(3, 136);
            extractLbl.Name = "extractLbl";
            extractLbl.Size = new System.Drawing.Size(111, 68);
            extractLbl.TabIndex = 2;
            extractLbl.Text = "Extract";
            extractLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            extractLbl.Click += extractLbl_Click;
            extractLbl.MouseEnter += sidePanelButtonMouseEnter;
            extractLbl.MouseLeave += sidePanelButtonMouseExit;
            // 
            // libraryLbl
            // 
            libraryLbl.AutoSize = true;
            libraryLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            libraryLbl.Font = new System.Drawing.Font("Segoe UI Variable Text Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            libraryLbl.ForeColor = System.Drawing.Color.White;
            libraryLbl.Location = new System.Drawing.Point(3, 204);
            libraryLbl.Name = "libraryLbl";
            libraryLbl.Size = new System.Drawing.Size(111, 68);
            libraryLbl.TabIndex = 3;
            libraryLbl.Text = "Library";
            libraryLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            libraryLbl.Click += libraryLbl_Click;
            libraryLbl.MouseEnter += sidePanelButtonMouseEnter;
            libraryLbl.MouseLeave += sidePanelButtonMouseExit;
            // 
            // settingsLbl
            // 
            settingsLbl.AutoSize = true;
            settingsLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            settingsLbl.Font = new System.Drawing.Font("Segoe UI Variable Text Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            settingsLbl.ForeColor = System.Drawing.Color.White;
            settingsLbl.Location = new System.Drawing.Point(3, 272);
            settingsLbl.Name = "settingsLbl";
            settingsLbl.Size = new System.Drawing.Size(111, 72);
            settingsLbl.TabIndex = 4;
            settingsLbl.Text = "Settings";
            settingsLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            settingsLbl.Click += settingsLbl_Click;
            settingsLbl.MouseEnter += sidePanelButtonMouseEnter;
            settingsLbl.MouseLeave += sidePanelButtonMouseExit;
            // 
            // mainPanel
            // 
            mainPanel.Controls.Add(homePage1);
            mainPanel.Controls.Add(extractControl1);
            mainPanel.Controls.Add(library1);
            mainPanel.Controls.Add(settings1);
            mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            mainPanel.Location = new System.Drawing.Point(117, 0);
            mainPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            mainPanel.Name = "mainPanel";
            mainPanel.Size = new System.Drawing.Size(542, 344);
            mainPanel.TabIndex = 1;
            // 
            // homePage1
            // 
            homePage1.AllowDrop = true;
            homePage1.AutoSize = true;
            homePage1.BackColor = System.Drawing.Color.White;
            homePage1.Dock = System.Windows.Forms.DockStyle.Fill;
            homePage1.Location = new System.Drawing.Point(0, 0);
            homePage1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            homePage1.MinimumSize = new System.Drawing.Size(494, 294);
            homePage1.Name = "homePage1";
            homePage1.Size = new System.Drawing.Size(542, 344);
            homePage1.TabIndex = 0;
            // 
            // extractControl1
            // 
            extractControl1.BackColor = System.Drawing.Color.White;
            extractControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            extractControl1.Location = new System.Drawing.Point(0, 0);
            extractControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            extractControl1.Name = "extractControl1";
            extractControl1.Size = new System.Drawing.Size(542, 344);
            extractControl1.TabIndex = 1;
            // 
            // library1
            // 
            library1.BackColor = System.Drawing.Color.White;
            library1.Dock = System.Windows.Forms.DockStyle.Fill;
            library1.Location = new System.Drawing.Point(0, 0);
            library1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            library1.Name = "library1";
            library1.Size = new System.Drawing.Size(542, 344);
            library1.TabIndex = 2;
            // 
            // settings1
            // 
            settings1.BackColor = System.Drawing.Color.FromArgb(192, 255, 192);
            settings1.Dock = System.Windows.Forms.DockStyle.Fill;
            settings1.Location = new System.Drawing.Point(0, 0);
            settings1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            settings1.Name = "settings1";
            settings1.Size = new System.Drawing.Size(542, 344);
            settings1.TabIndex = 2;
            // 
            // openFileDialog
            // 
            openFileDialog.FileName = "openFileDialog1";
            openFileDialog.SupportMultiDottedExtensions = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            ClientSize = new System.Drawing.Size(659, 344);
            Controls.Add(mainPanel);
            Controls.Add(tableLayoutPanel1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            MinimumSize = new System.Drawing.Size(675, 383);
            Name = "MainForm";
            Text = "Product Manager for DAZ Studio";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label homeLabel;
        private System.Windows.Forms.Panel mainPanel;
        private Home homePage1;
        private System.Windows.Forms.Label extractLbl;
        private System.Windows.Forms.Label libraryLbl;
        private System.Windows.Forms.Label settingsLbl;
        public Extract extractControl1;
        private Library library1;
        private Settings settings1;
        internal System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}

