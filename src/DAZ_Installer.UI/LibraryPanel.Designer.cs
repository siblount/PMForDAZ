
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
            this.components = new System.ComponentModel.Container();
            this.mainContentPanel = new System.Windows.Forms.Panel();
            this.pageButtonControl1 = new DAZ_Installer.PageButtonControl();
            this.buttonsContainer = new System.Windows.Forms.Panel();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.createNewRecordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonsContainer.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainContentPanel
            // 
            this.mainContentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainContentPanel.AutoScroll = true;
            this.mainContentPanel.Location = new System.Drawing.Point(0, 0);
            this.mainContentPanel.Name = "mainContentPanel";
            this.mainContentPanel.Size = new System.Drawing.Size(578, 343);
            this.mainContentPanel.TabIndex = 0;
            // 
            // pageButtonControl1
            // 
            this.pageButtonControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.pageButtonControl1.AutoSize = true;
            this.pageButtonControl1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pageButtonControl1.CurrentPage = ((uint)(1u));
            this.pageButtonControl1.Location = new System.Drawing.Point(107, 9);
            this.pageButtonControl1.MaximumSize = new System.Drawing.Size(510, 45);
            this.pageButtonControl1.MinimumSize = new System.Drawing.Size(50, 42);
            this.pageButtonControl1.Name = "pageButtonControl1";
            this.pageButtonControl1.PageCount = ((uint)(1u));
            this.pageButtonControl1.Size = new System.Drawing.Size(386, 45);
            this.pageButtonControl1.TabIndex = 1;
            this.pageButtonControl1.SizeChanged += new System.EventHandler(this.pageButtonControl1_SizeChanged);
            // 
            // buttonsContainer
            // 
            this.buttonsContainer.Controls.Add(this.pageButtonControl1);
            this.buttonsContainer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.buttonsContainer.Location = new System.Drawing.Point(0, 339);
            this.buttonsContainer.Name = "buttonsContainer";
            this.buttonsContainer.Size = new System.Drawing.Size(578, 57);
            this.buttonsContainer.TabIndex = 2;
            this.buttonsContainer.SizeChanged += new System.EventHandler(this.buttonsContainer_SizeChanged);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createNewRecordToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(171, 26);
            // 
            // createNewRecordToolStripMenuItem
            // 
            this.createNewRecordToolStripMenuItem.Name = "createNewRecordToolStripMenuItem";
            this.createNewRecordToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.createNewRecordToolStripMenuItem.Text = "Create new record";
            this.createNewRecordToolStripMenuItem.Click += new System.EventHandler(this.createNewRecordToolStripMenuItem_Click);
            // 
            // LibraryPanel
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.buttonsContainer);
            this.Controls.Add(this.mainContentPanel);
            this.DoubleBuffered = true;
            this.Name = "LibraryPanel";
            this.Size = new System.Drawing.Size(578, 396);
            this.buttonsContainer.ResumeLayout(false);
            this.buttonsContainer.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mainContentPanel;
        private PageButtonControl pageButtonControl1;
        internal System.Windows.Forms.Panel buttonsContainer;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem createNewRecordToolStripMenuItem;
    }
}
