
namespace DAZ_Installer.Windows.Pages
{
    partial class Home
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
            components = new System.ComponentModel.Container();
            titleLbl = new System.Windows.Forms.Label();
            extractBtn = new System.Windows.Forms.Button();
            addMoreFilesBtn = new System.Windows.Forms.Button();
            clearListBtn = new System.Windows.Forms.Button();
            listView1 = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            homeListContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addMoreItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            dropBtn = new System.Windows.Forms.Button();
            homeListContextMenuStrip.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // titleLbl
            // 
            titleLbl.BackColor = System.Drawing.Color.White;
            titleLbl.Dock = System.Windows.Forms.DockStyle.Top;
            titleLbl.Font = new System.Drawing.Font("Segoe UI Variable Text Light", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            titleLbl.ForeColor = System.Drawing.Color.FromArgb(31, 31, 31);
            titleLbl.Location = new System.Drawing.Point(0, 0);
            titleLbl.Name = "titleLbl";
            titleLbl.Size = new System.Drawing.Size(542, 55);
            titleLbl.TabIndex = 0;
            titleLbl.Text = "Product Manager for DAZ Studio";
            titleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // extractBtn
            // 
            extractBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            extractBtn.Location = new System.Drawing.Point(130, 208);
            extractBtn.Name = "extractBtn";
            extractBtn.Size = new System.Drawing.Size(248, 38);
            extractBtn.TabIndex = 2;
            extractBtn.Text = "Extract File(s)";
            extractBtn.UseVisualStyleBackColor = true;
            extractBtn.Click += button1_Click;
            // 
            // addMoreFilesBtn
            // 
            addMoreFilesBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            addMoreFilesBtn.Location = new System.Drawing.Point(3, 208);
            addMoreFilesBtn.Name = "addMoreFilesBtn";
            addMoreFilesBtn.Size = new System.Drawing.Size(121, 38);
            addMoreFilesBtn.TabIndex = 4;
            addMoreFilesBtn.Text = "Add more files";
            addMoreFilesBtn.UseVisualStyleBackColor = true;
            addMoreFilesBtn.Click += addMoreFilesBtn_Click;
            // 
            // clearListBtn
            // 
            clearListBtn.Dock = System.Windows.Forms.DockStyle.Fill;
            clearListBtn.Location = new System.Drawing.Point(384, 208);
            clearListBtn.Name = "clearListBtn";
            clearListBtn.Size = new System.Drawing.Size(122, 38);
            clearListBtn.TabIndex = 3;
            clearListBtn.Text = "Clear List";
            clearListBtn.UseVisualStyleBackColor = true;
            clearListBtn.Click += clearListBtn_Click;
            // 
            // listView1
            // 
            listView1.AllowDrop = true;
            listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1 });
            tableLayoutPanel1.SetColumnSpan(listView1, 3);
            listView1.ContextMenuStrip = homeListContextMenuStrip;
            listView1.Dock = System.Windows.Forms.DockStyle.Fill;
            listView1.ForeColor = System.Drawing.SystemColors.WindowText;
            listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listView1.Location = new System.Drawing.Point(3, 3);
            listView1.Name = "listView1";
            listView1.Size = new System.Drawing.Size(503, 199);
            listView1.TabIndex = 4;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "a";
            columnHeader1.Width = 550;
            // 
            // homeListContextMenuStrip
            // 
            homeListContextMenuStrip.DropShadowEnabled = false;
            homeListContextMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            homeListContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { removeToolStripMenuItem, addMoreItemsToolStripMenuItem });
            homeListContextMenuStrip.Name = "homeListContextMenuStrip";
            homeListContextMenuStrip.ShowImageMargin = false;
            homeListContextMenuStrip.Size = new System.Drawing.Size(144, 48);
            homeListContextMenuStrip.Opening += homeListContextMenuStrip_Opening;
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += removeToolStripMenuItem_Click;
            // 
            // addMoreItemsToolStripMenuItem
            // 
            addMoreItemsToolStripMenuItem.Name = "addMoreItemsToolStripMenuItem";
            addMoreItemsToolStripMenuItem.Size = new System.Drawing.Size(143, 22);
            addMoreItemsToolStripMenuItem.Text = "Add more items...";
            addMoreItemsToolStripMenuItem.Click += addMoreItemsToolStripMenuItem_Click;
            // 
            // openFileDialog1
            // 
            openFileDialog1.AddExtension = false;
            openFileDialog1.DefaultExt = "zip";
            openFileDialog1.Filter = "RAR files (*.rar)|*.rar|ZIP files (*.zip)|*.zip|7z files (*.7z)|*.7z|7z part file base(*.001)|*.001|All files (*.*)|*.*";
            openFileDialog1.Multiselect = true;
            openFileDialog1.SupportMultiDottedExtensions = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tableLayoutPanel1.Controls.Add(extractBtn, 1, 1);
            tableLayoutPanel1.Controls.Add(clearListBtn, 2, 1);
            tableLayoutPanel1.Controls.Add(addMoreFilesBtn, 0, 1);
            tableLayoutPanel1.Controls.Add(listView1, 0, 0);
            tableLayoutPanel1.Location = new System.Drawing.Point(18, 55);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            tableLayoutPanel1.Size = new System.Drawing.Size(509, 249);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // dropBtn
            // 
            dropBtn.AllowDrop = true;
            dropBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dropBtn.BackColor = System.Drawing.Color.FromArgb(192, 255, 192);
            dropBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            dropBtn.FlatAppearance.BorderSize = 0;
            dropBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            dropBtn.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dropBtn.ForeColor = System.Drawing.Color.FromArgb(32, 32, 32);
            dropBtn.Location = new System.Drawing.Point(18, 55);
            dropBtn.Name = "dropBtn";
            dropBtn.Size = new System.Drawing.Size(509, 249);
            dropBtn.TabIndex = 6;
            dropBtn.Text = "Click here to select file(s) or drag them here.";
            dropBtn.UseVisualStyleBackColor = false;
            dropBtn.Click += dropBtn_Click;
            dropBtn.DragEnter += dropBtn_DragEnter;
            dropBtn.DragLeave += dropBtn_DragLeave;
            // 
            // Home
            // 
            AllowDrop = true;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            AutoSize = true;
            BackColor = System.Drawing.Color.White;
            Controls.Add(dropBtn);
            Controls.Add(titleLbl);
            Controls.Add(tableLayoutPanel1);
            Name = "Home";
            Size = new System.Drawing.Size(542, 344);
            DragDrop += Home_DragDrop;
            DragEnter += Home_DragEnter;
            homeListContextMenuStrip.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label titleLbl;
        private System.Windows.Forms.Button extractBtn;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        internal System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ContextMenuStrip homeListContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addMoreItemsToolStripMenuItem;
        private System.Windows.Forms.Button addMoreFilesBtn;
        private System.Windows.Forms.Button clearListBtn;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button dropBtn;
    }
}
