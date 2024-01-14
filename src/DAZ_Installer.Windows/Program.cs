// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Database;
using DAZ_Installer.Windows.Forms;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Serilog;
using Serilog.Templates;
using System.Reflection;

namespace DAZ_Installer.Windows
{
    static class Program
    {
        public static readonly string AppName = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;
        public static readonly string AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public static readonly string Authors = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company;
        public static readonly string VersionSuffix = "Pre-Alpha";
        public static bool IsRunByIDE => Debugger.IsAttached;
        public static readonly DragDropEffects DropEffect = DragDropEffects.All;
        public static int MainThreadID { get; private set; } = 0;
        public static bool IsOnMainThread => MainThreadID == Environment.CurrentManagedThreadId;
        public static DPDatabase Database { get; private set; } = new DPDatabase("Database/db.db");
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .MinimumLevel.Debug()
#if DEBUG
                .WriteTo.Debug(SerilogLoggerConstants.Template)
#endif
                .WriteTo.Async(a => a.File(SerilogLoggerConstants.Template, "log.txt",
                                           fileSizeLimitBytes: 20 * 1024 * 1024, // 20 MB
                                           rollOnFileSizeLimit: true,
                                           retainedFileCountLimit: 5,
                                           retainedFileTimeLimit: TimeSpan.FromDays(5)),
                blockWhenFull: true)
                .CreateLogger();
            Log.ForContext(typeof(Program)).Information("Starting application");
            if (CheckInstances()) return;
            Thread.CurrentThread.Name = "Main";
            using var mutex = new Mutex(false, "DAZ_Installer Instance");
            mutex.WaitOne(0);
            // Set the main thread ID to this one.
            MainThreadID = Environment.CurrentManagedThreadId;
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                // TODO: Show error handler here.
                Log.ForContext(typeof(Program)).Fatal(ex, "Application shutdown due to fatal error.");
            }
            finally {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Checks if there is a instance of the application running. 
        /// </summary>
        /// <returns>True if there the app is already running, otherwise false.</returns>
        static bool CheckInstances()
        {
            using var mutex = new Mutex(false, "DAZ_Installer Instance");
            // Code from: https://saebamini.com/Allowing-only-one-instance-of-a-C-app-to-run/
            var isAnotherInstanceOpen = !mutex.WaitOne(0);
            if (isAnotherInstanceOpen)
            {
                Log.Warning("User attempted to launch another instance of the application.");
                MessageBox.Show(null, "Only one instance of Daz Product Installer is allowed!", "Launch cancelled", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }

            mutex.ReleaseMutex();
            return false;
        }
    }
}
