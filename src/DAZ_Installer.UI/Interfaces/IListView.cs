using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.UI
{
    public interface IListView : IUpdatableControl
    {
        /// <inheritdoc cref="ListView.Groups"/>
        IListViewGroupCollection Groups { get; }
        /// <inheritdoc cref="ListView.Items"/>
        IListViewItemCollection Items { get; }
        /// <inheritdoc cref="ListView.SelectedItems"/>
        ISelectedListViewItemCollection SelectedItems { get; }
    }
}
