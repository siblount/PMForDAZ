// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.DP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace DAZ_Installer
{
    /// <summary>
    /// The LibrayPanel is only responsible for showing the library panels that correspond to their page.
    /// </summary>
    public partial class LibraryPanel : UserControl
    {
        public LibraryPanel()
        {
            InitializeComponent();
        }
        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always), Description("Holds the current library items."), Category("Items")]

        internal List<LibraryItem> LibraryItems { get; } = new List<LibraryItem>(25);

        internal List<LibraryItem> SearchItems { get; set; } = new List<LibraryItem>(25);
        internal uint CurrentPage 
        { 
            get => pageButtonControl1.CurrentPage; 
            set
            {
                pageButtonControl1.CurrentPage = value;
            }
        }

        internal uint PageCount
        {
            get => pageButtonControl1.PageCount;
            set => pageButtonControl1.PageCount = value;
        }

        internal bool EditMode
        {
            set
            {
                if (editMode == value) return;
                else
                {
                    if (value == true)
                    {
                        mainContentPanel.SuspendLayout();
                    } else
                    {
                        mainContentPanel.ResumeLayout(true);
                    }
                    editMode = value;
                }
            }
            get => editMode;
        }
        private bool editMode;
        internal volatile bool SearchMode = false;

        internal void UpdateMainContent()
        {
            EditMode = true;
            if (!SearchMode)
            {
                DPCommon.WriteToLog("Update main content.");
                DPCommon.WriteToLog($"Library items: {LibraryItems.Count}");
                
                mainContentPanel.Controls.Clear();
                mainContentPanel.Controls.AddRange(LibraryItems.ToArray());

                foreach (var item in LibraryItems)
                {
                    if (item != null)
                        item.Dock = DockStyle.Top;
                }
                
            } else
            {
                DPCommon.WriteToLog("Update search content.");
                DPCommon.WriteToLog($"Search items: {SearchItems.Count}");

                mainContentPanel.Controls.Clear();
                mainContentPanel.Controls.AddRange(SearchItems.ToArray());

                foreach (var item in SearchItems)
                {
                    if (item != null)
                        item.Dock = DockStyle.Top;
                }
            }
            EditMode = false;
        }

        private void pageButtonControl1_SizeChanged(object sender, EventArgs e)
        {
            // We need to manually center it in the containing panel.
            pageButtonControl1.Left = (buttonsContainer.ClientSize.Width - pageButtonControl1.Width) / 2;
            //pageButtonControl1.Top = (buttonsContainer.ClientSize.Height - pageButtonControl1.Height) / 2;
        }

        internal void AddPageChangeListener(PageButtonControl.PageChangeHandler pageChangedFunc)
        {
            pageButtonControl1.PageChange += pageChangedFunc;
        }

        private void createNewRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        // Form.OnResizeEnd may be better performance wise.
        private void buttonsContainer_SizeChanged(object sender, EventArgs e)
        {
            // We need to manually center it in the containing panel.
            // TODO: Hide 
            pageButtonControl1.Left = (buttonsContainer.ClientSize.Width - pageButtonControl1.Width) / 2;
            //pageButtonControl1.Top = (buttonsContainer.ClientSize.Height - pageButtonControl1.Height) / 2;
        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }
    }
}
