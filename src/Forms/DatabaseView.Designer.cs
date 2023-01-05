namespace DAZ_Installer.WinApp.Forms
{
    partial class DatabaseView
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseView));
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.tableLbl = new System.Windows.Forms.Label();
            this.tableNames = new System.Windows.Forms.ComboBox();
            this.changeTableBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGrid
            // 
            this.dataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Location = new System.Drawing.Point(0, 28);
            this.dataGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.ReadOnly = true;
            this.dataGrid.RowHeadersWidth = 51;
            this.dataGrid.RowTemplate.Height = 29;
            this.dataGrid.Size = new System.Drawing.Size(700, 310);
            this.dataGrid.TabIndex = 0;
            // 
            // tableLbl
            // 
            this.tableLbl.Location = new System.Drawing.Point(0, 4);
            this.tableLbl.Name = "tableLbl";
            this.tableLbl.Size = new System.Drawing.Size(52, 19);
            this.tableLbl.TabIndex = 0;
            this.tableLbl.Text = "Table: ";
            this.tableLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableNames
            // 
            this.tableNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableNames.FormattingEnabled = true;
            this.tableNames.Location = new System.Drawing.Point(58, 2);
            this.tableNames.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tableNames.Name = "tableNames";
            this.tableNames.Size = new System.Drawing.Size(482, 23);
            this.tableNames.TabIndex = 1;
            // 
            // changeTableBtn
            // 
            this.changeTableBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.changeTableBtn.Location = new System.Drawing.Point(544, 2);
            this.changeTableBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.changeTableBtn.Name = "changeTableBtn";
            this.changeTableBtn.Size = new System.Drawing.Size(148, 22);
            this.changeTableBtn.TabIndex = 2;
            this.changeTableBtn.Text = "Change Table";
            this.changeTableBtn.UseVisualStyleBackColor = true;
            this.changeTableBtn.Click += new System.EventHandler(this.changeTableBtn_Click);
            // 
            // DatabaseView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 338);
            this.Controls.Add(this.changeTableBtn);
            this.Controls.Add(this.tableLbl);
            this.Controls.Add(this.tableNames);
            this.Controls.Add(this.dataGrid);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "DatabaseView";
            this.Text = "Database Viewer";
            this.Load += new System.EventHandler(this.DatabaseView_Load);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataGridView dataGrid;
        internal System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.Label tableLbl;
        private System.Windows.Forms.ComboBox tableNames;
        private System.Windows.Forms.Button changeTableBtn;
    }
}