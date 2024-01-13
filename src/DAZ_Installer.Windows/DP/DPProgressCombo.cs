// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using DAZ_Installer.Windows.Pages;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.DP
{
    internal class DPProgressCombo
    {
        internal static Stack<DPProgressCombo> ProgressCombos = new(3);
        internal TableLayoutPanel Panel { get; private set; }
        internal Label ProgressBarLbl { get; private set; }
        internal ProgressBar ProgressBar { get; private set; }

        internal bool IsMarqueueProgressBar => ProgressBar.Style == ProgressBarStyle.Marquee;

        internal DPProgressCombo()
        {
            Extract.ExtractPage.Invoke(CreateProgressCombo);
            Extract.ExtractPage.BeginInvoke(Extract.ExtractPage.AddNewProgressCombo, this);
            ProgressCombos.Push(this);
        }

        /// <summary>
        /// Creates a new DPProgressCombo.
        /// </summary>
        // This function is called once on the UI thread.
        private void CreateProgressCombo()
        {
            // Panel
            Panel = new TableLayoutPanel();
            Panel.Dock = DockStyle.Fill;
            Panel.ColumnCount = 1;
            Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            Panel.RowCount = 2;
            Panel.RowStyles.Add(new ColumnStyle(SizeType.AutoSize));
            Panel.RowStyles.Add(new ColumnStyle(SizeType.AutoSize));

            ProgressBarLbl = new Label();
            ProgressBarLbl.Text = "Processing ...";
            ProgressBarLbl.Dock = DockStyle.Fill;
            ProgressBarLbl.AutoEllipsis = true;
            ProgressBarLbl.TextAlign = ContentAlignment.BottomLeft;
            ProgressBarLbl.MinimumSize = new Size(0, 25);
            Panel.Controls.Add(ProgressBarLbl, 0, 0);

            ProgressBar = new ProgressBar();
            ProgressBar.Value = 50;
            ProgressBar.Dock = DockStyle.Fill;
            ProgressBar.MinimumSize = new Size(0, 18);
            ProgressBar.MarqueeAnimationSpeed /= 5;
            Panel.Controls.Add(ProgressBar, 0, 1);

            // ProgressBar.CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// Changes the process bar style to either <see cref="ProgressBarStyle.Marquee"/> or <see cref="ProgressBarStyle.Blocks"/>". 
        /// It automatically checks if Invoke is required.
        /// </summary>
        /// <param name="marqueue">Whether to set the progress bar style to Marqueue or not.</param>
        internal void ChangeProgressBarStyle(bool marqueue)
        {
            // Removed check if marque is already set to marquee due to the fact that it could change later due to async queued' calls.
            if (Extract.ExtractPage.InvokeRequired)
            {
                Extract.ExtractPage.BeginInvoke(ChangeProgressBarStyle, marqueue);
                return;
            }
            try
            {
                ProgressBar.SuspendLayout();
                if (marqueue)
                {
                    ProgressBar.Value = 10;
                    ProgressBar.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    ProgressBar.Value = 50;
                    ProgressBar.Style = ProgressBarStyle.Blocks;
                }
            } finally
            {
                ProgressBar.ResumeLayout();
            }
        }

        /// <summary>
        /// Removes all DPProgressCombos from the UI. This is a blocking UI call.
        /// </summary>
        internal static void RemoveAll()
        {
            Extract.ExtractPage.Invoke(Extract.ExtractPage.ResetExtractPage);
            ProgressCombos.Clear();
        }

        /// <summary>
        /// Pops the last DPProgressCombo from the stack and removes it from the UI. This is a blocking UI call.
        /// </summary>
        internal void Pop()
        {
            if (ProgressCombos.TryPop(out _))
                Extract.ExtractPage.Invoke(Extract.ExtractPage.DeleteProgressionCombo, this);
        }

        /// <summary>
        /// Sets the value of the progress bar. Automatically checks if Invoke is required.
        /// </summary>
        /// <param name="value"></param>
        internal void SetProgressBarValue(int value)
        {
            if (Extract.ExtractPage.InvokeRequired)
            {
                Extract.ExtractPage.BeginInvoke(SetProgressBarValue, value);
                return;
            }
            ProgressBar.Value = value;
        }

        /// <summary>
        /// Updates the text of the progress bar label and the main process label. Automatically checks if Invoke is required.
        /// </summary>
        /// <param name="text">The text to set it to.</param>
        internal void UpdateText(string text)
        {
            if (Extract.ExtractPage.InvokeRequired)
            {
                Extract.ExtractPage.BeginInvoke(UpdateText, text);
                return;
            }
            ProgressBarLbl.Text = Extract.ExtractPage.mainProcLbl.Text = text;
        }

        ~DPProgressCombo()
        {
            ProgressBar.Dispose();
            ProgressBarLbl.Dispose();
            Panel.Dispose();
        }
    }
}