using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.UI
{
    /// <inheritdoc cref="ListView.ListViewItemCollection"/>
    public interface IListViewItemCollection : ICollection
    {
        /// <inheritdoc cref="ListView.ListViewItemCollection.Add(string?)"/>
        ListViewItem Add(string? text);
        /// <inheritdoc cref="ListView.ListViewItemCollection.Add(ListViewItem)"/>
        ListViewItem Add(ListViewItem item);
        /// <inheritdoc cref="ListView.ListViewItemCollection.Clear()"/>
        void Clear();
        bool Contains(ListViewItem item);
        /// <inheritdoc cref="ListView.ListViewItemCollection.Remove(ListViewItem)"/>
        void Remove(ListViewItem item);
        /// <inheritdoc cref="ListView.ListViewItemCollection.this[string]"/>

        ListViewItem? this[string key] { get; }
    }
}
