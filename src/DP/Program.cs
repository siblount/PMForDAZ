// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Threading;
using System.Windows.Forms;

namespace DAZ_Installer
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var mutex = new Mutex(false, "DAZ_Installer Instance"))
            {
                // Code from: https://saebamini.com/Allowing-only-one-instance-of-a-C-app-to-run/
                bool isAnotherInstanceOpen = !mutex.WaitOne(0);
                if (isAnotherInstanceOpen)
                {
                    MessageBox.Show(null, "Only one instance of Daz Product Installer is allowed!", "Launch cancelled", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DP.DPGlobal.mainThreadID = Thread.CurrentThread.ManagedThreadId;
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
                
                mutex.ReleaseMutex();
            }

        }
    }
}
