// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE
using System;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using DAZ_Installer.DP;

namespace DAZ_Installer
{
    /// <summary>
    /// The Library class is responsible for the loading, adding & removing LibraryItems. It is also responsible for controlling the LibraryPanel and effectively managing image resources. It also controls search interactions. 
    /// </summary>
    public partial class Library : UserControl
    {
        public static Library self;
        protected static Image arrowRight;
        protected static Image arrowDown;
        protected static Image noImageFound;
        protected static Size lastClientSize;
        protected const byte maxImagesLoad = byte.MaxValue;
        protected const byte maxListSize = 25;
        protected byte maxImageFit;
        protected List<LibraryItem> libraryItems { get => libraryPanel1.LibraryItems; }
        protected LibrarySearchItem[] searchItems { get => libraryPanel1.SearchItems; set => libraryPanel1.SearchItems = value; }
        protected DPProductRecord[] ProductRecords { get; set; } = new DPProductRecord[0];
        private DPProductRecord[] lastSearchRecords { get; set; } = new DPProductRecord[0];
        protected Dictionary<uint, List<LibraryItem>> pages = new Dictionary<uint, List<LibraryItem>>();
        
        protected bool mainImagesLoaded = false;

        internal DPSortMethod SearchSortMethod = DPSortMethod.Date;

        protected bool SearchMode
        {
            get => searchMode;
            set => libraryPanel1.SearchMode = searchMode = value;
        }
        private bool searchMode;
        // Quick Library Info 
        public Library()
        {
            InitializeComponent();
            self = this;
            libraryPanel1.CurrentPage = 1;

            Initalize();
            // Am listening...
            libraryPanel1.AddPageChangeListener(UpdatePage);
        }

        // Tasks could be done on main thread or (usually) on another thread.
        private void Initalize()
        {
            DPDatabase.InitializeQ();
            Task.Run(LoadLibraryItemImages)
                .ContinueWith(t => LoadLibraryItems());
            DPDatabase.SearchUpdated += OnSearchUpdate;
            DPDatabase.LibraryQueryCompleted += OnLibraryQueryUpdate;
            
        }

        // Called only when visible. Can be loaded but but visible.
        private void Library_Load(object sender, EventArgs e)
        {

        }

        // Called on a different thread.
        public void LoadLibraryItemImages()
        {
            thumbnails.Images.Add(Properties.Resources.NoImageFound);

            noImageFound = thumbnails.Images[0];
            mainImagesLoaded = true;
            DPCommon.WriteToLog("Loaded images.");

        }

        private void LoadLibraryItems()
        {
            // Read library items file.
            // ReadLibraryItemsFile.

            // Wait for main image loading to be true.
            // TODO: Remove while loop.
            // Shouldn't sleep anymore since its chained with ContinueWith.

            DPDatabase.GetProductRecords(DPSortMethod.Relevance, 25);

            // Invoke or BeginInvoke cannot be called on a control until the window handle has been created.'
            DPCommon.WriteToLog("Loaded library items.");

        }

        /// <summary>
        ///  Clears the current page library items or search items and handles removing image references.
        /// </summary>
        private void ClearPageContents()
        {
            libraryPanel1.EditMode = true;
            if (searchMode)
            {
                foreach (var lb in libraryPanel1.LibraryItems)
                {
                    if (lb == null || lb.ProductRecord == null) continue;

                    lb.Image = null;
                    RemoveReferenceImage(Path.GetFileName(lb.ProductRecord.ThumbnailPath));
                    lb.Dispose();
                }
                libraryPanel1.LibraryItems.Clear();
            } else
            {
                if (searchItems == null)
                {
                    libraryPanel1.EditMode = false;
                    return;
                }
                foreach (var lb in libraryPanel1.SearchItems)
                {
                    if (lb == null || lb.SearchRecord == null) continue;

                    lb.Image = null;
                    RemoveReferenceImage(Path.GetFileName(lb.SearchRecord.ThumbnailPath));
                    lb.Dispose();
                }
                ArrayHelper.ClearArray(libraryPanel1.SearchItems);
            }
            
            libraryPanel1.EditMode = false;
            // Get the current page contents.
        }

        internal LibrarySearchItem AddNewSearchItem(DPProductRecord record)
        {
            if (InvokeRequired) 
                return (LibrarySearchItem)Invoke(new Func<DPProductRecord, LibrarySearchItem>(AddNewSearchItem), record);

            var searchItem = new LibrarySearchItem();
            searchItem.TitleText = record.ProductName;
            searchItem.Tags = record.Tags;
            searchItem.Dock = DockStyle.Top;
            searchItem.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            var openSlot = ArrayHelper.GetNextOpenSlot(searchItems);
            if (openSlot != -1) searchItems[openSlot] = searchItem;

            return searchItem;
        }

