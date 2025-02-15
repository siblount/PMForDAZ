using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.UI
{
    public interface ISelectedListViewItemCollection : ICollection
    {
        /// <inheritdoc cref="ListView.SelectedListViewItemCollection.this[int]"/>
        ListViewItem this[int key] { get; }
    }
}
