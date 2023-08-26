// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using DAZ_Installer.WinApp.Pages;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DAZ_Installer.WinApp
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
            Extract.ExtractPage.Invoke(Extract.ExtractPage.AddNewProgressCombo, this);
            ProgressCombos.Push(this);
        }

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


        internal void ChangeProgressBarStyle(bool marqueue)
        {
            if (IsMarqueueProgressBar == marqueue) return;
            if (Extract.ExtractPage.InvokeRequired)
            {
                Extract.ExtractPage.Invoke(ChangeProgressBarStyle, marqueue);
                return;
            }

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
            ProgressBar.ResumeLayout();

        }

        internal static void RemoveAll()
        {
            Extract.ExtractPage.Invoke(Extract.ExtractPage.ResetExtractPage);
            ProgressCombos.Clear();
        }

        internal void Remove()
        {
            if (ProgressCombos.TryPop(out _))
                Extract.ExtractPage.DeleteProgressionCombo(this);
        }

        internal void UpdateText(string text)
        {
            ProgressBarLbl.Text =
            Extract.ExtractPage.mainProcLbl.Text =
            text;
        }

        ~DPProgressCombo()
        {
            ProgressBar.Dispose();
            ProgressBarLbl.Dispose();
            Panel.Dispose();
        }
    }
}