using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Serilog;
using DAZ_Installer.Core.Extraction;
using Moq;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.IO;
using DAZ_Installer.Core.Integration;
using System.IO.Compression;
using DAZ_Installer.Core.Tests.RealData;

#pragma warning disable 618
namespace DAZ_Installer.Core.RealData.Tests
{
    /// <summary>
    /// This class uses real archives/products such as from the DAZ store, to make sure that the DPProcessor works correctly. It uses the "manifests" that was generated from
    /// the <see cref="DAZ_Installer.TestingSuiteWindows"/> project/tool to determine which files should be moved into the user's library. 
    /// In order to run these tests, you must have the DAZ_Installer.TestingSuiteWindows project/tool built and run it to generate the manifests.
    /// Then you must copy the manifests into the DAZ_Installer.CoreTests/RealData/Manifests folder.
    /// To protect the copyrights of authors, please do NOT upload the archives online. Test offline with your products.
    /// </summary>
    [TestClass]
    public class DPProcessorTests
    {
        public static readonly string TempPath = Path.Combine(Path.GetTempPath(), "DAZ_Installer.CoreTests", "RealDataDir");
        public static readonly string ArchivesPath = Path.Combine(Environment.CurrentDirectory, "RealData", "Archives");
        public static readonly string ManifestsPath = Path.Combine(Environment.CurrentDirectory, "RealData", "Manifests");
        public static readonly string ExtractPath = Path.Combine(TempPath, "Extract");
        public static readonly DPFileScopeSettings DefaultScope = new(Enumerable.Empty<string>(), new[] { TempPath, ExtractPath }, false);
        public static readonly DPFileSystem FileSystem = new(DefaultScope);
        public static List<DPProcessorTestManifest> Manifests = RealDataHelper.GetValidTestCases(ArchivesPath, ManifestsPath);

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();
        }

        public static IEnumerable<DPProcessorTestManifest> ArchivesToTest => Manifests.ToArray();

        [TestMethod]
        [DynamicData(nameof(ArchivesToTest), typeof(DPProcessorTestManifest), DynamicDataSourceType.Property)]
        public void ProcessArchiveTest(DPProcessorTestManifest manifest)
        {
            var a = new DPArchive(FileSystem.CreateFileInfo(Path.Combine(ArchivesPath, manifest.ArchiveName)));
            var p = new DPProcessor();

            var processSettings = manifest.Settings with { TempPath = TempPath, DestinationPath = ExtractPath };
            p.ProcessArchive(a, processSettings);
        }
    }
}