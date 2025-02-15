using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.UI
{
    public sealed class ListViewAdapter(ListView listView) : IListView
    {
        private readonly ListView listView = listView;

        public static implicit operator ListView(ListViewAdapter adapter) => adapter.listView;
        public bool InvokeRequired => listView.InvokeRequired;

        public IListViewGroupCollection Groups => new ListViewGroupCollectionAdapter(listView.Groups);

        public IListViewItemCollection Items => new ListViewItemCollectionAdapter(listView.Items);

        public ISelectedListViewItemCollection SelectedItems => new SelectedListViewItemCollectionAdapter(listView.SelectedItems);

        public IAsyncResult BeginInvoke(Action action) => listView.BeginInvoke(action);
        public void Invoke(Action action) => listView.Invoke(action);
        public void BeginUpdate() => listView.BeginUpdate();
        public void EndUpdate() => listView.EndUpdate();
        public void ResumeLayout() => listView.ResumeLayout();
        public void SuspendLayout() => listView.SuspendLayout();
    }
}
