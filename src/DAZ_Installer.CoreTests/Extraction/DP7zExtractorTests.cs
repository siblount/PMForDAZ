using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core.Extraction.Fakes;
using DAZ_Installer.CoreTests.Extraction;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.IO;
using NSubstitute;
using Serilog;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;

namespace DAZ_Installer.Core.Extraction.Tests
{
    [TestClass]
    public class DP7zExtractorTests
    {
        /// <summary>
        /// A factory that returns a mocked fake archive with the default contents.
        /// </summary>
        static IProcessFactory DefaultFactory { get; set; } = null!;
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

            DefaultFactory = Substitute.For<IProcessFactory>();
            DefaultFactory.Create().Returns(_ => SetupFakeProcess(DefaultContents));

            DefaultPeekOptions.peek = false;
        }

        private struct MockOptions
        {
            public bool partialFileInfo = true;
            public bool partialFakeProcess = true;
            public bool partialDPFileInfo = true;
            public bool partialZipArchiveEntry = true;
            public string[] paths = DefaultContents;
            public bool peek = true;

            public MockOptions() { }
        }

        private static DPArchive SetupArchiveWithPartiallyFakedDependencies(MockOptions options, out DP7zExtractor extractor, out FakeProcess fakeProcess, out FakeDPFileInfo fakeDPFileInfo, out FakeFileInfo fakeFileInfo, out IProcessFactory factory)
        {
            factory = Substitute.For<IProcessFactory>();
            fakeProcess = SetupFakeProcess(options.paths);
            fakeFileInfo = options.partialFileInfo ? Substitute.ForPartsOf<FakeFileInfo>("Z:/test.7z", null) : Substitute.For<FakeFileInfo>("Z:/test.7z", null);
            fakeDPFileInfo = options.partialDPFileInfo ? Substitute.ForPartsOf<FakeDPFileInfo>(fakeFileInfo, new FakeDPIOContext(), null) : Substitute.For<FakeDPFileInfo>(fakeFileInfo, new FakeDPIOContext(), null);
            factory.Create().ReturnsForAnyArgs(fakeProcess);
            extractor = new DP7zExtractor(Log.Logger.ForContext<DP7zExtractor>(), factory);
            var arc = new DPArchive(string.Empty, Log.Logger.ForContext<DPArchive>(), fakeDPFileInfo, extractor);
            if (!options.peek) return arc;
            extractor.Peek(arc);
            fakeProcess.OutputEnumerable = new[] { "Everything is Ok" }; 
            return arc;
        }

        /// <summary>
        /// Returns a new <see cref="FakeProcess"/> and sets up the <see cref="FakeProcess.OutputEnumerable"/> to contain the given paths.
        /// </summary>
        /// <param name="paths">The entries to add to the fake archive.</param>
        internal static FakeProcess SetupFakeProcess(IEnumerable<string> paths, bool partial = true)
        {
            var l = new List<string>();
            foreach (var p in paths) FakeProcess.GetLinesForEntity(p, l);
            var proc = partial ? Substitute.ForPartsOf<FakeProcess>() : Substitute.For<FakeProcess>();
            proc.OutputEnumerable = l;
            return proc;
        }

        public static DPExtractionReport RunAndAssertExtractEvents(DPAbstractExtractor extractor, DPExtractSettings settings, bool toTemp = false)
        {
            bool extracting = false, moving = false;
            extractor.Extracting += () =>
            {
                if (extracting)
                    Assert.Fail("Extracting event was raised more than once");
                extracting = true;
            };
            extractor.Moving += () =>
            {
                if (moving && !toTemp)
                    Assert.Fail("Moving event was raised more than once");
                else if (!moving && toTemp) Assert.Fail("Moving event was called when extracting to temp");
                moving = true;
            };
            bool extractFinished = false, moveFinished = false;
            extractor.ExtractFinished += () =>
            {
                if (extractFinished)
                    Assert.Fail("ExtractFinished event was raised more than once");
                extractFinished = true;
            };
            extractor.MoveFinished += () =>
            {
                if (moveFinished && !toTemp)
                    Assert.Fail("MoveFinished event was raised more than once");
                else if (!moveFinished && toTemp) Assert.Fail("MoveFinished event was called when extracting to temp");
                moveFinished = true;
            };
            var report = toTemp ? extractor.ExtractToTemp(settings) : extractor.Extract(settings);
            Assert.IsTrue(extracting, "Extracting event was not raised");
            Assert.IsTrue(extractFinished, "ExtractFinished event was not raised");
            if (!toTemp)
            {
                Assert.IsTrue(moving, "Moving event was not raised"); 
                Assert.IsTrue(moveFinished, "MoveFinished event was not raised");
            }
            return report;
        }

