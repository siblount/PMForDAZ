using Serilog;
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
    public partial class ProgressCombo : UserControl, IProgressCombo
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private CancellationTokenSource CancellationTokenSource { get; set; } = new();
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks>
        /// When the user clicks the cancel button, the <see cref="CancellationTokenSource"/> is cancelled 
        /// and reset. Receivers should cache the token to check for cancellation since the token may be
        /// reset (aka a new cancellation token source is created).
        /// </remarks>
        public CancellationToken Token => CancellationTokenSource.Token;

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
                Invoke(StartProgress); // Changed to Invoke, otherwise CancellationTokenSource would always be outdated
                              // by the time the user clicks to cancel, the progress is using a new one
                              // but we gave DPExtractJob an old one because the BeginInvoke, even though
                              // this is called first, would actually be executed after DPExtractJob calls for the Token, thus making it outdated.
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
