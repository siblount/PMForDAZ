using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Moq;
using Serilog;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.IO;
using DAZ_Installer.CoreTests.Extraction;
using DAZ_Installer.Core.Extraction.Fakes;

#pragma warning disable CS0618 // Obsolete is for production code, not testing code.
namespace DAZ_Installer.Core.Extraction.Tests
{
    [TestClass]
    public class DPZipExtractorTests
    {
        /// <summary>
        /// A factory that returns a mocked fake archive with the default contents.
        /// </summary>
        static IZipArchiveFactory DefaultFactory { get; set; } = null!;
        static string[] DefaultContents => DPArchiveTestHelpers.DefaultContents;
        static MockOptions DefaultOptions = new();
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .CreateLogger();
            var mock = new Mock<IZipArchiveFactory>();
            mock.Setup(m => m.Create(It.IsAny<Stream>())).Returns(() => SetupFakeArchiveAndEntries(DefaultContents).Object);
            DefaultFactory = mock.Object;
        }

        private struct MockOptions
        {
            public bool partialFileInfo = true;
            public bool partialDPFileInfo = true;
            public bool partialZipArchiveEntry = true;
            public bool partialFileSystem = true;
            public string[] paths = DefaultContents;
            public bool peek = true;

            public MockOptions() { }
        }

        private static DPArchive NewMockedArchive(MockOptions options, out DPZipExtractor extractor, out Mock<FakeZipArchive> fakeArc, out Mock<FakeDPFileInfo> fakeDPFileInfo, out Mock<FakeFileInfo> fakeFileInfo, out Mock<IZipArchiveFactory> factory, out Mock<FakeFileSystem> fakeFileSystem)
        {
            var fs = new Mock<FakeFileSystem>(DPFileScopeSettings.All) { CallBase = options.partialFileSystem };
            fakeFileSystem = fs;
            fakeArc = SetupFakeArchiveAndEntries(options.paths, options.partialZipArchiveEntry);
            fakeFileInfo = new Mock<FakeFileInfo>("Z:/test.zip") { CallBase = options.partialFileInfo };
            fakeDPFileInfo = new Mock<FakeDPFileInfo>(fakeFileInfo.Object, fs.Object, null) { CallBase = options.partialFileInfo };
            factory = new Mock<IZipArchiveFactory>();
            factory.Setup(m => m.Create(It.IsAny<Stream>())).Returns(fakeArc.Object);
            extractor = new DPZipExtractor(Log.Logger.ForContext<DP7zExtractor>(), factory.Object);
            var arc = new DPArchive(string.Empty, Log.Logger.ForContext<DPArchive>(), fakeDPFileInfo.Object, extractor);
            if (options.peek) extractor.Peek(arc);
            return arc;
        }

        /// <summary>
        /// Returns a new <see cref="FakeZipArchive"/> and sets up mocked <see cref="FakeZipArchiveEntry"/>s for the specified paths.
        /// </summary>
        /// <param name="paths">The entries to add to the fake archive.</param>
        /// <param name="partial">Whether to partially mock the entries, default is true.</param>
        /// <returns>A new <see cref="FakeZipArchive"/> with partially mocked <see cref="FakeZipArchiveEntry"/>s.</returns>
        internal static Mock<FakeZipArchive> SetupFakeArchiveAndEntries(IEnumerable<string> paths, bool partial = true)
        {
            var arc = new Mock<FakeZipArchive>() { CallBase = true };
            foreach (var path in paths)
            {
                var entryMock = new Mock<FakeZipArchiveEntry>(arc.Object, Stream.Null) { CallBase = partial };
                if (!partial) entryMock.SetupGet(x => x.FullName).Returns(path);
                else entryMock.Object.FullName = path;
                if (!partial) entryMock.SetupGet(x => x.Name).Returns(Path.GetFileName(path));
                arc.Object.PathToEntries.Add(entryMock.Object.FullName, entryMock.Object);
            }
            return arc;
        }

        [TestMethod]
        public void DPZipExtractorTest()
        {
            var l = Mock.Of<ILogger>();
            var e = new DPZipExtractor(l, DefaultFactory);
            Assert.AreEqual(l, e.Logger);
            Assert.AreEqual(DefaultFactory, e.Factory);
        }

