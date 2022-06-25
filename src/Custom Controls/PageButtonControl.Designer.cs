
namespace DAZ_Installer
{
    partial class PageButtonControl
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.gotoTxtBox = new System.Windows.Forms.TextBox();
            this.pageLbl = new System.Windows.Forms.Label();
            this.goBtn = new System.Windows.Forms.Button();
            this.goNextBtn = new System.Windows.Forms.Button();
            this.goPrevBtn = new System.Windows.Forms.Button();
            this.goFirstBtn = new System.Windows.Forms.Button();
            this.goLastBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.ColumnCount = 12;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.gotoTxtBox, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.pageLbl, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.goBtn, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.goNextBtn, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.goPrevBtn, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.goFirstBtn, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.goLastBtn, 4, 0);
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(421, 42);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // gotoTxtBox
            // 
            this.gotoTxtBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.gotoTxtBox.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.gotoTxtBox.Location = new System.Drawing.Point(255, 6);
            this.gotoTxtBox.MinimumSize = new System.Drawing.Size(50, 27);
            this.gotoTxtBox.Name = "gotoTxtBox";
            this.gotoTxtBox.PlaceholderText = "Goto";
            this.gotoTxtBox.Size = new System.Drawing.Size(97, 30);
            this.gotoTxtBox.TabIndex = 7;
            this.gotoTxtBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.gotoTxtBox_KeyPress);
            this.gotoTxtBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.gotoTxtBox_KeyUp);
            // 
            // pageLbl
            // 
            this.pageLbl.AutoSize = true;
            this.pageLbl.Dock = System.Windows.Forms.DockStyle.Left;
            this.pageLbl.Location = new System.Drawing.Point(86, 3);
            this.pageLbl.Name = "pageLbl";
            this.pageLbl.Size = new System.Drawing.Size(83, 36);
            this.pageLbl.TabIndex = 1;
            this.pageLbl.Text = "Page 0 of 0";
            this.pageLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // goBtn
            // 
            this.goBtn.Dock = System.Windows.Forms.DockStyle.Left;
            this.goBtn.Location = new System.Drawing.Point(358, 6);
            this.goBtn.Name = "goBtn";
            this.goBtn.Size = new System.Drawing.Size(57, 30);
            this.goBtn.TabIndex = 1;
            this.goBtn.Text = "Go";
            this.goBtn.UseVisualStyleBackColor = true;
            this.goBtn.Click += new System.EventHandler(this.goBtn_Click);
            // 
            // goNextBtn
            // 
            this.goNextBtn.AutoSize = true;
            this.goNextBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.goNextBtn.Dock = System.Windows.Forms.DockStyle.Left;
            this.goNextBtn.Location = new System.Drawing.Point(175, 6);
            this.goNextBtn.Name = "goNextBtn";
            this.goNextBtn.Size = new System.Drawing.Size(29, 30);
            this.goNextBtn.TabIndex = 8;
            this.goNextBtn.Text = ">";
            this.goNextBtn.UseVisualStyleBackColor = true;
            this.goNextBtn.Click += new System.EventHandler(this.SwitchPageRight);
            // 
            // goPrevBtn
            // 
            this.goPrevBtn.AutoSize = true;
            this.goPrevBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.goPrevBtn.Dock = System.Windows.Forms.DockStyle.Left;
            this.goPrevBtn.Location = new System.Drawing.Point(51, 6);
            this.goPrevBtn.Name = "goPrevBtn";
            this.goPrevBtn.Size = new System.Drawing.Size(29, 30);
            this.goPrevBtn.TabIndex = 1;
            this.goPrevBtn.Text = "<";
            this.goPrevBtn.UseVisualStyleBackColor = true;
            this.goPrevBtn.Click += new System.EventHandler(this.SwitchPageLeft);
            // 
            // goFirstBtn
            // 
            this.goFirstBtn.AutoSize = true;
            this.goFirstBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.goFirstBtn.Dock = System.Windows.Forms.DockStyle.Left;
            this.goFirstBtn.Location = new System.Drawing.Point(6, 6);
            this.goFirstBtn.Name = "goFirstBtn";
            this.goFirstBtn.Size = new System.Drawing.Size(39, 30);
            this.goFirstBtn.TabIndex = 0;
            this.goFirstBtn.Text = "<<";
            this.goFirstBtn.UseVisualStyleBackColor = true;
            this.goFirstBtn.Click += new System.EventHandler(this.SwitchToFirst);
            // 
            // goLastBtn
            // 
            this.goLastBtn.AutoSize = true;
            this.goLastBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.goLastBtn.Dock = System.Windows.Forms.DockStyle.Left;
            this.goLastBtn.Location = new System.Drawing.Point(210, 6);
            this.goLastBtn.Name = "goLastBtn";
            this.goLastBtn.Size = new System.Drawing.Size(39, 30);
            this.goLastBtn.TabIndex = 9;
            this.goLastBtn.Text = ">>";
            this.goLastBtn.UseVisualStyleBackColor = true;
            this.goLastBtn.Click += new System.EventHandler(this.SwitchToLast);
            // 
            // PageButtonControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PageButtonControl";
            this.Size = new System.Drawing.Size(424, 45);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button goPrevBtn;
        private System.Windows.Forms.Button goFirstBtn;
        private System.Windows.Forms.TextBox gotoTxtBox;
        private System.Windows.Forms.Button goNextBtn;
        private System.Windows.Forms.Button goLastBtn;
        private System.Windows.Forms.Button goBtn;
        private System.Windows.Forms.Label pageLbl;
    }
}
