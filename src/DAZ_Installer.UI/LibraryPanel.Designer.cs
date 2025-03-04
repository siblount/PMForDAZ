
namespace DAZ_Installer
{
    partial class LibraryPanel
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
            mainContentPanel = new Panel();
            pageButtonControl1 = new PageButtonControl();
            buttonsContainer = new Panel();
            buttonsContainer.SuspendLayout();
            SuspendLayout();
            // 
            // mainContentPanel
            // 
            mainContentPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            mainContentPanel.AutoScroll = true;
            mainContentPanel.Location = new Point(0, 0);
            mainContentPanel.Name = "mainContentPanel";
            mainContentPanel.Size = new Size(578, 343);
            mainContentPanel.TabIndex = 0;
            // 
            // pageButtonControl1
            // 
            pageButtonControl1.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            pageButtonControl1.AutoSize = true;
            pageButtonControl1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pageButtonControl1.CurrentPage = 1U;
            pageButtonControl1.Location = new Point(107, 9);
            pageButtonControl1.MaximumSize = new Size(510, 45);
            pageButtonControl1.MinimumSize = new Size(50, 42);
            pageButtonControl1.Name = "pageButtonControl1";
            pageButtonControl1.PageCount = 1U;
            pageButtonControl1.Size = new Size(386, 45);
            pageButtonControl1.TabIndex = 1;
            pageButtonControl1.SizeChanged += pageButtonControl1_SizeChanged;
            // 
            // buttonsContainer
            // 
            buttonsContainer.Controls.Add(pageButtonControl1);
            buttonsContainer.Dock = DockStyle.Bottom;
            buttonsContainer.Location = new Point(0, 339);
            buttonsContainer.Name = "buttonsContainer";
            buttonsContainer.Size = new Size(578, 57);
            buttonsContainer.TabIndex = 2;
            buttonsContainer.SizeChanged += buttonsContainer_SizeChanged;
            // 
            // LibraryPanel
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            BackColor = Color.White;
            Controls.Add(buttonsContainer);
            Controls.Add(mainContentPanel);
            DoubleBuffered = true;
            Name = "LibraryPanel";
            Size = new Size(578, 396);
            buttonsContainer.ResumeLayout(false);
            buttonsContainer.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mainContentPanel;
        private PageButtonControl pageButtonControl1;
        public System.Windows.Forms.Panel buttonsContainer;
    }
}
