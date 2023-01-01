
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
            this.libraryPanel1 = new DAZ_Installer.LibraryPanel();
            this.sortByCombo = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // searchBox
            // 
            this.searchBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.searchBox.Location = new System.Drawing.Point(394, 32);
            this.searchBox.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.searchBox.Name = "searchBox";
            this.searchBox.PlaceholderText = "Search";
            this.searchBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.searchBox.Size = new System.Drawing.Size(117, 23);
            this.searchBox.TabIndex = 0;
            this.searchBox.WordWrap = false;
            this.searchBox.TextChanged += new System.EventHandler(this.searchBox_TextChanged);
            this.searchBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.searchBox_KeyDown);
            // 
            // titleLbl
            // 
            this.titleLbl.AutoSize = true;
            this.titleLbl.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 17.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.titleLbl.Location = new System.Drawing.Point(34, 22);
            this.titleLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.titleLbl.Name = "titleLbl";
            this.titleLbl.Size = new System.Drawing.Size(80, 31);
            this.titleLbl.TabIndex = 1;
            this.titleLbl.Text = "Library";
            // 
            // thumbnails
            // 
            this.thumbnails.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            this.thumbnails.ImageSize = new System.Drawing.Size(125, 119);
            this.thumbnails.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // libraryPanel1
            // 
            this.libraryPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.libraryPanel1.BackColor = System.Drawing.Color.White;
            this.libraryPanel1.Location = new System.Drawing.Point(34, 71);
            this.libraryPanel1.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.libraryPanel1.Name = "libraryPanel1";
            this.libraryPanel1.Size = new System.Drawing.Size(478, 254);
            this.libraryPanel1.TabIndex = 2;
            // 
            // sortByCombo
            // 
            this.sortByCombo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.sortByCombo.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            this.sortByCombo.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.sortByCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortByCombo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.sortByCombo.FormattingEnabled = true;
            this.sortByCombo.Location = new System.Drawing.Point(292, 32);
            this.sortByCombo.Name = "sortByCombo";
            this.sortByCombo.Size = new System.Drawing.Size(95, 23);
            this.sortByCombo.TabIndex = 4;
            this.sortByCombo.SelectedIndexChanged += new System.EventHandler(this.sortByCombo_SelectedIndexChanged);
            // 
            // Library
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.sortByCombo);
            this.Controls.Add(this.libraryPanel1);
            this.Controls.Add(this.titleLbl);
            this.Controls.Add(this.searchBox);
            this.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.Name = "Library";
            this.Size = new System.Drawing.Size(542, 344);
            this.Load += new System.EventHandler(this.Library_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Label titleLbl;
        private System.Windows.Forms.ImageList thumbnails;
        private LibraryPanel libraryPanel1;
        private System.Windows.Forms.ComboBox sortByCombo;
    }
}
