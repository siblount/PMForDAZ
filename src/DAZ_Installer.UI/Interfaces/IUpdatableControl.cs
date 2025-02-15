using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.UI
{
    /// <summary>
    /// A control that has the BeginUpdate and EndUpdate methods.
    /// </summary>
    public interface IUpdatableControl : IControl
    {
        void BeginUpdate();
        void EndUpdate();
    }
}
