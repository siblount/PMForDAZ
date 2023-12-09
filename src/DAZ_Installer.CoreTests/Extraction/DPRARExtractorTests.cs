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
#pragma warning disable 618
namespace DAZ_Installer.Core.Extraction.Tests
{
    [TestClass]
    public class DPRARExtractorTests
    {
        /// <summary>
        /// A factory that returns a mocked fake archive with the default contents.
        /// </summary>
        static IRARFactory DefaultFactory { get; set; } = null!;
        static string[] DefaultContents => DPArchiveTestHelpers.DefaultContents;
        static MockOptions DefaultOptions = new();
        static MockOptions DefaultPeekOptions = new();
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();

            var mock = new Mock<IRARFactory>();
            mock.Setup(x => x.Create(It.IsAny<string>())).Returns(SetupFakeRAR(DefaultContents).Object);
            DefaultFactory = mock.Object;
            DefaultPeekOptions.peek = false;
        }

        private struct MockOptions
        {
            public bool partialFileInfo = true;
            public bool partialRAR = true;
            public bool partialDPFileInfo = true;
            public bool partialZipArchiveEntry = true;
            public bool partialFileSystem = true;
            public string[] paths = DefaultContents;
            public bool peek = true;

            public MockOptions() { }
        }

        private static DPArchive SetupArchiveWithPartiallyFakedDependencies(MockOptions options, out DPRARExtractor extractor, out Mock<FakeRAR> fakeRAR, out Mock<FakeDPFileInfo> fakeDPFileInfo, out Mock<FakeFileInfo> fakeFileInfo, out Mock<IRARFactory> factory, out Mock<FakeFileSystem> fakeFileSystem)
        {
            var fs = new Mock<FakeFileSystem>(DPFileScopeSettings.All) { CallBase = options.partialFileSystem };
            fakeFileSystem = fs;
            factory = new Mock<IRARFactory>();
            var aFakeRAR = SetupFakeRAR(options.paths, options.partialRAR);
            fakeRAR = aFakeRAR;
            fakeFileInfo = new Mock<FakeFileInfo>("Z:/test.rar") { CallBase = options.partialFileInfo };
            fakeDPFileInfo = new Mock<FakeDPFileInfo>(fakeFileInfo.Object, fakeFileSystem.Object, null) { CallBase = options.partialDPFileInfo };
            factory.Setup(x => x.Create(It.IsAny<string>())).Returns(() =>
            {
                aFakeRAR.Object.Disposed = false;
                return aFakeRAR.Object;
            });
            extractor = new DPRARExtractor(Log.Logger.ForContext<DPRARExtractor>(), factory.Object);
            var arc = new DPArchive(string.Empty, Log.Logger.ForContext<DPArchive>(), fakeDPFileInfo.Object, extractor);
            if (!options.peek) return arc;
            extractor.Peek(arc);
            return arc;
        }

        /// <summary>
        /// Returns a new <see cref="fakeRAR"/> and sets up the <see cref="fakeRAR.OutputEnumerable"/> to contain the given paths.
        /// </summary>
        /// <param name="paths">The entries to add to the fake archive.</param>
        internal static Mock<FakeRAR> SetupFakeRAR(IEnumerable<string> paths, bool partial = true)
        {
            var l = new List<RARFileInfo>();
            foreach (var p in paths)
                l.Add(FakeRAR.CreateFileInfoForEntity(p));
            return partial ? new Mock<FakeRAR>(l) { CallBase = true } : new Mock<FakeRAR>(l);
        }

        public static void SetupTargetPathsForTemp(DPArchive arc, string basePath)
        {
            foreach (var file in arc.Contents.Values)
                file.TargetPath = Path.Combine(basePath, Path.GetFileNameWithoutExtension(arc.FileName), file.Path);
        }

        [TestMethod]
        public void DPRARExtractorTest()
        {
            var l = Mock.Of<ILogger>();
            var e = new DPRARExtractor(l, DefaultFactory);
            Assert.AreEqual(l, e.Logger);
            Assert.AreEqual(DefaultFactory, e.Factory);
        }

