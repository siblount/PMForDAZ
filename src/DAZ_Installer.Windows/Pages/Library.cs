// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using DAZ_Installer.Database;
using DAZ_Installer.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAZ_Installer.Windows.DP;
using Serilog;
using DAZ_Installer.IO;
using System.ComponentModel;
using DAZ_Installer.Common;
using System.Linq;

namespace DAZ_Installer.Windows.Pages
{
    /// <summary>
    /// The Library class is responsible for the loading, adding & removing LibraryItems. It is also responsible for controlling the LibraryPanel and effectively managing image resources. It also controls search interactions. 
    /// </summary>
    public partial class Library : UserControl
    {
        public static Library self;
        protected static Image noImageFound;
        protected const byte MAX_CAPACITY = 25;
        protected List<DPProductRecordLite> ProductRecords { get; set; } = new(MAX_CAPACITY);
        private List<DPProductRecordLite> SearchRecords { get; set; } = new(MAX_CAPACITY);

        internal DPSortMethod SortMethod = DPSortMethod.Date;
        private string lastSearchQuery = string.Empty;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ILogger Logger { get; set; } = Log.ForContext<Library>();
        protected bool SearchMode
        {
            get => searchMode;
            set => libraryPanel1.SearchMode = searchMode = value;
        }
        private bool searchMode;
        private uint lastSearchID = 1;
        // Quick Library Info 
        public Library()
        {
            InitializeComponent();
            self = this;
            SetupSortMethodCombo();
            LoadLibraryItemImages();
        }

        // Called only when visible. Can be loaded but but visible.
        private void Library_Load(object sender, EventArgs e)
        {

            libraryPanel1.CurrentPage = 1;
            Task.Run(LoadLibraryItems);
            libraryPanel1.AddPageChangeListener(UpdatePage);
            Program.Database.ProductRecordAdded += OnAddedProductRecord;
            Program.Database.ProductRecordRemoved += OnRemovedProductRecord;
            Program.Database.ProductRecordModified += OnModifiedProductRecord;
        }

        private void LoadLibraryItemImages()
        {
            thumbnails.Images.Clear();
            thumbnails.Images.Add(Resources.NoImageFound);
            noImageFound = thumbnails.Images[0];
            // DPCommon.WriteToLog("Loaded images.");
        }

        private void LoadLibraryItems()
        {
            if (Program.IsRunByIDE && !IsHandleCreated) return;
            Program.Database.GetProductRecordsQ(SortMethod, libraryPanel1.CurrentPage, 25, 0, OnLibraryQueryUpdate);

            // Invoke or BeginInvoke cannot be called on a control until the window handle has been created.'
            // DPCommon.WriteToLog("Loaded library items.");
        }

        private void SetupSortMethodCombo()
        {
            foreach (var option in Enum.GetNames<DPSortMethod>())
            {
                sortByCombo.Items.Add(option);
            }
            sortByCombo.SelectedItem = Enum.GetName(SortMethod);
        }

        /// <summary>
        ///  Clears the current page library items or search items and handles removing image references.
        /// </summary>
        private void ClearPageContents()
        {
            foreach (var lb in libraryPanel1.LibraryItems)
            {
                var thumbnail = lb.ProductRecord.Thumbnail;
                RemoveReferenceImage(Path.GetFileName(thumbnail));
            }
        }

