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

namespace DAZ_Installer.Windows.Pages
{
    /// <summary>
    /// The Library class is responsible for the loading, adding & removing LibraryItems. It is also responsible for controlling the LibraryPanel and effectively managing image resources. It also controls search interactions. 
    /// </summary>
    public partial class Library : UserControl
    {
        public static Library self;
        protected static Image noImageFound;
        protected static Size lastClientSize;
        protected const byte maxImagesLoad = byte.MaxValue;
        protected const byte MAX_CAPACITY = 25;
        protected byte maxImageFit;
        protected List<LibraryItem> libraryItems => libraryPanel1.LibraryItems;
        protected List<LibraryItem> searchItems { get => libraryPanel1.SearchItems; set => libraryPanel1.SearchItems = value; }
        protected List<DPProductRecordLite> ProductRecords { get; set; } = new(MAX_CAPACITY);
        private List<DPProductRecordLite> SearchRecords { get; set; } = new(MAX_CAPACITY);
        protected bool mainImagesLoaded = false;

        internal DPSortMethod SortMethod = DPSortMethod.Date;
        private string lastSearchQuery = string.Empty;

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

        // Called on a different thread.
        private void LoadLibraryItemImages()
        {
            thumbnails.Images.Clear();
            thumbnails.Images.Add(Resources.NoImageFound);
            noImageFound = thumbnails.Images[0];

            mainImagesLoaded = true;
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
            foreach (var option in Enum.GetNames(typeof(DPSortMethod)))
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
            libraryPanel1.EditMode = true;
            if (searchMode)
            {
                foreach (LibraryItem lb in libraryPanel1.LibraryItems)
                {
                    if (lb == null || lb.ProductRecord == null) continue;

                    lb.Image = null;
                    RemoveReferenceImage(Path.GetFileName(lb.ProductRecord.Thumbnail));
                    lb.Dispose();
                }
                libraryPanel1.LibraryItems.Clear();
            }
            else
            {
                if (searchItems == null)
                {
                    libraryPanel1.EditMode = false;
                    return;
                }
                foreach (LibraryItem lb in libraryPanel1.SearchItems)
                {
                    if (lb == null || lb.ProductRecord == null) continue;

                    lb.Image = null;
                    RemoveReferenceImage(Path.GetFileName(lb.ProductRecord.Thumbnail));
                    lb.Dispose();
                }
                libraryPanel1.SearchItems.Clear();
            }
            libraryPanel1.EditMode = false;
        }

        internal LibraryItem AddNewSearchItem(DPProductRecordLite record)
        {
            if (InvokeRequired)
                return (LibraryItem)Invoke(new Func<DPProductRecordLite, LibraryItem>(AddNewSearchItem), record);

            var searchItem = new LibraryItem();
            searchItem.TitleText = record.Name;
            searchItem.MaxTagCount = DPSettings.CurrentSettingsObject.MaxTagsToShow;
            searchItem.Tags = record.Tags;
            searchItem.Dock = DockStyle.Top;
            searchItem.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            searchItems.Add(searchItem);

            return searchItem;
        }

        internal LibraryItem AddNewLibraryItem(DPProductRecordLite record)
        {
            if (InvokeRequired)
            {
                return (LibraryItem)Invoke(new Func<DPProductRecordLite, LibraryItem>(AddNewLibraryItem), record);
            }
            var lb = new LibraryItem();
            lb.Database = Program.Database;
            lb.ProductRecordFormType = typeof(ProductRecordForm);
            lb.TitleText = record.Name;
            lb.MaxTagCount = DPSettings.CurrentSettingsObject.MaxTagsToShow;
            lb.Tags = record.Tags;
            lb.Dock = DockStyle.Top;
            lb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            lb.Image = noImageFound;

            if (libraryItems.Count != libraryItems.Capacity) libraryItems.Add(lb);
            return lb;
        }

