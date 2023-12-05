using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core.Extraction.Fakes;
using DAZ_Installer.CoreTests.Extraction;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.IO;
using Moq;
using Serilog;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using DAZ_Installer.External;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Castle.DynamicProxy.Generators;
#pragma warning disable 618
namespace DAZ_Installer.Core.Extraction.Integration.Tests
{
    [TestClass]
    public class DPRARExtractorTests
    {
        public static readonly string ExtractPath = Path.Combine(Path.GetTempPath(), "DAZ_InstallerTests", "Extract");
        public static readonly string TestSubjectsPath = Path.Combine(Environment.CurrentDirectory, "Integration", "Test Subjects");
        public static readonly DPFileScopeSettings DefaultScope = new(Enumerable.Empty<string>(), new[] { ExtractPath }, false);
        public static readonly DPFileSystem FileSystem = new DPFileSystem(DefaultScope);

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();
            MSTestLogger.LogMessage("Class initializing...");
            if (!Directory.Exists(TestSubjectsPath)) throw new DirectoryNotFoundException(TestSubjectsPath);
        }

        [TestMethod]
        public void DPRARExtractorTest()
        {
            var l = Mock.Of<ILogger>();
            var f = new RARFactory();
            var e = new DPRARExtractor(l, f);
            Assert.AreEqual(l, e.Logger);
            Assert.AreEqual(f, e.Factory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            MSTestLogger.LogMessage("Cleaning up...");
            Directory.Delete(ExtractPath, true);
        }

        [TestInitialize]
        public void Initialize()
        {
            MSTestLogger.LogMessage("Initializing...");
            Directory.CreateDirectory(ExtractPath);
            Console.WriteLine(Environment.CurrentDirectory);
            Console.WriteLine(Directory.Exists(TestSubjectsPath));
        }

        // This is different than the one from DPArchiveTestHelpers.
        public static void SetupTargetPathsForTemp(DPArchive arc, string basePath)
        {
            foreach (var file in arc.Contents.Values)
                file.TargetPath = Path.Combine(basePath, Path.GetFileNameWithoutExtension(arc.FileName), file.Path);
        }

        [TestMethod]
        [DataRow("Test.rar"), DataRow("Test_split.part1.rar"), DataRow("Test_split_solid.part1.rar")]
        public void ExtractTest(string path)
        {
            List<byte> fileData = new(65536);
            var factory = new Mock<IRARFactory>();
            using var r2 = new RAR(Path.Combine(TestSubjectsPath, path));
            RAR.DataAvailableHandler fileDataFunc = (s, e) =>
            {
                if (!s.CurrentFile.FileName.Contains("random_image") && !path.Contains("Test_split")) return;
                fileData.AddRange(e.Data);
            };
            r2.DataAvailable += fileDataFunc;
            factory.SetupSequence(x => x.Create(It.IsAny<string>())).Returns(new RAR(Path.Combine(TestSubjectsPath, path))).Returns(r2);
            var e = new DPRARExtractor(Log.Logger, factory.Object);
            var fi = FileSystem.CreateFileInfo(Path.Combine(TestSubjectsPath, path));
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);

            //// Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_AfterExtract()
        {
            var e = new DPRARExtractor();
            var fi = FileSystem.CreateFileInfo(Path.Combine(TestSubjectsPath, "Test.rar"));
            var arc = new DPArchive(fi);
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);
            e.Extract(settings);

            //// Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_AfterExtractTemp()
        {
            var e = new DPRARExtractor();
            var fi = FileSystem.CreateFileInfo(Path.Combine(TestSubjectsPath, "Test.rar"));
            var arc = new DPArchive(fi);
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);
            e.ExtractToTemp(settings);

            //// Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, arc.Contents.Values.Select(x => x.Path));
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }


        [TestMethod]
        public void ExtractToTempTest()
        {
            var e = new DPRARExtractor();
            var fi = FileSystem.CreateFileInfo(Path.Combine(TestSubjectsPath, "Test.rar"));
            var arc = new DPArchive(fi);
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            SetupTargetPathsForTemp(arc, ExtractPath);

            //// Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings, true);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, arc.Contents.Values.Select(x => x.Path));
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);

        }
        [TestMethod]
        public void ExtractToTempTest_AfterExtract()
        {
            var e = new DPRARExtractor();
            var fi = FileSystem.CreateFileInfo(Path.Combine(TestSubjectsPath, "Test.rar"));
            var arc = new DPArchive(fi);
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            SetupTargetPathsForTemp(arc, ExtractPath);
            e.ExtractToTemp(settings);

            //// Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings, true);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, arc.Contents.Values.Select(x => x.Path));
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        [DataRow("Test.rar"), DataRow("Test_split.part1.rar"), DataRow("Test_split_solid.part1.rar")]

        public void PeekTest(string path)
        {
            var e = new DPRARExtractor();
            var fi = FileSystem.CreateFileInfo(Path.Combine(TestSubjectsPath, path));
            var arc = new DPArchive(fi);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);

            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, arc.Contents.Values.Select(x => x.Path));
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

    }
}