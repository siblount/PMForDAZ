// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using DAZ_Installer.WinApp.Forms;

namespace DAZ_Installer
{
    static class Program
    {
        public static bool IsRunByIDE => Debugger.IsAttached;
        public static DragDropEffects DropEffect = DragDropEffects.All;
        public static int MainThreadID { get; private set; } = 0;
        public static bool IsOnMainThread
        {
            get => MainThreadID == Thread.CurrentThread.ManagedThreadId;
        }
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (CheckInstances()) return;
            using var mutex = new Mutex(false, "DAZ_Installer Instance");
            mutex.WaitOne(0);
            // Set the main thread ID to this one.
            MainThreadID = Thread.CurrentThread.ManagedThreadId;
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Checks if there is a instance of the application running. 
        /// </summary>
        /// <returns>True if there the app is already running, otherwise false.</returns>
        static bool CheckInstances()
        {
            using (var mutex = new Mutex(false, "DAZ_Installer Instance"))
            {
                // Code from: https://saebamini.com/Allowing-only-one-instance-of-a-C-app-to-run/
                bool isAnotherInstanceOpen = !mutex.WaitOne(0);
                if (isAnotherInstanceOpen)
                {
                    MessageBox.Show(null, "Only one instance of Daz Product Installer is allowed!", "Launch cancelled", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return true;
                }

                mutex.ReleaseMutex();
            }
            return false;
        }
    }
}