        public LibraryItem AddNewLibraryItem(string title, string[] tags, string[] folders)
        {
            if (InvokeRequired)
            {
                return (LibraryItem)Invoke(new Func<string, string[], string[], LibraryItem>(AddNewLibraryItem), title, tags, folders);
            }
            var lb = new LibraryItem();
            lb.TitleText = title;
            lb.Tags = tags;
            lb.ProductRecordFormType = typeof(ProductRecordForm);
            //lb.Folders = folders;
            lb.Dock = DockStyle.Top;
            lb.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            lb.Image = noImageFound;

            if (libraryItems.Count != libraryItems.Capacity) libraryItems.Add(lb);

            return lb;
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

        public void RemoveReferenceImage(string imageName)
        {
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
            Log.ForContext<Library>().Information("Trying to update page.");
            try
            {
                ClearPageContents();
                ClearLibraryItems();
                ClearSearchItems();
                if (searchMode) AddSearchItems();
                else AddLibraryItems();
                // TO DO : Check if we need to move to the left page.
                // Example - There are no library items on current page (invalid page) and no pages above it.
                UpdatePageCount();
                if (InvokeRequired) Invoke(libraryPanel1.UpdateMainContent);
                else libraryPanel1.UpdateMainContent();
            }
            catch { }
        }
        public void ForcePageUpdate()
        {
            if (InvokeRequired) { Invoke(ForcePageUpdate); return; }
            Log.ForContext<Library>().Information("Forcing page update");

            // DPCommon.WriteToLog("force page update called.");
            ClearPageContents();
            ClearSearchItems();
            ClearLibraryItems();
            AddLibraryItems();
            // TO DO : Check if we need to move to the left page.
            // Example - There are no library items on current page (invalid page) and no pages above it.
            UpdatePageCount();
            libraryPanel1.UpdateMainContent();

        }

        // Used for handling page events.
        // TODO: Potential previous page == the same dispite mode.
        public void UpdatePage(uint page)
        {
            // DPCommon.WriteToLog("page update called.");
            // if (page == libraryPanel1.PreviousPage) return;

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
                (uint)Math.Ceiling(SearchRecords.Count / 25f) :
                (uint)Math.Ceiling(Program.Database.ProductRecordCount / 25f);

            if (pageCount != libraryPanel1.PageCount) libraryPanel1.PageCount = pageCount;
        }

        private void AddLibraryItems()
        {
            // DPCommon.WriteToLog("Add library items.");
            libraryPanel1.EditMode = true;
            // Loop while i is less than records count and count is less than 25.
            for (var i = 0; i < ProductRecords.Count; i++)
            {
                DPProductRecordLite record = ProductRecords[i];
                LibraryItem lb = AddNewLibraryItem(record);
                lb.ProductRecord = record;

                lb.Image = File.Exists(record.Thumbnail) ? AddReferenceImage(record.Thumbnail)
                                                            : noImageFound;

            }
            libraryPanel1.EditMode = false;
        }

        private void AddSearchItems()
        {
            // DPCommon.WriteToLog("Add search items.");
            libraryPanel1.EditMode = true;
            // Loop while i is less than records count and count is less than 25.
            var startIndex = (libraryPanel1.CurrentPage - 1) * 25;
            var count = 0;
            for (var i = startIndex; i < SearchRecords.Count && count < 25; i++, count++)
            {
                DPProductRecordLite record = SearchRecords[(int)i];
                LibraryItem lb = AddNewSearchItem(record);
                lb.ProductRecord = record;

                lb.Image = File.Exists(record.Thumbnail) ? AddReferenceImage(record.Thumbnail)
                                                            : noImageFound;
            }
            libraryPanel1.EditMode = false;
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
            if (record.ID <= (libraryPanel1.CurrentPage) * 25 && record.ID > (libraryPanel1.CurrentPage - 1) * 25)
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
            var lb = libraryPanel1.LibraryItems.Find(l => l.ProductRecord.ID == ID);
            if (lb is null) return;
            var record = lb.ProductRecord;
            DisableLibraryItem(lb);
            collection.RemoveAt(collection.IndexOf(record));
            TryPageUpdate();
        }

        private void OnModifiedProductRecord(DPProductRecord updatedRecord, long oldID)
        {
            List<DPProductRecordLite> collection = searchMode ? SearchRecords : ProductRecords;
            var i = collection.IndexOf(collection.Find(r => r.ID == oldID));
            if (i == -1) return;
            LibraryItem? lb = libraryPanel1.LibraryItems.Find(l => l.ProductRecord == SearchRecords[i]);
            if (lb is null) return;
            var liteRecord = updatedRecord.ToLite();
            UpdateLibraryItem(lb, liteRecord);
            collection[i] = liteRecord;
        }

        public void ClearLibraryItems() => libraryItems.Clear();
        public void ClearSearchItems() => searchItems.Clear();

        private void UpdateLibraryItem(LibraryItem lb, DPProductRecordLite record)
        {
            if (InvokeRequired)
            {
                Invoke(UpdateLibraryItem, lb, record);
                return;
            }
            libraryPanel1.EditMode = true;
            lb.TitleText = record.Name;
            lb.Tags = record.Tags;
            lb.ProductRecord = record;
            lb.Image = File.Exists(record.Thumbnail) ? AddReferenceImage(record.Thumbnail)
                                                        : noImageFound;
            libraryPanel1.EditMode = false;
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
