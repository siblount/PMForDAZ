using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Serilog;
using DAZ_Installer.Core.Extraction;
using Moq;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.IO;
using DAZ_Installer.Core.Tests;
using System.IO.Compression;

#pragma warning disable 618
namespace DAZ_Installer.Core.Integration.Tests
{
    [TestClass]
    public class DPProcessorTests
    {
        public static readonly string TempPath = Path.Combine(Path.GetTempPath(), "DAZ_Installer.CoreTests", "Integration");
        public static readonly string ArchivePath = Path.Combine(TempPath, "Test Archive.zip");
        public static readonly string ArchiveContentsPath = Path.Combine(TempPath, "Archive Contents");
        public static readonly string ExtractPath = Path.Combine(TempPath, "Extract");
        public static readonly DPFileScopeSettings DefaultScope = new(Enumerable.Empty<string>(), new[] { ExtractPath }, false);
        public static readonly DPFileSystem FileSystem = new DPFileSystem(DefaultScope);
        public static List<string> ArchiveContents = new(5);
        static readonly DPProcessSettings DefaultProcessSettings = new(TempPath, ExtractPath, InstallOptions.ManifestAndAuto);

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();
            ArchiveContents = DPIntegrationArchiveHelpers.CreateArchiveContents(ArchiveContentsPath);
            ZipFile.CreateFromDirectory(ArchiveContentsPath, ArchivePath, CompressionLevel.NoCompression, false);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Directory.Delete(TempPath, true);
            Directory.Delete(ExtractPath, true);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.Delete(ExtractPath, true);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Directory.CreateDirectory(ExtractPath);
        }

        [TestMethod]
        public void ProcessArchiveTest()
        {
            var a = new DPArchive(FileSystem.CreateFileInfo(ArchivePath));
            var p = new DPProcessor();
            var settings = DPProcessorTestHelpers.CreateExtractSettings(ArchiveContents, a);
            var expectedFiles = new[] { "data/TheRealSolly/data.dsf", "data/TheRealSolly/a.txt", "docs/b.txt" };
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectArchiveProcessed = new() { { a.FileName, DPProcessorTestHelpers.CreateExtractionReport(settings, Enumerable.Empty<string>(), expectedFiles) } }
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);

            p.ProcessArchive(a, DefaultProcessSettings);
        }
    }
}