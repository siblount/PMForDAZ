using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.UI
{
    public class ListViewGroupCollectionAdapter : IListViewGroupCollection
    {
        private readonly ListViewGroupCollection _collection;

        public ListViewGroupCollectionAdapter(ListViewGroupCollection collection)
        {
            _collection = collection;
        }

        public int Count => _collection.Count;
        public bool IsSynchronized => true;
        public object SyncRoot => this;

        public ListViewGroup? this[string key] => _collection[key];

        public ListViewGroup Add(string key, string title) => _collection.Add(key, title);
        public void Clear() => _collection.Clear();
        public bool Contains(ListViewGroup item) => _collection.Contains(item);
        public void Remove(ListViewGroup item) => _collection.Remove(item);
        public void CopyTo(Array array, int index) => _collection.CopyTo(array, index);
        public IEnumerator GetEnumerator() => _collection.GetEnumerator();
    }
}