        [TestMethod]
        public void ExtractTest()
        {
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

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
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out var arcDPFileInfo, out _, out _, out _);
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
            var arc = NewMockedArchive(new MockOptions() { paths = Array.Empty<string>() }, out var e, out var _, out _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", Array.Empty<DPFile>(), archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_ArcFileInfoOpenFail()
        {
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out var arcDPFileInfo, out _, out _, out _);
            arcDPFileInfo.Setup(x => x.OpenRead()).Throws(new IOException());

            var settings = new DPExtractSettings("Z:/temp", new DPFile[] {new(), new(), new()}, archive: arc);
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
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out _, out _, out _, out _);
            arc.Contents.Values.First().AssociatedArchive = new DPArchive();

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { 
                ExtractedFiles = arc.Contents.Values.Skip(1).ToList(), 
                ErroredFiles = new() { { arc.Contents.Values.First(), "" } },
                Settings = settings 
            };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_FilesNotWhitelisted()
        {
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out _, out _, out _, out var fs);
            e.Peek(arc);
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
        public void ExtractTest_RetrySuccessUnauthorizedFileException()
        {
            var arc = NewMockedArchive(new() { partialZipArchiveEntry = false }, out var e, out var zipArc, out _, out _, out _, out var _);

            var firstEntityName = zipArc.Object.PathToEntries.Values.Where(x => !string.IsNullOrEmpty(x.Name)).First().FullName;
            var subEntity = new Mock<FakeZipArchiveEntry>(zipArc.Object, Stream.Null) { CallBase = true };
            var calledOnce = false;
            subEntity.Object.FullName = firstEntityName;
            subEntity.Setup(x => x.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>())).Callback(() =>
            {
                if (calledOnce) return;
                calledOnce = true;
                Log.Logger.Information("Throwing UnauthorizedAccessException");
                throw new UnauthorizedAccessException();
            });
            zipArc.Object.PathToEntries[firstEntityName] = subEntity.Object;

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport()
            {
                ExtractedFiles = arc.Contents.Values.ToList(),
                ErroredFiles = new(0),
                Settings = settings
            };
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
        public void ExtractTest_RetryFailUnauthorizedFileException()
        {
            var arc = NewMockedArchive(DefaultOptions, out var e, out var zipArc, out _, out _, out _, out _);

            var firstEntityName = zipArc.Object.PathToEntries.Values.Where(x => !string.IsNullOrEmpty(x.Name)).First().FullName;
            var subEntity = new Mock<IZipArchiveEntry>();
            subEntity.Setup(x => x.ExtractToFile(It.IsAny<string>(), It.IsAny<bool>())).Throws(new UnauthorizedAccessException());
            zipArc.Object.PathToEntries[firstEntityName] = subEntity.Object;

            var file = arc.Contents[PathHelper.NormalizePath(firstEntityName)];

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport()
            {
                ExtractedFiles = arc.Contents.Values.Where(x => x != file).ToList(),
                ErroredFiles = new() { { file, ""} },
                Settings = settings
            };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, expectedReport.ExtractedFiles.Select(x => x.Path));
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(expectedReport.ExtractedFiles);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_UnexpectedExtractError()
        {
            var arc = NewMockedArchive(DefaultOptions, out var e, out var zipArc, out _, out _, out _, out _);

            var firstEntityName = zipArc.Object.PathToEntries.Values.Where(x => !string.IsNullOrEmpty(x.Name)).First().FullName;
            var subEntity = new Mock<IZipArchiveEntry>();
            zipArc.Object.PathToEntries[firstEntityName] = null;

            var file = arc.Contents[PathHelper.NormalizePath(firstEntityName)];

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport()
            {
                ExtractedFiles = arc.Contents.Values.Where(x => x != file).ToList(),
                ErroredFiles = new() { { file, "" } },
                Settings = settings
            };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, expectedReport.ExtractedFiles.Select(x => x.Path));
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(expectedReport.ExtractedFiles);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_AfterExtract()
        {
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

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
            var arc = NewMockedArchive(DefaultOptions, out var e, out var fakeArc, out _, out _, out _, out _);

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
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

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
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPathsForTemp(arc, settings.TempPath); // This should not matter, but will reuse this for testing purposes for AssertExtractorSetPathsCorrectly

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
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out _, out _, out _, out _);
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPathsForTemp(arc, settings.TempPath); // ExtractToTemp does not require TargetPaths, it will do 
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
            var e = new DPZipExtractor(Log.Logger, DefaultFactory);
            var dpFileInfo = new FakeDPFileInfo(new FakeFileInfo("Z:/test.zip", null), new(), null);
            var arc = new DPArchive(string.Empty, Log.Logger, dpFileInfo, e);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void PeekTest_EmitsWithErrors()
        {
            var erroringFactory = new Mock<IZipArchiveFactory>();
            erroringFactory.Setup(erroringFactory => erroringFactory.Create(It.IsAny<Stream>())).Throws(new Exception("Test exception"));
            var e = new DPZipExtractor(Log.Logger, erroringFactory.Object);
            var dpFileInfo = new FakeDPFileInfo(new FakeFileInfo("Z:/test.zip", null), new(), null);
            var arc = new DPArchive(string.Empty, Log.Logger, dpFileInfo, e);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
        }
        [TestMethod]
        public void PeekTest_SeekingErrorStopsSeeking()
        {
            var erroringFactory = new Mock<IZipArchiveFactory>();
            erroringFactory.Setup(erroringFactory => erroringFactory.Create(It.IsAny<Stream>())).Throws(new Exception("Test exception"));
            var e = new DPZipExtractor(Log.Logger, erroringFactory.Object);
            var dpFileInfo = new FakeDPFileInfo(new FakeFileInfo("Z:/test.zip", null), new(), null);
            var arc = new DPArchive(string.Empty, Log.Logger, dpFileInfo, e);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
        }

        [TestMethod]
        public void ExtractTest_CancelledBeforeOp()
        {
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");
            e.CancellationToken = new(true);

            // Testing Extract() here:
            var report = e.Extract(settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_CancelledDuringOp()
        {
            var arc = NewMockedArchive(DefaultOptions, out var e, out var _, out _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(1) { arc.Contents.First().Value }, ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");
            e.ExtractProgress += (_, __) => e.CancellationToken = new(true);

            // Testing Extract() here:
            var report = e.Extract(settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void PeekTest_CancelledBeforeOp()
        {
            var arc = NewMockedArchive(DefaultOptions with { peek = false }, out var e, out var a, out _, out _, out _, out _);
            e.CancellationToken = new(true);

            // Testing Peek() here:
            // Testing Peek() here:
            e.Peek(arc);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
            Assert.AreEqual(0, arc.Contents.Count);
        }

        [TestMethod]
        public void PeekTest_CancelledDuringOp()
        {
            var arc = NewMockedArchive(DefaultOptions with { peek = false }, out var e, out var a, out _, out _, out _, out _);
            a.Setup(x => x.Entries).Callback(() => e.CancellationToken = new(true));

            // Testing Peek() here:
            e.Peek(arc);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
            Assert.AreEqual(0, arc.Contents.Count);
        }
    }
}