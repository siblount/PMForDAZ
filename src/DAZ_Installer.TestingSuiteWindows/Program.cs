using Serilog;
using DAZ_Installer.Core;

namespace DAZ_Installer.TestingSuiteWindows
{
    internal static class Program
    {
        internal static readonly string TempPath = Path.Combine(Path.GetTempPath(), "DAZ_Installer_TestingSuiteWindows");
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
#if DEBUG
                .WriteTo.Debug(SerilogLoggerConstants.Template)
#endif
                .WriteTo.Sink(new RichTextBoxSink())
                .WriteTo.Async(a => a.File(SerilogLoggerConstants.Template, "log.txt",
                                           fileSizeLimitBytes: 20 * 1024 * 1024, // 20 MB
                                           rollOnFileSizeLimit: true,
                                           retainedFileCountLimit: 5,
                                           retainedFileTimeLimit: TimeSpan.FromDays(5)),
                blockWhenFull: true)
                .CreateLogger();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}