        public static void SetupTargetPathsForTemp(DPArchive arc, string basePath)
        {
            foreach (var file in arc.Contents.Values)
                file.TargetPath = Path.Combine(basePath, Path.GetFileNameWithoutExtension(arc.FileName), file.Path);
        }

        [TestMethod]
        public void DP7zExtractorTest()
        {
            var l = Substitute.For<ILogger>();
            var e = new DP7zExtractor(l, DefaultFactory);
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
            var report = RunAndAssertExtractEvents(e, settings);
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
            var arc = SetupArchiveWithPartiallyFakedDependencies(new MockOptions() { paths = Array.Empty<string>() }, out var e, out var proc, out _, out _, out _);
            proc.OutputEnumerable = new[] {null, "Everything is Ok"};
            var settings = new DPExtractSettings("Z:/temp", Array.Empty<DPFile>(), archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.Context, e.Context);
        }

        [TestMethod]
        public void ExtractTest_ArcFileInfoOpenFail()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);
            arc.FileInfo!.OpenRead().Returns(_ => throw new IOException());

            var settings = new DPExtractSettings("Z:/temp", new DPFile[] { new(), new(), new() }, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.Context, e.Context);
        }

        [TestMethod]
        public void ExtractTest_FileNotPartOfArchive()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var _, out _, out _, out _);
            arc.Contents.Values.First().AssociatedArchive = new DPArchive();

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
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.Context, e.Context);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, successFiles.Select(x => x.Path));
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(successFiles);
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
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.Context, e.Context);
        }
        [TestMethod]
        public void ExtractTest_UnexpectedExtractErrorEverythingFine()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultOptions, out var e, out var proc, out _, out _, out _);
            proc.ErrorEnumerable = new[] { "i am a teapot" };
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
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
            var report = RunAndAssertExtractEvents(e, settings);
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
            var report = RunAndAssertExtractEvents(e, settings);
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
            var report = RunAndAssertExtractEvents(e, settings);
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
            SetupTargetPathsForTemp(arc, settings.TempPath); // This should not matter, but will reuse this for testing purposes for AssertExtractorSetPathsCorrectly

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings, true);
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
            SetupTargetPathsForTemp(arc, settings.TempPath); // ExtractToTemp does not require TargetPaths
            arc.ExtractContents(settings);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings, true);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.Context, e.Context);
        }

        [TestMethod]
        public void PeekTest()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var _, out _, out _, out _);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            Assert.AreEqual(arc.Context, e.Context);
        }

        [TestMethod]
        public void PeekTest_EmitsWithErrors()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var fakeProcess, out _, out _, out _);
            fakeProcess.ErrorEnumerable = new[] { "Can not open encrypted archive. Wrong password? You silly goose." };

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
            e.ArchiveErrored += (s, e) => Assert.AreEqual("Can not open encrypted archive. Wrong password? You silly goose.", e.Explaination);
        }
        [TestMethod]
        public void PeekTest_StartProcessFails()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var fakeProcess, out _, out _, out _);
            fakeProcess.When(x => x.Start()).Throw(new Exception("Something went wrong"));

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
            e.ArchiveErrored += (s, e) => Assert.AreEqual("Failed to start 7z process", e.Explaination);
        }

        [TestMethod]
        public void PeekTest_Encrypted()
        {
            var arc = SetupArchiveWithPartiallyFakedDependencies(DefaultPeekOptions, out var e, out var fakeProcess, out _, out _, out _);
            var l = new List<string>();
            FakeProcess.GetLinesForEntity("encrypted_something.jpg", l);
            l.Insert(3, "Encrypted = +");
            fakeProcess.OutputEnumerable = fakeProcess.OutputEnumerable.Concat(l);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
            e.ArchiveErrored += (s, e) => Assert.AreEqual(DPArchiveErrorArgs.EncryptedFilesExplanation, e.Explaination);
        }

    }
}