        [TestMethod]
        public void ExtractTest()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_QuitsOnArcNotExists()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out var arcDPFileInfo, out _, out _, out _);
            arcDPFileInfo.SetupGet(x => x.Exists).Returns(false);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_NoContents()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(new MockOptions() { paths = Array.Empty<string>() }, out var e, out var proc, out _, out _, out _, out _);
            var settings = new DPExtractSettings("Z:/temp", Array.Empty<DPFile>(), archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_RAROpenFail()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var fakeRAR, out _, out _, out var factory, out _);
            fakeRAR.Setup(x => x.Open(It.IsAny<RAR.OpenMode>())).Throws(new Exception("no no no... we no do that here"));

            var settings = new DPExtractSettings("Z:/temp", new DPFile[] { new(), new(), new() }, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_FileNotPartOfArchive()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _, out _);
            arc.Contents.Values.First().AssociatedArchive = null;

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var successFiles = arc.Contents.Values.Skip(1).ToList();
            var expectedReport = new DPExtractionReport()
            {
                ExtractedFiles = successFiles,
                ErroredFiles = new() { { arc.Contents.Values.First(), "" } },
                Settings = settings
            };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, successFiles.Select(x => x.Path));
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(successFiles);
        }
        [TestMethod]
        public void ExtractTest_FilesNotWhitelisted()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _, out var fs);
            fs.Object.Scope = DPFileScopeSettings.None;

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport()
            {
                ExtractedFiles = new(0),
                ErroredFiles = arc.Contents.Values.ToDictionary(x => x, x => ""),
                Settings = settings
            };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_UnexpectedExtractErrorEverythingFine()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var proc, out _, out _, out _, out _);
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_AfterExtract()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");
            e.Extract(settings);
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abcd/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_AfterExtractError()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var fakeArc, out _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");
            settings.FilesToExtract.Add(null);
            e.Extract(settings);
            settings.FilesToExtract.Remove(null);

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_AfterExtractTemp()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");
            e.ExtractToTemp(settings);
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abcd/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }


        [TestMethod]
        public void ExtractToTempTest()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            SetupTargetPathsForTemp(arc, settings.TempPath); // This should not matter, but will reuse this for testing purposes for AssertExtractorSetPathsCorrectly

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings, true);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);

        }
        [TestMethod]
        public void ExtractToTempTest_AfterExtract()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _, out _);
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            SetupTargetPathsForTemp(arc, settings.TempPath); // ExtractToTemp does not require TargetPaths
            arc.ExtractContents(settings);

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings, true);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void PeekTest()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var _, out _, out _, out _, out _);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void PeekTest_EmitsWithErrors()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var fakeRAR, out _, out _, out _, out _);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
            e.ArchiveErrored += (s, e) => Assert.AreEqual("Can not open encrypted archive. Wrong password? You silly goose.", e.Explaination);
        }
        [TestMethod]
        public void PeekTest_StartProcessFails()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var fakeRAR, out _, out _, out _, out _);
            fakeRAR.Setup(x => x.ReadHeader()).Throws(new Exception("Something went wrong"));

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
            e.ArchiveErrored += (s, e) => Assert.AreEqual("Failed to start 7z process", e.Explaination);
        }

        [TestMethod]
        public void PeekTest_Encrypted()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var fakeRAR, out _, out _, out _, out _);
            var l = new List<string>();
            fakeRAR.Object.FilesEnumerable.MoveNext();
            fakeRAR.Object.FilesEnumerable.Current.encrypted = true;
            fakeRAR.Object.FilesEnumerable.Reset();

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
            e.ArchiveErrored += (s, e) => Assert.AreEqual(DPArchiveErrorArgs.EncryptedFilesExplanation, e.Explaination);
        }

        [TestMethod]
        public void ExtractTest_CancelledBeforeOp()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

            CancellationTokenSource cts = new();
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc) { CancelToken = cts.Token };
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");
            cts.Cancel(true);

            // Testing Extract() here:
            var report = e.Extract(settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);


            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_CancelledDuringOp()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

            CancellationTokenSource cts = new();
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc) { CancelToken = cts.Token };
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(1) { arc.Contents.First().Value }, ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");
            e.ExtractProgress += (s, e) => cts.Cancel(true);

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);


            Assert.AreEqual(arc.FileSystem, e.FileSystem);

        }

        [TestMethod]
        public void PeekTest_CancelledBeforeOp()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var _, out _, out _, out _, out _);
            e.CancellationToken = new(true);
            // Testing Peek() here:
            e.Peek(arc);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
            Assert.AreEqual(0, arc.Contents.Count);
        }

        [TestMethod]
        public void PeekTest_CancelledDuringOp()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var r, out _, out _, out _, out _);
            r.Setup(x => x.ReadHeader()).Callback(() => e.CancellationToken = new(true));

            // Testing Peek() here:
            e.Peek(arc);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
            Assert.AreEqual(0, arc.Contents.Count);
        }

    }
}