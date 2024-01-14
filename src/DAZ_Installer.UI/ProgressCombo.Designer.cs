namespace DAZ_Installer.UI
{
    partial class ProgressCombo
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
            progressBarLbl = new Label();
            progressBar = new ProgressBar();
            cancelBtn = new Button();
            mainProcLbl = new Label();
            SuspendLayout();
            // 
            // progressBarLbl
            // 
            progressBarLbl.AutoEllipsis = true;
            progressBarLbl.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point);
            progressBarLbl.Location = new Point(0, 46);
            progressBarLbl.Name = "progressBarLbl";
            progressBarLbl.Size = new Size(667, 31);
            progressBarLbl.TabIndex = 0;
            progressBarLbl.Text = "Processing...";
            progressBarLbl.TextAlign = ContentAlignment.BottomLeft;
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(0, 80);
            progressBar.MarqueeAnimationSpeed = 20;
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(667, 293);
            progressBar.TabIndex = 1;
            progressBar.Value = 50;
            // 
            // cancelBtn
            // 
            cancelBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cancelBtn.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point);
            cancelBtn.Location = new Point(612, 3);
            cancelBtn.Name = "cancelBtn";
            cancelBtn.Size = new Size(52, 46);
            cancelBtn.TabIndex = 2;
            cancelBtn.Text = "X";
            cancelBtn.UseVisualStyleBackColor = true;
            cancelBtn.Click += cancelBtn_Click;
            // 
            // mainProcLbl
            // 
            mainProcLbl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            mainProcLbl.AutoEllipsis = true;
            mainProcLbl.Font = new Font("Segoe UI Variable Display Semil", 17.25F, FontStyle.Regular, GraphicsUnit.Point);
            mainProcLbl.Location = new Point(0, 0);
            mainProcLbl.Margin = new Padding(4, 0, 4, 0);
            mainProcLbl.Name = "mainProcLbl";
            mainProcLbl.Size = new Size(608, 46);
            mainProcLbl.TabIndex = 3;
            mainProcLbl.Text = "Nothing to extract.";
            mainProcLbl.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProgressCombo
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            Controls.Add(cancelBtn);
            Controls.Add(mainProcLbl);
            Controls.Add(progressBar);
            Controls.Add(progressBarLbl);
            Name = "ProgressCombo";
            Size = new Size(667, 373);
            ResumeLayout(false);
        }

        #endregion

        private Label progressBarLbl;
        private ProgressBar progressBar;
        private Button cancelBtn;
        internal Label mainProcLbl;
    }
}
