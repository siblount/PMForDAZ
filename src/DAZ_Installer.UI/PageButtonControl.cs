﻿// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
//using static DAZ_Installer

namespace DAZ_Installer
{

    public partial class PageButtonControl : UserControl
    {
        //
        public delegate void PageChangeHandler(uint page);
        //
        //
        public event PageChangeHandler PageChange;
        //
        [Range(1, uint.MaxValue)]
        [Browsable(true), Description("Gets the current page and setting it calls UpdateControl."), Category("Data"), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public uint CurrentPage
        {
            get => currentPage;
            set
            {
                // Do not update & signal a page change if it is invalid except if it is 0.
                if (value == 0) currentPage = 1;
                else if (!IsValidPageNumber(value)) return;
                else currentPage = value;
                PageChange?.Invoke(currentPage);
                UpdateControl();
            }
        }
        [Range(1, uint.MaxValue)]
        [Browsable(true), Description("Gets the page count and setting it calls UpdateControl."), Category("Data"), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]

        public uint PageCount
        {
            get => pageCount;
            set
            {
                pageCount = value == 0 ? 1 : value;
                // Remember that CurrentPage property also calls UpdateControl() which is 
                // why we use else here.
                // IsValidPageNumber is to signal an update if, for example, the PageCount is now
                // lower than the current page (ex: switching from library to search view).
                if (!IsValidPageNumber(currentPage)) CurrentPage = PageCount;
                else UpdateControl();
            }
        }
        protected uint currentPage = 1;
        protected uint pageCount = 1;
        [ThreadStatic]
        protected bool invoked = false;
        public PageButtonControl() => InitializeComponent();
        private bool IsValidPageNumber(uint page) => page <= pageCount && page > 0;

        private void SwitchPage(object _, EventArgs __)
        {
            if (uint.TryParse(gotoTxtBox.Text, out var page))
                CurrentPage = page;
        }

        private void SwitchPageLeft(object _, EventArgs __) => CurrentPage = currentPage - 1;
        private void SwitchPageRight(object _, EventArgs __) => CurrentPage = CurrentPage + 1;
        private void SwitchToFirst(object _, EventArgs __) => CurrentPage = 1;
        private void SwitchToLast(object _, EventArgs __) => CurrentPage = pageCount;
        public void UpdateControl()
        {
            var isOnMainThread = false;
            //bool isOnMainThread = IsOnMainThread;
            if ((!isOnMainThread || (IsHandleCreated && InvokeRequired)) && !invoked)
            {
                if (!IsHandleCreated) return; // Stop failing to create component on designer.
                invoked = true;
                Invoke(UpdateControl);
                return;
            }
            else if (!isOnMainThread && !IsHandleCreated) return;
            tableLayoutPanel1.SuspendLayout();
            pageLbl.Text = $"Page {currentPage} out of {pageCount}";
            tableLayoutPanel1.ResumeLayout();
        }

        public void SilentUpdatePageCount(uint newPageCount)
        {
            if (InvokeRequired)
            {
                Invoke(SilentUpdatePageCount, newPageCount);
                return;
            }
            pageCount = newPageCount;
            pageLbl.Text = $"Page {currentPage} out of {pageCount}";
        }

        public void SilentUpdateCurrentPage(uint newCurrentPage)
        {
            if (InvokeRequired)
            {
                Invoke(SilentUpdateCurrentPage, newCurrentPage);
                return;
            }
            currentPage = newCurrentPage;
            pageLbl.Text = $"Page {currentPage} out of {pageCount}";
        }

        private void gotoTxtBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                SwitchPage(null, null);
        }

        private void gotoTxtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) || (e.KeyChar == '.'))
                e.Handled = true;
        }

        private void goBtn_Click(object sender, EventArgs e) => SwitchPage(null, null);
    }
}
