using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using NSubstitute;
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

            DefaultFactory = Substitute.For<IZipArchiveFactory>();
            DefaultFactory.Create(Arg.Any<Stream>()).Returns(_ => SetupFakeArchiveAndEntries(DefaultContents));
        }

        private struct MockOptions
        {
            public bool partialFileInfo = true;
            public bool partialDPFileInfo = true;
            public bool partialZipArchiveEntry = true;
            public string[] paths = DefaultContents;

            public MockOptions() { }
        }

        private static DPArchive SetupArchiveWithPartiallyFakedDependencies(MockOptions options, out DPZipExtractor extractor, out FakeZipArchive fakeArc, out FakeDPFileInfo fakeDPFileInfo, out FakeFileInfo fakeFileInfo, out IZipArchiveFactory factory)
        {
            factory = Substitute.For<IZipArchiveFactory>();
            fakeArc = SetupFakeArchiveAndEntries(options.paths, options.partialZipArchiveEntry);
            fakeFileInfo = options.partialFileInfo ? Substitute.ForPartsOf<FakeFileInfo>("Z:/test.zip", null) : Substitute.For<FakeFileInfo>("Z:/test.zip", null);
            fakeDPFileInfo = options.partialDPFileInfo ? Substitute.ForPartsOf<FakeDPFileInfo>(fakeFileInfo, new FakeDPIOContext(), null) : Substitute.For<FakeDPFileInfo>(fakeFileInfo, new FakeDPIOContext(), null);
            factory.Create(default).ReturnsForAnyArgs(fakeArc);
            extractor = new DPZipExtractor(Log.Logger.ForContext<DP7zExtractor>(), factory);
            var arc = new DPArchive(string.Empty, Log.Logger.ForContext<DPArchive>(), fakeDPFileInfo, extractor);
            extractor.Peek(arc);
            return arc;
        }

        /// <summary>
        /// Returns a new <see cref="FakeZipArchive"/> and sets up mocked <see cref="FakeZipArchiveEntry"/>s for the specified paths.
        /// </summary>
        /// <param name="paths">The entries to add to the fake archive.</param>
        /// <param name="partial">Whether to partially mock the entries, default is true.</param>
        /// <returns>A new <see cref="FakeZipArchive"/> with partially mocked <see cref="FakeZipArchiveEntry"/>s.</returns>
        internal static FakeZipArchive SetupFakeArchiveAndEntries(IEnumerable<string> paths, bool partial = true)
        {
            var arc = Substitute.ForPartsOf<FakeZipArchive>();
            foreach (var path in paths)
            {
                var entry = partial ? Substitute.ForPartsOf<FakeZipArchiveEntry>(arc, Stream.Null) : Substitute.For<FakeZipArchiveEntry>(arc, Stream.Null);
                entry.FullName = path;
                if (!partial) entry.Name.Returns(Path.GetFileName(path));
                arc.PathToEntries.Add(entry.FullName, entry);
            }
            return arc;
        }

        [TestMethod]
        public void DPZipExtractorTest()
        {
            var l = Substitute.For<ILogger>();
            var e = new DPZipExtractor(l, DefaultFactory);
            Assert.AreEqual(l, e.Logger);
            Assert.AreEqual(DefaultFactory, e.Factory);
        }

        [TestMethod]
        public void ExtractTest()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            
            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.Context, e.Context);
        }

        [TestMethod]
        public void ExtractTest_QuitsOnArcNotExists()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);
            arc.FileInfo!.Exists.Returns(false);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.Context, e.Context);
        }

        [TestMethod]
        public void ExtractTest_NoContents()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(new MockOptions() { paths = Array.Empty<string>() }, out var e, out var _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", Array.Empty<DPFile>(), archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.Context, e.Context);
        }

        [TestMethod]
        public void ExtractTest_ArcFileInfoOpenFail()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);
            arc.FileInfo!.OpenRead().Returns(_ => throw new IOException());

            var settings = new DPExtractSettings("Z:/temp", new DPFile[] {new(), new(), new()}, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.Context, e.Context);
        }

        [TestMethod]
        public void ExtractTest_FileNotPartOfArchive()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);
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
            Assert.AreEqual(arc.Context, e.Context);
        }
        [TestMethod]
        public void ExtractTest_FilesNotWhitelisted()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);
            e.Peek(arc);
            arc.FileInfo!.Context.ChangeScopeTo(DPFileScopeSettings.None);

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
            Assert.AreEqual(arc.Context, e.Context);
        }
        [TestMethod]
        public void ExtractTest_RetrySuccessUnauthorizedFileException()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(new() { partialZipArchiveEntry = false }, out var e, out var zipArc, out _, out _, out _);

            var firstEntityName = zipArc.PathToEntries.Values.Where(x => !string.IsNullOrEmpty(x.Name)).First().FullName;
            var subEntity = Substitute.For<FakeZipArchiveEntry>(zipArc, Stream.Null);
            var calledOnce = false;
            subEntity.FullName = firstEntityName;
            subEntity.When(x => x.ExtractToFile(Arg.Any<string>(), Arg.Any<bool>())).Do(x =>
            {
                if (calledOnce) return;
                calledOnce = true;
                Log.Logger.Information("Throwing UnauthorizedAccessException");
                throw new UnauthorizedAccessException();
            });
            zipArc.PathToEntries[firstEntityName] = subEntity;

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
            Assert.AreEqual(arc.Context, e.Context);
        }
        [TestMethod]
        public void ExtractTest_RetryFailUnauthorizedFileException()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var zipArc, out _, out _, out _);

            var firstEntityName = zipArc.PathToEntries.Values.Where(x => !string.IsNullOrEmpty(x.Name)).First().FullName;
            var subEntity = Substitute.For<IZipArchiveEntry>();
            subEntity.When(x => x.ExtractToFile(Arg.Any<string>(), Arg.Any<bool>())).Throw(new UnauthorizedAccessException("u shall not pass"));
            zipArc.PathToEntries[firstEntityName] = subEntity;

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
            Assert.AreEqual(arc.Context, e.Context);
        }
        [TestMethod]
        public void ExtractTest_UnexpectedExtractError()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var zipArc, out _, out _, out _);

            var firstEntityName = zipArc.PathToEntries.Values.Where(x => !string.IsNullOrEmpty(x.Name)).First().FullName;
            var subEntity = Substitute.For<IZipArchiveEntry>();
            zipArc.PathToEntries[firstEntityName] = null;

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
            Assert.AreEqual(arc.Context, e.Context);
        }
        [TestMethod]
        public void ExtractTest_AfterExtract()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);

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
            Assert.AreEqual(arc.Context, e.Context);
        }
        [TestMethod]
        public void ExtractTest_AfterExtractError()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var fakeArc, out _, out _, out _);

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
            Assert.AreEqual(arc.Context, e.Context);
        }
        [TestMethod]
        public void ExtractTest_AfterExtractTemp()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);

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
            Assert.AreEqual(arc.Context, e.Context);
        }


        [TestMethod]
        public void ExtractToTempTest()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPathsForTemp(arc, settings.TempPath); // This should not matter, but will reuse this for testing purposes for AssertExtractorSetPathsCorrectly

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings, true);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.Context, e.Context);

        }
        [TestMethod]
        public void ExtractToTempTest_AfterExtract()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);
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
            Assert.AreEqual(arc.Context, e.Context);
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
            Assert.AreEqual(arc.Context, e.Context);
        }

        [TestMethod]
        public void PeekTest_EmitsWithErrors()
        {
            var erroringFactory = Substitute.For<IZipArchiveFactory>();
            erroringFactory.Create(default).ReturnsForAnyArgs(x => throw new Exception("Test exception"));
            var e = new DPZipExtractor(Log.Logger, erroringFactory);
            var dpFileInfo = new FakeDPFileInfo(new FakeFileInfo("Z:/test.zip", null), new(), null);
            var arc = new DPArchive(string.Empty, Log.Logger, dpFileInfo, e);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
        }
        [TestMethod]
        public void PeekTest_SeekingErrorStopsSeeking()
        {
            var erroringFactory = Substitute.For<IZipArchiveFactory>();
            erroringFactory.Create(default).ReturnsForAnyArgs(x => throw new Exception("Test exception"));
            var e = new DPZipExtractor(Log.Logger, erroringFactory);
            var dpFileInfo = new FakeDPFileInfo(new FakeFileInfo("Z:/test.zip", null), new(), null);
            var arc = new DPArchive(string.Empty, Log.Logger, dpFileInfo, e);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
        }

    }
}