        internal LibraryItem AddNewLibraryItem(DPProductRecord record)
        {
            if (InvokeRequired)
            {
                return (LibraryItem)Invoke(new Func<DPProductRecord, LibraryItem>(AddNewLibraryItem), record);
            }
            var lb = new LibraryItem();
            lb.TitleText = record.ProductName;
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
            if (thumbnails.Images.ContainsKey(fileName))
            {
                var i = thumbnails.Images.IndexOfKey(fileName);
                return thumbnails.Images[i];
            } else
            {
                // 125, 119
                var icon = Image.FromFile(filePath);
                thumbnails.Images.Add(icon);
                // Get the last index.
                var i = thumbnails.Images.Count - 1;
                thumbnails.Images.SetKeyName(i, fileName);
                return thumbnails.Images[i];
            }
        }

        public void RemoveReferenceImage(string imageName)
        {
            if (thumbnails.Images.ContainsKey(imageName))
            {
                thumbnails.Images.RemoveByKey(imageName);
                thumbnails.Images.Keys.Remove(imageName);
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
            try
            {
                ClearPageContents();
                ClearLibraryItems();
                if (searchMode) AddSearchItems();
                else AddLibraryItems();
                // TO DO : Check if we need to move to the left page.
                // Example - There are no library items on current page (invalid page) and no pages above it.
                UpdatePageCount();
                if (InvokeRequired) Invoke(libraryPanel1.UpdateMainContent);
                else libraryPanel1.UpdateMainContent();
            } catch { }
        }
        public void ForcePageUpdate()
        {
            if (InvokeRequired) {Invoke(ForcePageUpdate); return; }
            DPCommon.WriteToLog("force page update called.");
            ClearPageContents();
            ClearLibraryItems();
            AddLibraryItems();
            // TO DO : Check if we need to move to the left page.
            // Example - There are no library items on current page (invalid page) and no pages above it.
            UpdatePageCount();
            libraryPanel1.UpdateMainContent();
            
        }

        // Used for handling page events.
        // TODO: Potential previous page == the same dispite mode.
        public void UpdatePage(int page) {
            DPCommon.WriteToLog("page update called.");
            if (page == libraryPanel1.PreviousPage) return;

            ClearPageContents();
            if (searchMode) AddSearchItems();
            else AddLibraryItems();
            libraryPanel1.UpdateMainContent();
        }

        private void UpdatePageCount()
        {
            int pageCount = searchMode ?
                (int)Math.Ceiling(lastSearchRecords.Length / 25f) :
                (int)Math.Ceiling(ProductRecords.Length / 25f);

            if (pageCount != libraryPanel1.PageCount) libraryPanel1.PageCount = pageCount;
        }

        private void AddLibraryItems()
        {
            DPCommon.WriteToLog("Add library items.");
            libraryPanel1.EditMode = true;
            // Loop while i is less than records count and count is less than 25.
            for (var i = 0; i < ProductRecords.Length; i++)
            {
                var record = ProductRecords[i];
                var lb = AddNewLibraryItem(record);
                lb.ProductRecord = record;

                lb.Image = File.Exists(record.ThumbnailPath) ? AddReferenceImage(record.ThumbnailPath) 
                                                            : noImageFound;

            }
            libraryPanel1.EditMode = false;
        }

        private void AddSearchItems()
        {
            DPCommon.WriteToLog("Add search items.");
            var startRecordIndex = (libraryPanel1.CurrentPage - 1) * 25; // Current Page never 0.
            byte count = 0;
            libraryPanel1.EditMode = true;
            // Loop while i is less than records count and count is less than 25.
            for (var i = startRecordIndex; i < lastSearchRecords.Length && count < 25; i++, count++)
            {
                var record = lastSearchRecords[i];
                var searchItem = AddNewSearchItem(record);
                searchItems[count] = searchItem;

                // Check if image exists.
                if (File.Exists(record.ThumbnailPath))
                {
                    var image = AddReferenceImage(record.ThumbnailPath);
                    searchItem.Image = image;
                }
                else searchItem.Image = noImageFound;
            }
            libraryPanel1.EditMode = false;
        }
        
        // Possible race condition: ForcePageUpdate() from initailization.
        // TODO: Fix ^

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            Forms.DatabaseView databaseView = new Forms.DatabaseView();
            databaseView.ShowDialog();
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (searchBox.Text.Length != 0)
                    DPDatabase.Search(searchBox.Text);
            }
        }

        private void SwitchModes(bool toSearch)
        {
            SearchMode = toSearch;
            TryPageUpdate();
        }

        private void OnSearchUpdate(DPProductRecord[] searchResults)
        {
            lastSearchRecords = searchResults;
            if (!searchMode) SwitchModes(true);
            else TryPageUpdate();
        }

        private void OnLibraryQueryUpdate(DPProductRecord[] productRecords)
        {
            ProductRecords = productRecords;
            if (!searchMode) TryPageUpdate();
        }

        public void ClearLibraryItems() => libraryItems.Clear();

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            // Switch modes if search box is empty & we were in search mode previously.
            if (searchBox.Text.Length == 0 && searchMode) SwitchModes(false);
        }

        public void InformLibraryUpdate()
        {
            if (!searchMode)
            {
                DPDatabase.GetProductRecords(DPSortMethod.None, 25);
            }
        }
    }
}
