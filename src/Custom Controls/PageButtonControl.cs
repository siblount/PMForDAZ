// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using static DAZ_Installer.DP.DPCommon;

namespace DAZ_Installer
{

    // TO DO: Add another >> && << button to go to highest page and lowest page.
    // Show << when not in 1 - 7 range.

    public partial class PageButtonControl : UserControl
    {
        //
        internal delegate void PageChangeHandler(int page);
        //
        //
        internal event PageChangeHandler PageChange;
        //
        protected readonly Button[] buttons;
        protected readonly Dictionary<int, Button> pageButtonPairs;
        [Range(1, int.MaxValue)]
        [Browsable(true), Description("Gets the current page and setting it calls UpdateControl."), Category("Data"), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int CurrentPage {
            get => currentPage;
            set
            {
                previousCurrentPage = currentPage;
                currentPage = value;
                UpdateControl();
            }
        }
        [Range(1, int.MaxValue)]
        [Browsable(true), Description("Gets the page count and setting it calls UpdateControl."), Category("Data"), EditorBrowsable(EditorBrowsableState.Always), DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        
        public int PageCount {
            get => pageCount;
            set 
            {
                previousPageCount = pageCount;
                pageCount = value;
                UpdateControl();
            } 
        }
        protected int currentPage = 1;
        protected int pageCount = 1;
        protected int previousCurrentPage = 1;
        protected int previousPageCount = 1;
        public PageButtonControl()
        {
            InitializeComponent();
            buttons = new Button[] { btn1, btn2, btn3, btn4, btn5, btn6, btn7 };
            var dict = new Dictionary<int, Button>(buttons.Length);
            foreach (var button in buttons)
            {
                dict.Add(int.Parse(button.Text), button);
            }
            pageButtonPairs = dict;
        }

        internal int GetPreviousPage()
        {
            return previousCurrentPage;
        }

        private int GetButtonViaPage(int page)
        {
            var result = (page % 7) - 1;
            if (result == -1) return 6;
            else return result;
        }
        private bool IsValidPageNumber(int page) => page <= pageCount && page > 0;

        private void SwitchPage(object sender, EventArgs e)
        {
            // sender is Button.
            var page = int.Parse(((Button)sender).Text);
            if (IsValidPageNumber(page))
            {
                CurrentPage = page;
                PageChange.Invoke(page);
            }
        }


        private void SwitchPageLeft(object _, EventArgs __)
        {
            var page = currentPage - 1;
            if (IsValidPageNumber(page))
            {
                CurrentPage = page;
                PageChange.Invoke(page);
            }
        }
        private void SwitchPageRight(object _, EventArgs __)
        {
            var page = currentPage + 1;
            if (IsValidPageNumber(page))
            {
                CurrentPage = page;
                PageChange.Invoke(page);
            }

        }

        public void UpdateControl()
        {
            bool isOnMainThread = IsOnMainThread;
            if (!isOnMainThread || (IsHandleCreated && InvokeRequired))
            {
                if (!IsHandleCreated) return; // Stop failing to create component on designer.
                Invoke(UpdateControl);
                return;
            }
            else if (!isOnMainThread && !IsHandleCreated) return;
            // Only update if the currentPage is out of the current current range.
            // Get min page if teleport = currentPage - (currentPage % 7)
            var activeButtons = GetActiveButtons();
            // Current
            var buttonMin = GetButtonMinMax(true, ref activeButtons);
            var buttonMax = GetButtonMinMax(false, ref activeButtons);
            //
            var newPageOutOfCurrentRange = currentPage > buttonMax || currentPage < buttonMin;
            // Buttons 1-7 show only if pageCount == 7.
            // Show Goto on pageCount > 7
            // If we did next page via button page, simply make the next number the first button.
            // Otherwise, we did it via textbox and do it via 
            var showGoTo = pageCount > 7;
            int buttonsToShow = GetButtonsToShow();
            var pageCountChanged = previousPageCount != pageCount || pageCount == 1;
            tableLayoutPanel1.SuspendLayout();
            // If the current pending new page # is out of the current range.
            // Selected button will be disabled and have some sort of highlight.
            // 
            if (newPageOutOfCurrentRange || pageCountChanged)
            {
                // Update previous page count.
                previousPageCount = pageCount;
                // Reset everything.
                ResetTableAndButtons();
                ResetGoTo();

                // Update dictionary.
                UpdateDictionary();

                // Add buttons.
                tableLayoutPanel1.Controls.Add(goLeftBtn);
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                tableLayoutPanel1.ColumnCount = buttonsToShow + 2;
                for (int i = 0; i < buttonsToShow; i++)
                {
                    AddButtonToTable(ref buttons[i]);
                }
                // Add last button AND goto text box if needed.
                if (showGoTo)
                {
                    tableLayoutPanel1.Controls.Add(gotoTxtBox);
                    gotoTxtBox.Visible = true;
                }
                else {
                    try { tableLayoutPanel1.Controls.Remove(gotoTxtBox); } catch { };
                    
                    gotoTxtBox.Visible = false;
                }

                tableLayoutPanel1.Controls.Add(goRight);
                goRight.Visible = true;

                // Change the buttons text value and destination.
                ResetButtonTexts();
                UpdateSelectedButton();
                // TODO: Add func for adding LibaryItems per page.

            } else
            {
                // Get which button needs to be updated.
                UpdateSelectedButton();
            }
            tableLayoutPanel1.ResumeLayout();

        }

        private int GetButtonsToShow()
        {
            if (pageCount - currentPage < 7)
            {
                var result = (pageCount - currentPage) + 1;
                if (result < 0) return 1;
                else return result;
            }
            else if (pageCount - currentPage < 0) return 1;
            else return 7;
        }

        private void UpdateDictionary()
        {
            pageButtonPairs.Clear();
            foreach (var button in buttons)
            {
                pageButtonPairs.Add(int.Parse(button.Text), button);
            }
        }

        private void AddButtonToTable(ref Button button)
        {
            tableLayoutPanel1.Controls.Add(button);
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            button.Visible = true;
        }

        private void UpdateSelectedButton()
        {
            bool errored = false;
            TRYAGAIN:
            try
            {
                // Get last selected number and calculate where it was and enable it.
                try
                {
                    var lastButton = pageButtonPairs[previousCurrentPage];
                    lastButton.Enabled = true;
                } catch { }

                // Now disable the current page button.

                var button = pageButtonPairs[currentPage];
                button.Enabled = false;
            }
            catch {
                if (!errored)
                {
                    errored = true;
                    UpdateDictionary();
                    goto TRYAGAIN;
                }
            }
        }

        private void ResetButtonTexts()
        {
            // Just update them all starting from the start #.
            for (var i = 0; i < buttons.Length; i++)
            {
                buttons[i].Text = (currentPage + i).ToString();
            }
        }

        // Note: does not reset column count.
        private void ResetTableAndButtons()
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            foreach ( var button in buttons)
            {
                button.Visible = false;
                button.Enabled = true;
            }
        }

        private void ResetGoTo()
        {
            gotoTxtBox.Visible = false;
            gotoTxtBox.Text = "";
        }

        private int GetButtonMinMax(bool getMin, ref Button[] buttons)
        {
            if (getMin)
            {
                var min = 999999999;
                foreach (Button btn in buttons)
                {
                    int intMsg;
                    int.TryParse(btn.Text, out intMsg);
                    if (intMsg < min) min = intMsg;
                }
                return min;
            }
            else
            {
                int max = 0;
                foreach (Button btn in buttons)
                {
                    int intMsg;
                    int.TryParse(btn.Text, out intMsg);
                    if (intMsg > max) max = intMsg;
                }
                return max;
            }
        }

        private Button[] GetActiveButtons()
        {
            var activeButtons = new List<Button>(8);
            foreach (Button button in buttons)
            {
                if (button.Visible) activeButtons.Add(button);
            }
            activeButtons.TrimExcess();
            return activeButtons.ToArray();
        }

        private void gotoTxtBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                int page = int.Parse(gotoTxtBox.Text);
                if (IsValidPageNumber(page))
                {
                    CurrentPage = page;
                    PageChange.Invoke(page);
                }
            }
        }

        private void gotoTxtBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar) || (e.KeyChar == '.'))
            {
                e.Handled = true;
            }
        }
    }
}
