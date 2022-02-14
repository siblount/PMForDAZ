
namespace DAZ_Installer
{
    partial class ProductRecordForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.productNameTxtBox = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.listView1 = new System.Windows.Forms.ListView();
            this.label4 = new System.Windows.Forms.Label();
            this.listView2 = new System.Windows.Forms.ListView();
            this.dateExtractedLbl = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Product Name:";
            // 
            // productNameTxtBox
            // 
            this.productNameTxtBox.Location = new System.Drawing.Point(126, 27);
            this.productNameTxtBox.Name = "productNameTxtBox";
            this.productNameTxtBox.Size = new System.Drawing.Size(231, 27);
            this.productNameTxtBox.TabIndex = 1;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::DAZ_Installer.Properties.Resources.NoImageFound;
            this.pictureBox1.Location = new System.Drawing.Point(390, 27);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(226, 152);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(390, 185);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(226, 29);
            this.button1.TabIndex = 3;
            this.button1.Text = "Browse Image";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Tags:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(60, 67);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(297, 27);
            this.textBox1.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Content Folders:";
            // 
            // listView1
            // 
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(13, 130);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(344, 84);
            this.listView1.TabIndex = 7;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 217);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(107, 20);
            this.label4.TabIndex = 8;
            this.label4.Text = "Files Extracted:";
            // 
            // listView2
            // 
            this.listView2.HideSelection = false;
            this.listView2.Location = new System.Drawing.Point(13, 240);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(603, 85);
            this.listView2.TabIndex = 9;
            this.listView2.UseCompatibleStateImageBehavior = false;
            // 
            // dateExtractedLbl
            // 
            this.dateExtractedLbl.AutoSize = true;
            this.dateExtractedLbl.Location = new System.Drawing.Point(13, 332);
            this.dateExtractedLbl.Name = "dateExtractedLbl";
            this.dateExtractedLbl.Size = new System.Drawing.Size(114, 20);
            this.dateExtractedLbl.TabIndex = 10;
            this.dateExtractedLbl.Text = "Date Extracted: ";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 366);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 20);
            this.label5.TabIndex = 11;
            this.label5.Text = "label5";
            // 
            // ProductRecordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 450);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.dateExtractedLbl);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.productNameTxtBox);
            this.Controls.Add(this.label1);
            this.Name = "ProductRecordForm";
            this.Text = "ProductRecordForm";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox productNameTxtBox;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.Label dateExtractedLbl;
        private System.Windows.Forms.Label label5;
    }
}