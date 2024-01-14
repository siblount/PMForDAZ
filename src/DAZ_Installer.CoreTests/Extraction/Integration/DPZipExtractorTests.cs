using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Moq;
using Serilog;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.IO;
using DAZ_Installer.CoreTests.Extraction;
using DAZ_Installer.Core.Extraction.Fakes;
using DAZ_Installer.Core.Integration;
using System.IO;
using System.IO.Compression;

#pragma warning disable CS0618 // Obsolete is for production code, not testing code.
namespace DAZ_Installer.Core.Extraction.Integration.Tests
{
    [TestClass]
    public class DPZipExtractorTests
    {
        public static readonly string TempPath = Path.Combine(Path.GetTempPath(), "DAZ_Installer.CoreTests", "Extraction", "Integration", "Test Subjects");
        public static readonly string ArchivePath = Path.Combine(TempPath, "Test Archive.zip");
        public static readonly string ArchiveContentsPath = Path.Combine(TempPath, "Archive Contents");
        public static readonly string ExtractPath = Path.Combine(Path.GetTempPath(), "DAZ_InstallerTests", "Extract");
        public static readonly DPFileScopeSettings DefaultScope = new(Enumerable.Empty<string>(), new[] { ExtractPath }, false);
        public static readonly DPFileSystem FileSystem = new DPFileSystem(DefaultScope);
        public static List<string> ArchiveContents = new(5);

        /// <summary>
        /// A factory that returns a mocked fake archive with the default contents.
        /// </summary>
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
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
        public void DPZipExtractorTest()
        {
            var l = Mock.Of<ILogger>();
            var f = Mock.Of<IZipArchiveFactory>();
            var e = new DPZipExtractor(l, f);
            Assert.AreEqual(l, e.Logger);
            Assert.AreEqual(f, e.Factory);
        }

        [TestMethod]
        public void ExtractTest()
        {
            var fi = FileSystem.CreateFileInfo(ArchivePath);
            var e = new DPZipExtractor(Log.Logger, new ZipArchiveWrapperFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_AfterExtract()
        {
            var fi = FileSystem.CreateFileInfo(ArchivePath);
            var e = new DPZipExtractor(Log.Logger, new ZipArchiveWrapperFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);
            arc.Extract(settings);

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_AfterExtractTemp()
        {
            var fi = FileSystem.CreateFileInfo(ArchivePath);
            var e = new DPZipExtractor(Log.Logger, new ZipArchiveWrapperFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);
            arc.ExtractToTemp(settings);

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_CancelledBeforeOp()
        {
            var fi = FileSystem.CreateFileInfo(ArchivePath);
            var e = new DPZipExtractor(Log.Logger, new ZipArchiveWrapperFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            e.CancellationToken = new CancellationToken(true);
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);

            // Testing Extract() here:
            // TODO: ExtractFinish should be called once Extracting event is emitted.
            var report = e.Extract(settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_CancelledDuringOp()
        {
            var fi = FileSystem.CreateFileInfo(ArchivePath);
            var cts = new CancellationTokenSource();
            var e = new DPZipExtractor(Log.Logger, new ZipArchiveWrapperFactory()) { CancellationToken = cts.Token };
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            e.ExtractProgress += (_, _) => cts.Cancel();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(1) { arc.Contents.First().Value }, ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);


            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }


        [TestMethod]
        public void ExtractToTempTest()
        {
            var fi = FileSystem.CreateFileInfo(ArchivePath);
            var e = new DPZipExtractor(Log.Logger, new ZipArchiveWrapperFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPathsForTemp(arc, ExtractPath);

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings, true);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);

        }
        [TestMethod]
        public void ExtractToTempTest_AfterExtract()
        {
            var fi = FileSystem.CreateFileInfo(ArchivePath);
            var e = new DPZipExtractor(Log.Logger, new ZipArchiveWrapperFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPathsForTemp(arc, ExtractPath);
            arc.ExtractToTemp(settings);

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings, true);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void PeekTest()
        {
            var e = new DPZipExtractor(Log.Logger, new ZipArchiveWrapperFactory());
            var fi = FileSystem.CreateFileInfo(ArchivePath);
            var arc = new DPArchive(fi);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);

            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, arc.Contents.Values.Select(x => x.Path));
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
    }
}