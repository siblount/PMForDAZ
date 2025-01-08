// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Windows.Forms;

namespace DAZ_Installer.Windows.DP
{
    public static class DPGlobal
    {
        internal static int mainThreadID = 0;
        public static bool appClosing { get; set; } = false;
        public static event Action<FormClosingEventArgs> AppClosing;
        public static bool isWindows11 = false;
        // TODO: Handle closing while database is active.
        public static void HandleAppClosing(FormClosingEventArgs e)
        {
            appClosing = true;
            AppClosing?.Invoke(e);
        }
    }
}
