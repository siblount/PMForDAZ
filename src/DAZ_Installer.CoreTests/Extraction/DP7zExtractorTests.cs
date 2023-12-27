using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core.Extraction.Fakes;
using DAZ_Installer.CoreTests.Extraction;
using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using Moq;
using Serilog;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Microsoft.VisualStudio.TestPlatform.Utilities;

#pragma warning disable 618
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

            var factMockObj = new Mock<IProcessFactory>();
            factMockObj.Setup(x => x.Create()).Returns(SetupFakeProcess(DefaultContents).Object);
            DefaultFactory = factMockObj.Object;

            DefaultPeekOptions.peek = false;
        }

        private struct MockOptions
        {
            public bool partialFileInfo = true;
            public bool partialFakeProcess = true;
            public bool partialDPFileInfo = true;
            public bool partialZipArchiveEntry = true;
            public bool partialFileSystem = true;
            public string[] paths = DefaultContents;
            public bool peek = true;

            public MockOptions() { }
        }

        private readonly record struct MockOutputs (DP7zExtractor extractor, Mock<FakeProcess> fakeProcess, Mock<FakeDPFileInfo> fakeDPFileInfo, Mock<FakeFileInfo> fakeFileInfo, Mock<FakeFileSystem> fakeFileSystem, Mock<IProcessFactory> factory);

        private static DPArchive NewMockedArchive(MockOptions options, out MockOutputs mockOutputs)
        {
            var fakeProcess = SetupFakeProcess(options.paths);
            var fakeFileInfo = new Mock<FakeFileInfo>("Z:/test.7z") { CallBase = options.partialFileInfo };
            var fakeFileSystem = new Mock<FakeFileSystem>(DPFileScopeSettings.All) { CallBase = options.partialFileSystem };
            var fakeDPFileInfo = new Mock<FakeDPFileInfo>(fakeFileInfo.Object, fakeFileSystem.Object, null) { CallBase = options.partialDPFileInfo };
            var factory = new Mock<IProcessFactory>();
            factory.Setup(x => x.Create()).Returns(fakeProcess.Object);
            var extractor = new DP7zExtractor(Log.Logger.ForContext<DP7zExtractor>(), factory.Object);
            var arc = new DPArchive(string.Empty, Log.Logger.ForContext<DPArchive>(), fakeDPFileInfo.Object, extractor);
            mockOutputs = new MockOutputs(extractor, fakeProcess, fakeDPFileInfo, fakeFileInfo, fakeFileSystem, factory);
            if (!options.peek) return arc;
            extractor.Peek(arc);
            fakeProcess.Object.OutputEnumerable = new[] { "Everything is Ok" }; 
            return arc;
        }

        /// <summary>
        /// Returns a new <see cref="FakeProcess"/> and sets up the <see cref="FakeProcess.OutputEnumerable"/> to contain the given paths.
        /// </summary>
        /// <param name="paths">The entries to add to the fake archive.</param>
        internal static Mock<FakeProcess> SetupFakeProcess(IEnumerable<string> paths, bool partial = true)
        {
            var l = new List<string>();
            foreach (var p in paths) FakeProcess.GetLinesForEntity(p, l);
            var proc = partial ? new Mock<FakeProcess>() { CallBase = true } : new Mock<FakeProcess>();
            proc.Object.OutputEnumerable = l;
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
            var l = Mock.Of<ILogger>();
            var e = new DP7zExtractor(l, DefaultFactory);
            Assert.AreEqual(l, e.Logger);
            Assert.AreEqual(DefaultFactory, e.Factory);
        }

        [TestMethod]
        public void ExtractTest()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            var e = outputs.extractor;

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_PartialExtract()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            var e = outputs.extractor;

            var skippedFile = arc.Contents.Values.ElementAt(1);
            var successFiles = arc.Contents.Values.Except(new[] { skippedFile }).ToList();
            var settings = new DPExtractSettings("Z:/temp", successFiles, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = successFiles, ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(successFiles);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_QuitsOnArcNotExists()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            var e = outputs.extractor;
            var arcDPFileInfo = outputs.fakeDPFileInfo;
            arcDPFileInfo.Setup(x => x.Exists).Returns(false);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            e.Extracting += () => Assert.Fail("Extracting event was raised");
            var report = e.Extract(settings);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_NoContents()
        {
            var arc = NewMockedArchive(new MockOptions() { paths = Array.Empty<string>() }, out var outputs);
            var e = outputs.extractor;
            var proc = outputs.fakeProcess;
            proc.Object.OutputEnumerable = new[] {null, "Everything is Ok"};
            var settings = new DPExtractSettings("Z:/temp", Array.Empty<DPFile>(), archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_ArcFileInfoOpenFail()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var arcDPFileInfo, out var _, out var _, out var _);
            arcDPFileInfo.Setup(x => x.OpenRead()).Throws(new Exception("Something went wrong"));

            var settings = new DPExtractSettings("Z:/temp", new DPFile[] { new(), new(), new() }, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_FileNotPartOfArchive()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var _, out var _, out var _, out var _);
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
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, successFiles.Select(x => x.Path));
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(successFiles);
        }
        [TestMethod]
        public void ExtractTest_FilesNotWhitelisted()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var arcDPFileInfo, out var _, out var _, out var _);
            e.Peek(arc);
            outputs.fakeFileSystem.SetupProperty(x => x.Scope, DPFileScopeSettings.None);

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
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_UnexpectedExtractErrorEverythingFine()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            
            var e = outputs.extractor;
            var proc = outputs.fakeProcess;
            proc.Object.ErrorEnumerable = new[] { "i am a teapot" };
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_AfterExtract()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var _, out var _, out var _, out var _);

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
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_AfterExtractError()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var _, out var _, out var _, out var _);

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
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_AfterExtractTemp()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var _, out var _, out var _, out var _);

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
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }


        [TestMethod]
         public void ExtractToTempTest()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var _, out var _, out var _, out var _);

            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            SetupTargetPathsForTemp(arc, settings.TempPath); // This should not matter, but will reuse this for testing purposes for AssertExtractorSetPathsCorrectly

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings, true);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);

        }
        [TestMethod]
        public void ExtractToTempTest_AfterExtract()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var _, out var _, out var _, out var _);
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
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void PeekTest()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var _, out var _, out var _, out var _);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);

            DPArchiveTestHelpers.AssertDefaultContents(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, DefaultContents);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void PeekTest_EmitsWithErrors()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var fakeProcess, out var _, out var _, out var _, out var _);
            fakeProcess.Object.ErrorEnumerable = new[] { "Can not open encrypted archive. Wrong password? You silly goose." };

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
            e.ArchiveErrored += (s, e) => Assert.AreEqual("Can not open encrypted archive. Wrong password? You silly goose.", e.Explaination);
        }
        [TestMethod]
        public void PeekTest_StartProcessFails()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var fakeProcess, out var _, out var _, out var _, out var _);
            fakeProcess.Setup(x => x.Start()).Throws(new Exception("Something went wrong"));

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
            e.ArchiveErrored += (s, e) => Assert.AreEqual("Failed to start 7z process", e.Explaination);
        }

        [TestMethod]
        public void PeekTest_Encrypted()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            outputs.Deconstruct(out var e, out var fakeProcess, out var _, out var _, out var _, out var _);
            var l = new List<string>();
            FakeProcess.GetLinesForEntity("encrypted_something.jpg", l);
            l.Insert(3, "Encrypted = +");
            fakeProcess.Object.OutputEnumerable = fakeProcess.Object.OutputEnumerable.Concat(l);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);
            e.ArchiveErrored += (s, e) => Assert.AreEqual(DPArchiveErrorArgs.EncryptedFilesExplanation, e.Explaination);
        }
        [TestMethod]
        public void ExtractTest_CancelledBeforeOp()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            var e = outputs.extractor;

            CancellationTokenSource cts = new();
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc) { CancelToken = cts.Token };
            cts.Cancel(true);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = e.Extract(settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_CancelledDuringExtractOp()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            var e = outputs.extractor;

            CancellationTokenSource cts = new();
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc) { CancelToken = cts.Token };
            outputs.fakeProcess.Object.OutputDataReceived += _ => cts.Cancel(true);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_CancelledDuringMoveOp()
        {
            var arc = NewMockedArchive(DefaultOptions, out var outputs);
            var e = outputs.extractor;

            CancellationTokenSource cts = new();
            var settings = new DPExtractSettings("Z:/temp", arc.Contents.Values, archive: arc) { CancelToken = cts.Token };
            e.MoveProgress += (_, __) => cts.Cancel(true);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(1) { arc.Contents.First().Value }, ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/abc/");

            // Testing Extract() here:
            var report = DPArchiveTestHelpers.RunAndAssertExtractEvents(e, settings);
            DPArchiveTestHelpers.AssertReport(expectedReport, report);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void PeekTest_CancelledBeforeOp()
        {
            var arc = NewMockedArchive(DefaultOptions with { peek = false }, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var _, out var _, out var _, out var _);
            e.CancellationToken = new(true);
            // Testing Peek() here:
            e.Peek(arc);

            Assert.AreEqual(0, arc.Contents.Count);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void PeekTest_CancelledDuringOp()
        {
            var arc = NewMockedArchive(DefaultOptions with { peek = false }, out var outputs);
            outputs.Deconstruct(out var e, out var _, out var _, out var _, out var _, out var _);
            CancellationTokenSource cts = new();
            e.CancellationToken = cts.Token;
            outputs.fakeProcess.Object.OutputDataReceived += _ => cts.Cancel(true);

            // Testing Peek() here:
            e.Peek(arc);

            Assert.AreEqual(0, arc.Contents.Count);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

    }
}