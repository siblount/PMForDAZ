
namespace DAZ_Installer
{
    partial class Library
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
            this.components = new System.ComponentModel.Container();
            this.searchBox = new System.Windows.Forms.TextBox();
            this.titleLbl = new System.Windows.Forms.Label();
            this.thumbnails = new System.Windows.Forms.ImageList(this.components);
            this.arrows = new System.Windows.Forms.ImageList(this.components);
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.libraryPanel1 = new DAZ_Installer.LibraryPanel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.loadStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.stripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.Location = new System.Drawing.Point(387, 26);
            this.searchBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.searchBox.Name = "searchBox";
            this.searchBox.PlaceholderText = "Search";
            this.searchBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.searchBox.Size = new System.Drawing.Size(129, 23);
            this.searchBox.TabIndex = 0;
            this.searchBox.WordWrap = false;
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchBox_KeyDown);
            // 
            // titleLbl
            // 
            this.titleLbl.AutoSize = true;
            this.titleLbl.Font = new System.Drawing.Font("Segoe UI", 16.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.titleLbl.Location = new System.Drawing.Point(27, 18);
            this.titleLbl.Name = "titleLbl";
            this.titleLbl.Size = new System.Drawing.Size(79, 30);
            this.titleLbl.TabIndex = 1;
            this.titleLbl.Text = "Library";
            // 
            // thumbnails
            // 
            this.thumbnails.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.thumbnails.ImageSize = new System.Drawing.Size(125, 119);
            this.thumbnails.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // arrows
            // 
            this.arrows.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.arrows.ImageSize = new System.Drawing.Size(22, 22);
            this.arrows.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // libraryPanel1
            // 
            this.libraryPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.libraryPanel1.BackColor = System.Drawing.Color.White;
            this.libraryPanel1.Location = new System.Drawing.Point(27, 57);
            this.libraryPanel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.libraryPanel1.Name = "libraryPanel1";
            this.libraryPanel1.Size = new System.Drawing.Size(489, 262);
            this.libraryPanel1.TabIndex = 2;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadStripStatusLabel,
            this.stripProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 312);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 12, 0);
            this.statusStrip1.Size = new System.Drawing.Size(540, 22);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // loadStripStatusLabel
            // 
            this.loadStripStatusLabel.Name = "loadStripStatusLabel";
            this.loadStripStatusLabel.Size = new System.Drawing.Size(91, 17);
            this.loadStripStatusLabel.Text = "Loading items...";
            this.loadStripStatusLabel.Click += new System.EventHandler(this.toolStripStatusLabel1_Click);
            // 
            // stripProgressBar
            // 
            this.stripProgressBar.Name = "stripProgressBar";
            this.stripProgressBar.Size = new System.Drawing.Size(88, 16);
            // 
            // Library
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.libraryPanel1);
            this.Controls.Add(this.titleLbl);
            this.Controls.Add(this.searchBox);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Library";
            this.Size = new System.Drawing.Size(540, 334);
            this.Load += new System.EventHandler(this.Library_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Label titleLbl;
        private System.Windows.Forms.ImageList thumbnails;
        private System.Windows.Forms.ImageList arrows;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private LibraryPanel libraryPanel1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        internal System.Windows.Forms.ToolStripStatusLabel loadStripStatusLabel;
        internal System.Windows.Forms.ToolStripProgressBar stripProgressBar;
    }
}
