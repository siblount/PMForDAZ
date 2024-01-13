// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Windows.DP;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.Forms
{
    public partial class MainForm : Form
    {
        public static Color initialSidePanelColor;
        public static MainForm activeForm;

        internal static UserControl[] userControls = new UserControl[4];
        internal static UserControl visiblePage = null;

        public MainForm()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            activeForm = this;
            InitalizePages();

            if (Environment.OSVersion.Version.Build >= 22000) DPGlobal.isWindows11 = true;
        }

        private void InitalizePages()
        {
            visiblePage = homePage1;

            userControls[0] = homePage1;
            userControls[1] = extractControl1;
            userControls[2] = library1;
            userControls[3] = settings1;

            foreach (UserControl control in userControls)
            {
                if (control == visiblePage) continue;
                control.Visible = false;
                control.Enabled = false;
            }
        }

        internal static void SwitchPage(UserControl switchTo)
        {
            if (switchTo == visiblePage) return;
            switchTo.BringToFront();
            switchTo.Enabled = true;
            switchTo.Visible = true;

            visiblePage.Enabled = false;
            visiblePage.Visible = false;

            visiblePage = switchTo;
        }

        public void SwitchToExtractPage() => extractControl1.BringToFront();

        private void Form1_Load(object sender, EventArgs e) => initialSidePanelColor = tableLayoutPanel1.BackColor;

        private void sidePanelButtonMouseEnter(object sender, EventArgs e)
        {
            var button = (Label)sender;
            button.ForeColor = Color.FromKnownColor(KnownColor.Coral);
        }

        private void sidePanelButtonMouseExit(object sender, EventArgs e)
        {
            var button = (Label)sender;
            button.ForeColor = Color.FromKnownColor(KnownColor.White);
        }

        private void extractLbl_Click(object sender, EventArgs e) => SwitchPage(extractControl1);

        private void homeLabel_Click(object sender, EventArgs e) => SwitchPage(homePage1);

        private void libraryLbl_Click(object sender, EventArgs e)
        {

            if (library1 != visiblePage)
            {
                SwitchPage(library1);
                //library1.TryPageUpdate();
            }
        }

        private void settingsLbl_Click(object sender, EventArgs e) => SwitchPage(settings1);

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) => DPGlobal.HandleAppClosing(e);

        internal string? ShowMissingVolumePrompt(string msg, string filter, string ext, string? defaultLocation)
        {
            DialogResult result = MessageBox.Show(msg, "Missing volumes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                return ShowFileDialog(filter, ext, defaultLocation);
            }
            return null;

        }

        public string? ShowFileDialog(string filter, string defaultExt, string defaultLocation = null)
        {
            if (InvokeRequired)
            {
                return (string)Invoke(ShowFileDialog, filter, defaultExt, defaultLocation);
            }
            openFileDialog.Filter = filter;
            openFileDialog.DefaultExt = defaultExt;
            if (defaultLocation != null)
            {
                openFileDialog.InitialDirectory = defaultLocation;
            }
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        private void pictureBox1_Click(object sender, EventArgs e) => new AboutForm().ShowDialog();

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {

        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = Program.DropEffect;
            // Get the page we are currently in... If we are not on the home page, then switch to it.
            if (visiblePage != homePage1)
            {
                SwitchPage(homePage1);
            }
        }
    }
}
