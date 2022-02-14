// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [Browsable(true),EditorBrowsable(EditorBrowsableState.Always),Description("Holds the current library items."), Category("Items")]

        internal LibraryItem[] LibraryItems
        {
            get => libraryItems;
            set
            {
                if (value == null) libraryItems = new LibraryItem[25];
                else libraryItems = value;
                UpdateMainContent();
            }
        }
        protected LibraryItem[] libraryItems;
        internal int CurrentPage 
        { 
            get => pageButtonControl1.CurrentPage; 
            set
            {
                pageButtonControl1.CurrentPage = value;
            }
        }

        internal int PreviousPage
        {
            get => pageButtonControl1.GetPreviousPage();
        }

        internal int PageCount
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

        internal void UpdateMainContent()
        {
            DPCommon.WriteToLog("Update main content.");
            DPCommon.WriteToLog($"Library items: {libraryItems.Length}");
            EditMode = true;
            mainContentPanel.Controls.Clear();
            mainContentPanel.Controls.AddRange(libraryItems);
            
            foreach (var item in libraryItems)
            {
                if (item != null)
                item.Dock = DockStyle.Top;
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
