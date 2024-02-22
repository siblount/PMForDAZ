namespace DAZ_Installer.Windows.Forms
{
    partial class DatabaseToolsForm
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
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseToolsForm));
            dataGrid = new System.Windows.Forms.DataGridView();
            tableLbl = new System.Windows.Forms.Label();
            tableNames = new System.Windows.Forms.ComboBox();
            changeTableBtn = new System.Windows.Forms.Button();
            backupDatabaseBtn = new System.Windows.Forms.Button();
            restoreDatabaseBtn = new System.Windows.Forms.Button();
            vacuumDatabaseBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)dataGrid).BeginInit();
            SuspendLayout();
            // 
            // dataGrid
            // 
            dataGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dataGrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGrid.Location = new System.Drawing.Point(0, 56);
            dataGrid.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            dataGrid.Name = "dataGrid";
            dataGrid.ReadOnly = true;
            dataGrid.RowHeadersWidth = 51;
            dataGrid.RowTemplate.Height = 29;
            dataGrid.Size = new System.Drawing.Size(700, 282);
            dataGrid.TabIndex = 0;
            // 
            // tableLbl
            // 
            tableLbl.Location = new System.Drawing.Point(0, 4);
            tableLbl.Name = "tableLbl";
            tableLbl.Size = new System.Drawing.Size(52, 19);
            tableLbl.TabIndex = 0;
            tableLbl.Text = "Table: ";
            tableLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableNames
            // 
            tableNames.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            tableNames.FormattingEnabled = true;
            tableNames.Location = new System.Drawing.Point(58, 2);
            tableNames.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            tableNames.Name = "tableNames";
            tableNames.Size = new System.Drawing.Size(482, 23);
            tableNames.TabIndex = 1;
            // 
            // changeTableBtn
            // 
            changeTableBtn.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            changeTableBtn.Location = new System.Drawing.Point(544, 2);
            changeTableBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            changeTableBtn.Name = "changeTableBtn";
            changeTableBtn.Size = new System.Drawing.Size(148, 22);
            changeTableBtn.TabIndex = 2;
            changeTableBtn.Text = "Change Table";
            changeTableBtn.UseVisualStyleBackColor = true;
            changeTableBtn.Click += changeTableBtn_Click;
            // 
            // backupDatabaseBtn
            // 
            backupDatabaseBtn.Location = new System.Drawing.Point(4, 28);
            backupDatabaseBtn.Name = "backupDatabaseBtn";
            backupDatabaseBtn.Size = new System.Drawing.Size(121, 23);
            backupDatabaseBtn.TabIndex = 3;
            backupDatabaseBtn.Text = "Backup Database";
            backupDatabaseBtn.UseVisualStyleBackColor = true;
            backupDatabaseBtn.Click += backupDatabaseBtn_Click;
            // 
            // restoreDatabaseBtn
            // 
            restoreDatabaseBtn.Location = new System.Drawing.Point(131, 28);
            restoreDatabaseBtn.Name = "restoreDatabaseBtn";
            restoreDatabaseBtn.Size = new System.Drawing.Size(121, 23);
            restoreDatabaseBtn.TabIndex = 4;
            restoreDatabaseBtn.Text = "Restore Database";
            restoreDatabaseBtn.UseVisualStyleBackColor = true;
            restoreDatabaseBtn.Click += restoreDatabaseBtn_Click;
            // 
            // vacuumDatabaseBtn
            // 
            vacuumDatabaseBtn.Location = new System.Drawing.Point(258, 28);
            vacuumDatabaseBtn.Name = "vacuumDatabaseBtn";
            vacuumDatabaseBtn.Size = new System.Drawing.Size(121, 23);
            vacuumDatabaseBtn.TabIndex = 5;
            vacuumDatabaseBtn.Text = "Vacuum Database";
            vacuumDatabaseBtn.UseVisualStyleBackColor = true;
            vacuumDatabaseBtn.Click += vacuumDatabaseBtn_Click;
            // 
            // DatabaseToolsForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(700, 338);
            Controls.Add(vacuumDatabaseBtn);
            Controls.Add(restoreDatabaseBtn);
            Controls.Add(backupDatabaseBtn);
            Controls.Add(changeTableBtn);
            Controls.Add(tableLbl);
            Controls.Add(tableNames);
            Controls.Add(dataGrid);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            Name = "DatabaseToolsForm";
            Text = "Database Tools";
            Load += DatabaseView_Load;
            ((System.ComponentModel.ISupportInitialize)dataGrid).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.DataGridView dataGrid;
        private System.Windows.Forms.Label tableLbl;
        private System.Windows.Forms.ComboBox tableNames;
        private System.Windows.Forms.Button changeTableBtn;
        private System.Windows.Forms.Button backupDatabaseBtn;
        private System.Windows.Forms.Button restoreDatabaseBtn;
        private System.Windows.Forms.Button vacuumDatabaseBtn;
    }
}