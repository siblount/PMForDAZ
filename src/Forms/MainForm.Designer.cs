﻿
namespace DAZ_Installer.WinApp.Forms
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.homeLabel = new System.Windows.Forms.Label();
            this.extractLbl = new System.Windows.Forms.Label();
            this.libraryLbl = new System.Windows.Forms.Label();
            this.settingsLbl = new System.Windows.Forms.Label();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.homePage1 = new DAZ_Installer.Home();
            this.extractControl1 = new DAZ_Installer.Extract();
            this.library1 = new DAZ_Installer.Library();
            this.settings1 = new DAZ_Installer.Settings();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.mainPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.homeLabel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.extractLbl, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.libraryLbl, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.settingsLbl, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(117, 344);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::DAZ_Installer.Properties.Resources.Logo2_256x;
            this.pictureBox1.Location = new System.Drawing.Point(3, 2);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(111, 64);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // homeLabel
            // 
            this.homeLabel.AutoSize = true;
            this.homeLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(53)))), ((int)(((byte)(50)))), ((int)(((byte)(56)))));
            this.homeLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.homeLabel.Font = new System.Drawing.Font("Segoe UI Variable Text Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.homeLabel.ForeColor = System.Drawing.Color.White;
            this.homeLabel.Location = new System.Drawing.Point(3, 68);
            this.homeLabel.Name = "homeLabel";
            this.homeLabel.Size = new System.Drawing.Size(111, 68);
            this.homeLabel.TabIndex = 1;
            this.homeLabel.Text = "Home";
            this.homeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.homeLabel.Click += new System.EventHandler(this.homeLabel_Click);
            this.homeLabel.MouseEnter += new System.EventHandler(this.sidePanelButtonMouseEnter);
            this.homeLabel.MouseLeave += new System.EventHandler(this.sidePanelButtonMouseExit);
            // 
            // extractLbl
            // 
            this.extractLbl.AutoSize = true;
            this.extractLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.extractLbl.Font = new System.Drawing.Font("Segoe UI Variable Text Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.extractLbl.ForeColor = System.Drawing.Color.White;
            this.extractLbl.Location = new System.Drawing.Point(3, 136);
            this.extractLbl.Name = "extractLbl";
            this.extractLbl.Size = new System.Drawing.Size(111, 68);
            this.extractLbl.TabIndex = 2;
            this.extractLbl.Text = "Extract";
            this.extractLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.extractLbl.Click += new System.EventHandler(this.extractLbl_Click);
            this.extractLbl.MouseEnter += new System.EventHandler(this.sidePanelButtonMouseEnter);
            this.extractLbl.MouseLeave += new System.EventHandler(this.sidePanelButtonMouseExit);
            // 
            // libraryLbl
            // 
            this.libraryLbl.AutoSize = true;
            this.libraryLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.libraryLbl.Font = new System.Drawing.Font("Segoe UI Variable Text Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.libraryLbl.ForeColor = System.Drawing.Color.White;
            this.libraryLbl.Location = new System.Drawing.Point(3, 204);
            this.libraryLbl.Name = "libraryLbl";
            this.libraryLbl.Size = new System.Drawing.Size(111, 68);
            this.libraryLbl.TabIndex = 3;
            this.libraryLbl.Text = "Library";
            this.libraryLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.libraryLbl.Click += new System.EventHandler(this.libraryLbl_Click);
            this.libraryLbl.MouseEnter += new System.EventHandler(this.sidePanelButtonMouseEnter);
            this.libraryLbl.MouseLeave += new System.EventHandler(this.sidePanelButtonMouseExit);
            // 
            // settingsLbl
            // 
            this.settingsLbl.AutoSize = true;
            this.settingsLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settingsLbl.Font = new System.Drawing.Font("Segoe UI Variable Text Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.settingsLbl.ForeColor = System.Drawing.Color.White;
            this.settingsLbl.Location = new System.Drawing.Point(3, 272);
            this.settingsLbl.Name = "settingsLbl";
            this.settingsLbl.Size = new System.Drawing.Size(111, 72);
            this.settingsLbl.TabIndex = 4;
            this.settingsLbl.Text = "Settings";
            this.settingsLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.settingsLbl.Click += new System.EventHandler(this.settingsLbl_Click);
            this.settingsLbl.MouseEnter += new System.EventHandler(this.sidePanelButtonMouseEnter);
            this.settingsLbl.MouseLeave += new System.EventHandler(this.sidePanelButtonMouseExit);
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.homePage1);
            this.mainPanel.Controls.Add(this.extractControl1);
            this.mainPanel.Controls.Add(this.library1);
            this.mainPanel.Controls.Add(this.settings1);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(117, 0);
            this.mainPanel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(542, 344);
            this.mainPanel.TabIndex = 1;
            // 
            // homePage1
            // 
            this.homePage1.AllowDrop = true;
            this.homePage1.AutoSize = true;
            this.homePage1.BackColor = System.Drawing.Color.White;
            this.homePage1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.homePage1.Location = new System.Drawing.Point(0, 0);
            this.homePage1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.homePage1.MinimumSize = new System.Drawing.Size(494, 294);
            this.homePage1.Name = "homePage1";
            this.homePage1.Size = new System.Drawing.Size(542, 344);
            this.homePage1.TabIndex = 0;
            // 
            // extractControl1
            // 
            this.extractControl1.BackColor = System.Drawing.Color.White;
            this.extractControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.extractControl1.Location = new System.Drawing.Point(0, 0);
            this.extractControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.extractControl1.Name = "extractControl1";
            this.extractControl1.Size = new System.Drawing.Size(542, 344);
            this.extractControl1.TabIndex = 1;
            // 
            // library1
            // 
            this.library1.BackColor = System.Drawing.Color.White;
            this.library1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.library1.Location = new System.Drawing.Point(0, 0);
            this.library1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.library1.Name = "library1";
            this.library1.Size = new System.Drawing.Size(542, 344);
            this.library1.TabIndex = 2;
            // 
            // settings1
            // 
            this.settings1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.settings1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.settings1.Location = new System.Drawing.Point(0, 0);
            this.settings1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.settings1.Name = "settings1";
            this.settings1.Size = new System.Drawing.Size(542, 344);
            this.settings1.TabIndex = 2;
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.SupportMultiDottedExtensions = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(659, 344);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(675, 383);
            this.Name = "MainForm";
            this.Text = "Daz Product Installer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.ResumeLayout(false);

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

