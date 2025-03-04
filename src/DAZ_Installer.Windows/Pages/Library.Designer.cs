
namespace DAZ_Installer.Windows.Pages
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
            components = new System.ComponentModel.Container();
            searchBox = new System.Windows.Forms.TextBox();
            titleLbl = new System.Windows.Forms.Label();
            thumbnails = new System.Windows.Forms.ImageList(components);
            libraryPanel1 = new LibraryPanel();
            sortByCombo = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // searchBox
            // 
            searchBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            searchBox.Location = new System.Drawing.Point(394, 32);
            searchBox.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            searchBox.Name = "searchBox";
            searchBox.PlaceholderText = "Search";
            searchBox.RightToLeft = System.Windows.Forms.RightToLeft.No;
            searchBox.Size = new System.Drawing.Size(117, 23);
            searchBox.TabIndex = 0;
            searchBox.WordWrap = false;
            searchBox.TextChanged += searchBox_TextChanged;
            searchBox.KeyDown += searchBox_KeyDown;
            // 
            // titleLbl
            // 
            titleLbl.AutoSize = true;
            titleLbl.Font = new System.Drawing.Font("Segoe UI Variable Display Semil", 17.25F);
            titleLbl.Location = new System.Drawing.Point(34, 22);
            titleLbl.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            titleLbl.Name = "titleLbl";
            titleLbl.Size = new System.Drawing.Size(80, 31);
            titleLbl.TabIndex = 1;
            titleLbl.Text = "Library";
            // 
            // thumbnails
            // 
            thumbnails.ColorDepth = System.Windows.Forms.ColorDepth.Depth16Bit;
            thumbnails.ImageSize = new System.Drawing.Size(125, 119);
            thumbnails.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // libraryPanel1
            // 
            libraryPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            libraryPanel1.BackColor = System.Drawing.Color.White;
            libraryPanel1.Location = new System.Drawing.Point(34, 71);
            libraryPanel1.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            libraryPanel1.Name = "libraryPanel1";
            libraryPanel1.Size = new System.Drawing.Size(478, 254);
            libraryPanel1.TabIndex = 2;
            // 
            // sortByCombo
            // 
            sortByCombo.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            sortByCombo.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
            sortByCombo.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            sortByCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            sortByCombo.Font = new System.Drawing.Font("Segoe UI", 9F);
            sortByCombo.FormattingEnabled = true;
            sortByCombo.Location = new System.Drawing.Point(292, 32);
            sortByCombo.Name = "sortByCombo";
            sortByCombo.Size = new System.Drawing.Size(95, 23);
            sortByCombo.TabIndex = 4;
            sortByCombo.SelectedIndexChanged += sortByCombo_SelectedIndexChanged;
            // 
            // Library
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            BackColor = System.Drawing.Color.White;
            Controls.Add(sortByCombo);
            Controls.Add(libraryPanel1);
            Controls.Add(titleLbl);
            Controls.Add(searchBox);
            DoubleBuffered = true;
            Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            Name = "Library";
            Size = new System.Drawing.Size(542, 344);
            Load += Library_Load;
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox searchBox;
        private System.Windows.Forms.Label titleLbl;
        private System.Windows.Forms.ImageList thumbnails;
        private LibraryPanel libraryPanel1;
        private System.Windows.Forms.ComboBox sortByCombo;
    }
}
