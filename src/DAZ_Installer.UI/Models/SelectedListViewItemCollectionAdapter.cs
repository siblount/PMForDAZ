using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.UI
{
    public class SelectedListViewItemCollectionAdapter : ISelectedListViewItemCollection
    {
        private readonly ListView.SelectedListViewItemCollection _collection;

        public SelectedListViewItemCollectionAdapter(ListView.SelectedListViewItemCollection collection)
        {
            _collection = collection;
        }

        public int Count => _collection.Count;
        public bool IsSynchronized => true;
        public object SyncRoot => this;
        public ListViewItem this[int key] => _collection[key]!;
        public void CopyTo(Array array, int index) => _collection.CopyTo(array, index);
        public IEnumerator GetEnumerator() => _collection.GetEnumerator();
    }
}