        private async void OnProductRemovalRequested(LibraryItem item)
        {
            var record = item.ProductRecord;
            try
            {
                var fullRecord = await Program.Database.GetFullProductRecord(record.ID).ConfigureAwait(false) ?? throw new NullReferenceException();
                var fs = new DPFileSystem(new DPFileScopeSettings(Array.Empty<string>(), [fullRecord.Destination], false));
                var result = await DPProductRemover.RemoveProductAsync(record, Program.Database, DPSettings.CurrentSettingsObject,
                    fs).ConfigureAwait(false);
                if (!result.Success) throw new NullReferenceException();
            } catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occurred while attempting to remove a product record");
                MessageBox.Show($"Failed to remove product record.", "Failed to remove product record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void OnProductRecordRemovalRequested(LibraryItem item)
        {
            try
            {
                var result = await DPProductRemover.RemoveRecordAsync(item.ProductRecord, Program.Database).ConfigureAwait(false);
                if (!result) throw new NullReferenceException();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "An unexpected error occurred while attempting to remove a product record");
                MessageBox.Show($"Failed to remove product record.", "Failed to remove product record", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public Image AddReferenceImage(string filePath)
        {
            if (filePath == null) return noImageFound;
            // Key = FileName
            var fileName = Path.GetFileName(filePath);
            lock (thumbnails.Images)
            {
                if (thumbnails.Images.ContainsKey(fileName))
                {
                    var i = thumbnails.Images.IndexOfKey(fileName);
                    return thumbnails.Images[i];
                }
                else
                {
                    // 125, 119
                    using var icon = Image.FromFile(filePath);
                    thumbnails.Images.Add(icon);
                    // Get the last index.
                    var i = thumbnails.Images.Count - 1;
                    thumbnails.Images.SetKeyName(i, fileName);
                    return thumbnails.Images[i];
                }
            }
        }

        private void RemoveReferenceImage(string? imageName)
        {
            if (imageName is null) return;
            lock (thumbnails.Images)
            {
                if (thumbnails.Images.ContainsKey(imageName))
                {
                    thumbnails.Images.RemoveByKey(imageName);
                    thumbnails.Images.Keys.Remove(imageName);
                }
            }
        }

        // Used whenever a change has been made
        // 

        // Try page update
        internal void TryPageUpdate()
        {
            if (InvokeRequired)
            {
                Invoke(TryPageUpdate);
                return;
            }

            Logger.Information("Trying to update page.");
            try
            {
                ClearPageContents();
                var records = searchMode ? SearchRecords : ProductRecords;
                if (searchMode) {
                    var startIndex = (int)(libraryPanel1.CurrentPage - 1) * MAX_CAPACITY;
                    records = [..records.Skip(startIndex).Take(MAX_CAPACITY)];
                }
                // Just update the data and let LibraryPanel handle the UI controls
                libraryPanel1.UpdateMainContent(records, (item) => {
                    // Configure the item with our event handlers and data
                    item.Database = Program.Database;
                    item.MaxTagCount = DPSettings.CurrentSettingsObject.MaxTagsToShow;
                    item.ProductRecordFormType = typeof(ProductRecordForm);
                    item.ProductRemovalRequested = OnProductRemovalRequested;
                    item.ProductRecordRemovalRequested = OnProductRecordRemovalRequested;
                    item.Image = File.Exists(item.ProductRecord.Thumbnail) ? 
                        AddReferenceImage(item.ProductRecord.Thumbnail) : 
                        noImageFound;
                });

                UpdatePageCount();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update page to completion");
            }
        }

        // Used for handling page events.
        // TODO: Potential previous page == the same dispite mode.
        public void UpdatePage(uint page)
        {
            Logger.Debug("Update page called for page no. {page}", page);
            if (!searchMode)
            {
                Program.Database.GetProductRecordsQ(SortMethod, page, 25, callback: OnLibraryQueryUpdate);
            }
            else
            {
                TryPageUpdate();
            }
        }

        private void UpdatePageCount()
        {
            var pageCount = searchMode ?
                (uint)Math.Ceiling(SearchRecords.Count / (float)MAX_CAPACITY) :
                (uint)Math.Ceiling(Program.Database.ProductRecordCount / (float)MAX_CAPACITY);

            if (pageCount != libraryPanel1.PageCount) libraryPanel1.PageCount = pageCount;
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            // Switch modes if search box is empty & we were in search mode previously.
            if (searchBox.Text.Length == 0 && searchMode) SwitchModes(false);
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (searchBox.Text.Length != 0)
                {
                    lastSearchID = (uint)Random.Shared.Next(1, int.MaxValue);
                    lastSearchQuery = searchBox.Text;
                    Program.Database.SearchQ(searchBox.Text, SortMethod, callback: OnSearchUpdate);
                }
            }
        }

        private void SwitchModes(bool toSearch)
        {
            SearchMode = toSearch;
            TryPageUpdate();
        }

        private void OnSearchUpdate(List<DPProductRecordLite> searchResults)
        {
            SearchRecords = searchResults;
            if (!searchMode) SwitchModes(true);
            else TryPageUpdate();
        }

        private void OnLibraryQueryUpdate(List<DPProductRecordLite> productRecords)
        {
            ProductRecords = productRecords;
            if (!searchMode) TryPageUpdate();
        }

        private void OnAddedProductRecord(DPProductRecord record)
        {
            // DPCommon.WriteToLog($"A product has been added! {record.Name}");
            // First, check to see if it is in range of the current page.
            // If it is, then we need to update that page.
            if (record.ID <= libraryPanel1.CurrentPage * 25 && record.ID > (libraryPanel1.CurrentPage - 1) * 25)
            {
                ProductRecords.Add(record.ToLite());
                TryPageUpdate();
            }

            // Otherwise, we may need to change the page count and current page.
            if ((uint)Math.Ceiling((Program.Database.ProductRecordCount + 1) / 25f) != libraryPanel1.PageCount)
            {
                libraryPanel1.NudgePageCount(libraryPanel1.PageCount + 1);
                // Now we need to update the current page.
                // If the ID is higher than the current page range, then we don't do anything.
                // Otherwise, we need to move the current page up one.

                // 1/25/2024: I think this logic is flawed...might remove.
                if (record.ID < libraryPanel1.CurrentPage * 25)
                    libraryPanel1.NudgeCurrentPage(libraryPanel1.CurrentPage + 1);
            }
        }

        private void OnRemovedProductRecord(long ID)
        {
            var collection = SearchMode ? SearchRecords : ProductRecords;
            var lb = libraryPanel1.LibraryItems.FirstOrDefault(l => l.ProductRecord.ID == ID, null!);
            if (lb is null) return;
            var record = lb.ProductRecord;
            DisableLibraryItem(lb);
            collection.RemoveAt(collection.IndexOf(record));
            TryPageUpdate();
        }

        private void OnModifiedProductRecord(DPProductRecord updatedRecord, long oldID)
        {
            var collection = searchMode ? SearchRecords : ProductRecords;
            var i = collection.IndexOf(collection.Find(r => r.ID == oldID));
            if (i == -1) return;
            var lb = libraryPanel1.LibraryItems.FirstOrDefault(l => l.ProductRecord == collection[i], null!);
            if (lb is null) return;
            var liteRecord = updatedRecord.ToLite();
            UpdateLibraryItem(lb, liteRecord);
            collection[i] = liteRecord;
        }

        private void UpdateLibraryItem(LibraryItem lb, DPProductRecordLite record)
        {
            if (InvokeRequired)
            {
                Invoke(UpdateLibraryItem, lb, record);
                return;
            }
            libraryPanel1.BeginUpdate();
            try
            {
                lb.TitleText = record.Name;
                lb.Tags = record.Tags;
                lb.ProductRecord = record;
                lb.Image = File.Exists(record.Thumbnail) ? AddReferenceImage(record.Thumbnail)
                                                            : noImageFound;
            } catch (Exception ex)
            {
                Logger.Error(ex, "Failed to update library item");
            }
            libraryPanel1.EndUpdate();
        }

        private void DisableLibraryItem(LibraryItem lb)
        {
            if (InvokeRequired)
            {
                Invoke(DisableLibraryItem, lb);
                return;
            }
            lb.Enabled = lb.Visible = false;
        }

        private void sortByCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Prevent the database call on initialization for the Library.
            if (!IsHandleCreated) return;

            SortMethod = (DPSortMethod)Enum.Parse(typeof(DPSortMethod), sortByCombo.Text);
            if (searchMode) Program.Database.SearchQ(lastSearchQuery, SortMethod, callback: OnSearchUpdate);
            else Program.Database.GetProductRecordsQ(SortMethod, libraryPanel1.CurrentPage, 25, callback: OnLibraryQueryUpdate);
        }
    }
}
