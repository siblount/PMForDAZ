using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Windows.DP
{
    /// <summary>
    /// This interface is used to gather important registry values related to DAZ Studio.
    /// </summary>
    public interface IPlatformRegistry
    {
        /// <summary>
        /// The DAZ Content Directories. May be empty if none found.
        /// </summary>
        string[] ContentDirectories { get; }
        /// <summary>
        /// The application path to DAZ Studio. 
        /// Value may be <see cref="string.Empty"/> if not found.
        /// </summary>
        string DazAppPath { get; }
        /// <summary>
        /// Checks the Registry again and updates properties accordingly.
        /// </summary>
        void Refresh();
    }
}
