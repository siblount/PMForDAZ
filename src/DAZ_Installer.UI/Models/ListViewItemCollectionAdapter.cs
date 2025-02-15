using System.Collections;

namespace DAZ_Installer.UI {
    public class ListViewItemCollectionAdapter : IListViewItemCollection
    {
        private readonly ListView.ListViewItemCollection _collection;

        public ListViewItemCollectionAdapter(ListView.ListViewItemCollection collection)
        {
            _collection = collection;
        }

        public int Count => _collection.Count;
        public bool IsSynchronized => true;
        public object SyncRoot => this;

        public ListViewItem? this[string key] => _collection[key];

        public ListViewItem Add(string? text) => _collection.Add(text);
        public ListViewItem Add(ListViewItem item) => _collection.Add(item);
        public void Clear() => _collection.Clear();
        public bool Contains(ListViewItem item) => _collection.Contains(item);
        public void Remove(ListViewItem item) => _collection.Remove(item);
        public void CopyTo(Array array, int index) => _collection.CopyTo(array, index);
        public IEnumerator GetEnumerator() => _collection.GetEnumerator();
    }
}