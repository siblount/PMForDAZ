using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DAZ_Installer.UI
{
    public interface IListViewGroupCollection : ICollection
    {
        ListViewGroup Add(string key, string title);
        void Clear();
        ListViewGroup? this[string key] { get; }
    }
}
