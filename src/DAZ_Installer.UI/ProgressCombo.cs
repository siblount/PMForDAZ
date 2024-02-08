using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.UI
{
    public partial class ProgressCombo : UserControl
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CancellationTokenSource CancellationTokenSource { get; set; } = new();
        public bool IsMarquee => progressBar.Style == ProgressBarStyle.Marquee;

        public ProgressCombo()
        {
            InitializeComponent();
            progressBarLbl.Visible = progressBar.Visible = cancelBtn.Visible = false;
        }

        /// <summary>
        /// Ends the progress bar. Automatically checks if Invoke is required. 
        /// </summary>
        public void EndProgress()
        {
            if (InvokeRequired)
            {
                BeginInvoke(EndProgress);
                return;
            }
            cancelBtn.Visible = false;
        }

        /// <summary>
        /// Starts the progress bar. Automatically checks if Invoke is required. 
        /// CancellationTokenSource is reset here.
        /// </summary>
        public void StartProgress()
        {
            if (InvokeRequired)
            {
                BeginInvoke(StartProgress);
                return;
            }
            CancellationTokenSource = new();
            progressBarLbl.Visible = progressBar.Visible = cancelBtn.Visible = true;
        }

        /// <summary>
        /// Sets the progress of the progress bar. Automatically checks if Invoke is required.
        /// </summary>
        /// <param name="value">The value to set</param>
        public void SetProgress(int value)
        {
            if (InvokeRequired)
            {
                BeginInvoke(SetProgress, value);
                return;
            }
            progressBar.Value = value;
        }

        /// <summary>
        /// Changes the process bar style to either <see cref="ProgressBarStyle.Marquee"/> or <see cref="ProgressBarStyle.Blocks"/>". 
        /// It automatically checks if Invoke is required.
        /// </summary>
        /// <param name="marqueue">Whether to set the progress bar style to Marqueue or not.</param>
        public void ChangeProgressBarStyle(bool marqueue)
        {
            if (InvokeRequired)
            {
                BeginInvoke(ChangeProgressBarStyle, marqueue);
                return;
            }
            progressBar.SuspendLayout();
            if (marqueue)
            {
                progressBar.Value = 10;
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                progressBar.Value = 50;
                progressBar.Style = ProgressBarStyle.Blocks;
            }
            progressBar.ResumeLayout();
        }

        /// <summary>
        /// Sets the text of the progress bar label and the main process label. Automatically checks if Invoke is required.
        /// </summary>
        /// <param name="text">The text to set it to.</param>
        public void SetText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(SetText, text);
                return;
            }
            SuspendLayout();
            progressBarLbl.Text = mainProcLbl.Text = text;
            ResumeLayout();
        }

        /// <summary>
        /// Requests for cancellation.
        /// </summary>
        private void cancelBtn_Click(object sender, EventArgs e)
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new();
        }
    }
}
