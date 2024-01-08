namespace DAZ_Installer.TestingSuiteWindows
{
    partial class ProcessSettingsDialogue
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
            components = new System.ComponentModel.Container();
            label1 = new Label();
            destTxtBox = new TextBox();
            tmpTxtBox = new TextBox();
            label2 = new Label();
            label3 = new Label();
            installOptionComboBox = new ComboBox();
            overwriteChkBox = new CheckBox();
            label4 = new Label();
            cfListBox = new ListBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            removeToolStripMenuItem = new ToolStripMenuItem();
            removeAllToolStripMenuItem = new ToolStripMenuItem();
            copyToolStripMenuItem = new ToolStripMenuItem();
            cfTxtBox = new TextBox();
            addCFBtn = new Button();
            label5 = new Label();
            cfaListBox = new ListBox();
            cfATxtBox = new TextBox();
            label6 = new Label();
            cfaComboBox = new ComboBox();
            addCFABtn = new Button();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(94, 15);
            label1.TabIndex = 0;
            label1.Text = "Destination Path";
            // 
            // destTxtBox
            // 
            destTxtBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            destTxtBox.Location = new Point(112, 6);
            destTxtBox.Name = "destTxtBox";
            destTxtBox.ReadOnly = true;
            destTxtBox.Size = new Size(376, 23);
            destTxtBox.TabIndex = 1;
            // 
            // tmpTxtBox
            // 
            tmpTxtBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tmpTxtBox.Location = new Point(112, 40);
            tmpTxtBox.Name = "tmpTxtBox";
            tmpTxtBox.ReadOnly = true;
            tmpTxtBox.Size = new Size(376, 23);
            tmpTxtBox.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(43, 43);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 2;
            label2.Text = "Temp Path";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(28, 79);
            label3.Name = "label3";
            label3.Size = new Size(78, 15);
            label3.TabIndex = 4;
            label3.Text = "Install Option";
            // 
            // installOptionComboBox
            // 
            installOptionComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            installOptionComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            installOptionComboBox.FormattingEnabled = true;
            installOptionComboBox.Items.AddRange(new object[] { "Manifest Only", "Automatic", "File Sense Only" });
            installOptionComboBox.Location = new Point(112, 76);
            installOptionComboBox.Name = "installOptionComboBox";
            installOptionComboBox.Size = new Size(376, 23);
            installOptionComboBox.TabIndex = 5;
            installOptionComboBox.SelectedIndexChanged += installOptionComboBox_SelectedIndexChanged;
            // 
            // overwriteChkBox
            // 
            overwriteChkBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            overwriteChkBox.AutoSize = true;
            overwriteChkBox.Location = new Point(12, 471);
            overwriteChkBox.Name = "overwriteChkBox";
            overwriteChkBox.Size = new Size(202, 19);
            overwriteChkBox.TabIndex = 6;
            overwriteChkBox.Text = "Overwrite Files (Destination Only)";
            overwriteChkBox.UseVisualStyleBackColor = true;
            overwriteChkBox.CheckedChanged += overwriteChkBox_CheckedChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(15, 112);
            label4.Name = "label4";
            label4.Size = new Size(91, 15);
            label4.TabIndex = 7;
            label4.Text = "Content Folders";
            // 
            // cfListBox
            // 
            cfListBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cfListBox.ContextMenuStrip = contextMenuStrip1;
            cfListBox.FormattingEnabled = true;
            cfListBox.ItemHeight = 15;
            cfListBox.Location = new Point(112, 112);
            cfListBox.Name = "cfListBox";
            cfListBox.Size = new Size(376, 139);
            cfListBox.TabIndex = 8;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { removeToolStripMenuItem, removeAllToolStripMenuItem, copyToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(135, 70);
            // 
            // removeToolStripMenuItem
            // 
            removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            removeToolStripMenuItem.Size = new Size(134, 22);
            removeToolStripMenuItem.Text = "Remove";
            removeToolStripMenuItem.Click += removeToolStripMenuItem_Click;
            // 
            // removeAllToolStripMenuItem
            // 
            removeAllToolStripMenuItem.Name = "removeAllToolStripMenuItem";
            removeAllToolStripMenuItem.Size = new Size(134, 22);
            removeAllToolStripMenuItem.Text = "Remove All";
            removeAllToolStripMenuItem.Click += removeAllToolStripMenuItem_Click;
            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new Size(134, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += copyToolStripMenuItem_Click;
            // 
            // cfTxtBox
            // 
            cfTxtBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cfTxtBox.Location = new Point(112, 257);
            cfTxtBox.Name = "cfTxtBox";
            cfTxtBox.Size = new Size(319, 23);
            cfTxtBox.TabIndex = 9;
            // 
            // addCFBtn
            // 
            addCFBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addCFBtn.Location = new Point(437, 257);
            addCFBtn.Name = "addCFBtn";
            addCFBtn.Size = new Size(51, 23);
            addCFBtn.TabIndex = 10;
            addCFBtn.Text = "Add";
            addCFBtn.UseVisualStyleBackColor = true;
            addCFBtn.Click += addCFBtn_Click;
            // 
            // label5
            // 
            label5.Location = new Point(15, 284);
            label5.Name = "label5";
            label5.Size = new Size(91, 34);
            label5.TabIndex = 11;
            label5.Text = "Content Folder Aliases";
            // 
            // cfaListBox
            // 
            cfaListBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cfaListBox.ContextMenuStrip = contextMenuStrip1;
            cfaListBox.FormattingEnabled = true;
            cfaListBox.ItemHeight = 15;
            cfaListBox.Location = new Point(112, 286);
            cfaListBox.Name = "cfaListBox";
            cfaListBox.Size = new Size(376, 139);
            cfaListBox.TabIndex = 12;
            // 
            // cfATxtBox
            // 
            cfATxtBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cfATxtBox.Location = new Point(112, 431);
            cfATxtBox.Name = "cfATxtBox";
            cfATxtBox.Size = new Size(150, 23);
            cfATxtBox.TabIndex = 13;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Location = new Point(268, 434);
            label6.Name = "label6";
            label6.Size = new Size(18, 15);
            label6.TabIndex = 14;
            label6.Text = "to";
            // 
            // cfaComboBox
            // 
            cfaComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            cfaComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            cfaComboBox.FormattingEnabled = true;
            cfaComboBox.Location = new Point(292, 431);
            cfaComboBox.Name = "cfaComboBox";
            cfaComboBox.Size = new Size(139, 23);
            cfaComboBox.TabIndex = 15;
            // 
            // addCFABtn
            // 
            addCFABtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            addCFABtn.Location = new Point(437, 430);
            addCFABtn.Name = "addCFABtn";
            addCFABtn.Size = new Size(51, 23);
            addCFABtn.TabIndex = 16;
            addCFABtn.Text = "Add";
            addCFABtn.UseVisualStyleBackColor = true;
            addCFABtn.Click += addCFABtn_Click;
            // 
            // ProcessSettingsDialogue
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(500, 497);
            Controls.Add(addCFABtn);
            Controls.Add(cfaComboBox);
            Controls.Add(label6);
            Controls.Add(cfATxtBox);
            Controls.Add(cfaListBox);
            Controls.Add(label5);
            Controls.Add(addCFBtn);
            Controls.Add(cfTxtBox);
            Controls.Add(cfListBox);
            Controls.Add(label4);
            Controls.Add(overwriteChkBox);
            Controls.Add(installOptionComboBox);
            Controls.Add(label3);
            Controls.Add(tmpTxtBox);
            Controls.Add(label2);
            Controls.Add(destTxtBox);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimumSize = new Size(516, 536);
            Name = "ProcessSettingsDialogue";
            Text = "Process Settings Dialogue";
            Load += ProcessSettingsDialogue_Load;
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox destTxtBox;
        private TextBox tmpTxtBox;
        private Label label2;
        private Label label3;
        private ComboBox installOptionComboBox;
        private CheckBox overwriteChkBox;
        private Label label4;
        private ListBox cfListBox;
        private TextBox cfTxtBox;
        private Button addCFBtn;
        private Label label5;
        private ListBox cfaListBox;
        private TextBox cfATxtBox;
        private Label label6;
        private ComboBox cfaComboBox;
        private Button addCFABtn;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem removeToolStripMenuItem;
        private ToolStripMenuItem removeAllToolStripMenuItem;
        private ToolStripMenuItem copyToolStripMenuItem;
    }
}