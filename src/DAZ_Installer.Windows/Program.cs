// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using DAZ_Installer.Database;
using DAZ_Installer.Windows.Forms;
using System;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Serilog;
using Serilog.Templates;
using System.Reflection;
using System.Threading.Tasks;
using System.Data.Entity;
using DAZ_Installer.Windows.DP;

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
        public static ILogger Logger => Log.ForContext(typeof(Program));
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
            Logger.Information("Starting application");
            Logger.Information("App Version: {0} {1}", AppVersion, VersionSuffix);
            if (CheckInstances()) return;
            if (DatabaseUpdateRequired().Result == false) return;
            Thread.CurrentThread.Name = "Main";
            using var mutex = new Mutex(false, "DAZ_Installer Instance");
            mutex.WaitOne(0);
            InitSettings();
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
                MessageBox.Show($"Oops! A fatal error occurred that requires the application to shut down. Error:\n{ex.Message}", 
                    "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Fatal(ex, "Application shutdown due to fatal error");
            }
            finally {
                mutex.ReleaseMutex();
            }
        }

        static async Task<bool> DatabaseUpdateRequired()
        {
            if (Database.UpdateRequired)
            {
                Logger.Information("Database update is required");
                var result = MessageBox.Show($"A database update is required before starting this application. " +
                    $"First, a backup of the database will be saved, then the update will proceed.\n\n" +
                    $"Even though, a backup may be saved, as an extra precaution, it is highly recommended to manually backup the database located at: " +
                    $"\n\n{Path.GetFullPath(Database.Path)}.\n\n" +
                    $"Do you wish to start the update now? This may take a few minutes.", 
                    "Database Update Required", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No) return false;
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMinutes(20));
                var backupResult = await Database.BackupDatabaseQ().ConfigureAwait(false);
                if (!backupResult)
                {
                    Logger.Warning("Database did not backup database successfully during database update procedure");
                    result = MessageBox.Show($"Failed to backup the database. It is still possible to update the database. Do you wish to proceed or cancel?", 
                        "Backup failed", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                    if (result == DialogResult.No) return false;
                }
                await Database.UpdateDatabase(cts.Token).ConfigureAwait(false);
                await Database.RefreshDatabaseQ(true).ConfigureAwait(false);
                if (Database.UpdateRequired)
                {
                    Logger.Warning("The database update was not successful.");
                    MessageBox.Show("The database update was not successful. The changes to the database have not been applied. The application will now close.", "Database Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                MessageBox.Show("The database update was a success. The application will now start.", "Database Update Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            return true;
        }

        static void InitSettings() {
            try {
                var fileInfo = new FileInfo(DPSettings.SETTINGS_PATH);
                if (!fileInfo.Exists) DPSettings.CurrentSettingsObject = new DPSettings();
                using var txt = fileInfo.OpenText();
                DPSettings.CurrentSettingsObject = DPSettings.FromJson(txt.ReadToEnd());
                if (DPSettings.CurrentSettingsObject == null)
                {
                    Logger.Warning("Failed to load settings from disk. Using default settings.");
                    MessageBox.Show("Failed to load settings from disk. Using default settings.", "Settings Load Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DPSettings.CurrentSettingsObject = new DPSettings();
                }
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to load settings from disk. Using default settings.");  
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
                Logger.Warning("User attempted to launch another instance of the application.");
                MessageBox.Show(null, "Only one instance of Daz Product Installer is allowed!", "Launch cancelled", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }

            mutex.ReleaseMutex();
            return false;
        }
    }
}
