// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System.ComponentModel;
using Serilog;
using DAZ_Installer.UI;
using DAZ_Installer.Database;
using System.Windows.Forms;

namespace DAZ_Installer
{
    /// <summary>
    /// The LibrayPanel is only responsible for showing the library panels that correspond to their page.
    /// </summary>
    public partial class LibraryPanel : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ILogger Logger { get; set; } = Log.ForContext<LibraryPanel>();
        public LibraryPanel() => InitializeComponent();
        public IEnumerable<LibraryItem> LibraryItems => mainContentPanel.Controls.Cast<LibraryItem>();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public uint CurrentPage
        {
            get => pageButtonControl1.CurrentPage;
            set => pageButtonControl1.CurrentPage = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public uint PageCount
        {
            get => pageButtonControl1.PageCount;
            set => pageButtonControl1.PageCount = value;
        }
        private byte UpdateCount = 0;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LibraryItemPool LibraryItemPool { get; init; } = new LibraryItemPool();
        public volatile bool SearchMode = false;

        public void UpdateMainContent(IReadOnlyList<DPProductRecordLite> records, Action<LibraryItem> configureItem)
        {
            BeginUpdate();
            try {
                Logger.Debug("Updating main content");

                var currentControls = mainContentPanel.Controls;
                var currentCount = currentControls.Count;
                var targetCount = records.Count;

                // If we need more controls than we currently have
                var itemsToAdd = new List<LibraryItem>(Math.Max(0, targetCount - currentCount));
                while (currentCount < targetCount)
                {
                    var newItem = LibraryItemPool.Rent();
                    newItem.Reset();
                    newItem.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                    itemsToAdd.Add(newItem);
                    currentCount++;
                }
                currentControls.AddRange([..itemsToAdd]);

                // Update or hide existing controls
                for (int i = 0; i < currentCount; i++)
                {
                    var control = (LibraryItem)currentControls[i];
                    control.BeginUpdate();
                    try
                    {
                        if (i < targetCount)
                        {
                            control.TitleText = records[i].Name;
                            control.Tags = records[i].Tags;
                            control.ProductRecord = records[i];
                            control.Dock = DockStyle.Top; // Important: Dock must be set AFTER added to the panel controls.

                            // Let the caller configure additional properties and events
                            configureItem(control);
                        
                            control.Visible = true;
                        }
                        else
                        {
                            control.Visible = false;
                            LibraryItemPool.Return(control);
                        }
                    } catch (Exception ex)
                    {
                        Logger.Error(ex, "Failed to update or hide a library item");
                    }
                    control.EndUpdate(true);
                }
            }
            catch (Exception ex) {
                Logger.Error(ex, "Failed to update main content");
            }
            EndUpdate();
        }

        public void BeginUpdate()
        {
            if (UpdateCount++ != 0) return;
            SuspendLayout();
            mainContentPanel.SuspendLayout();
        }

        public void EndUpdate(bool resumeLayout = true)
        {
            if (--UpdateCount != 0) return;
            mainContentPanel.ResumeLayout(resumeLayout);
            ResumeLayout(resumeLayout);
        }

        public void NudgeCurrentPage(uint page) => pageButtonControl1.SilentUpdateCurrentPage(page);

        public void NudgePageCount(uint count) => pageButtonControl1.SilentUpdatePageCount(count);

        private void pageButtonControl1_SizeChanged(object sender, EventArgs e) =>
            // We need to manually center it in the containing panel.
            pageButtonControl1.Left = (buttonsContainer.ClientSize.Width - pageButtonControl1.Width) / 2;//pageButtonControl1.Top = (buttonsContainer.ClientSize.Height - pageButtonControl1.Height) / 2;

        public void AddPageChangeListener(PageButtonControl.PageChangeHandler pageChangedFunc) => pageButtonControl1.PageChange += pageChangedFunc;

        // Form.OnResizeEnd may be better performance wise.
        private void buttonsContainer_SizeChanged(object sender, EventArgs e) =>
            // We need to manually center it in the containing panel.
            // TODO: Hide 
            pageButtonControl1.Left = (buttonsContainer.ClientSize.Width - pageButtonControl1.Width) / 2;//pageButtonControl1.Top = (buttonsContainer.ClientSize.Height - pageButtonControl1.Height) / 2;
    }
}
