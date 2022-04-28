// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DAZ_Installer.DP
{
    static class DPGlobal
    {
        public static Dictionary<uint, IDPWorkingFile> dpObjects = new Dictionary<uint, IDPWorkingFile>();

        // Hold queues in case an attmept is made to extract files while an extraction process is on-going.
        // TO DO: Delete.
        public static Queue<DPExtractJob> pendingRequests { get; } = new Queue<DPExtractJob>();
        public static bool appClosing { get; set; } = false;
        public static event Action AppClosing;

        public static void HandleAppClosing()
        {
            appClosing = true;
            AppClosing?.Invoke();
        }
    }
}
