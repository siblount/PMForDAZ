// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;

namespace DAZ_Installer.DP
{
    static class DPGlobal
    {
        internal static int mainThreadID = 0;
        public static Dictionary<uint, IDPWorkingFile> dpObjects = new Dictionary<uint, IDPWorkingFile>();
        public static bool appClosing { get; set; } = false;
        public static event Action<FormClosingEventArgs> AppClosing;

        public static void HandleAppClosing(FormClosingEventArgs e)
        {
            appClosing = true;
            AppClosing?.Invoke(e);
        }
    }
}
