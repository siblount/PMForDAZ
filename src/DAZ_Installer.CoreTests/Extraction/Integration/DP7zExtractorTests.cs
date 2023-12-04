using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core.Extraction.Fakes;
using DAZ_Installer.CoreTests.Extraction;
using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using Moq;
using Serilog;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using DAZ_Installer.Core.Integration;
using System.Diagnostics;

#pragma warning disable 618
namespace DAZ_Installer.Core.Extraction.Integration.Tests
{
    [TestClass]
    public class DP7zExtractorTests
    {
        public static readonly string TempPath = Path.Combine(Path.GetTempPath(), "DAZ_Installer.CoreTests", "Extraction", "Integration", "Test Subjects");
        public static readonly string RegularArchivePath = Path.Combine(TempPath, "regular.7z");
        public static readonly string SolidArchivePath = Path.Combine(TempPath, "solid.7z");
        public static readonly string EncryptedArchivePath = Path.Combine(TempPath, "encrypted.7z");
        public static readonly string MultiVolumeArchivePathInit = Path.Combine(TempPath, "multivolume.7z");
        public static readonly string MultiVolumeArchivePath = Path.Combine(TempPath, "multivolume.7z.001");
        public static readonly string MultiVolumeArchivePath2 = Path.Combine(TempPath, "multivolume.7z.002");
        public static readonly string ArchiveContentsPath = Path.Combine(TempPath, "Archive Contents");
        public static List<string> ArchiveContents = new(5);
        // f DRY all my homies copypasta
        public static readonly string ExtractPath = Path.Combine(Path.GetTempPath(), "DAZ_InstallerTests", "Extract");
        public static readonly string TestSubjectsPath = Path.Combine(Environment.CurrentDirectory, "Integration", "Test Subjects");
        public static readonly DPFileScopeSettings DefaultScope = new(Enumerable.Empty<string>(), new[] { ExtractPath }, false);
        public static readonly DPFileSystem FileSystem = new DPFileSystem(DefaultScope);
        public static readonly ProcessStartInfo StartInfoTemplate = new() { FileName = "7za.exe", UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        public static readonly List<string> DefaultOpts = new()
        {
            "-t7z", // Set archive type to 7z
            "-mx0", // Set compression method to simply copy.
            "-ms=off" // Set solid mode to off.
        };
        public static IEnumerable<string[]> ArchiveEnumerable => new[]
        {
            new[] { RegularArchivePath },
            new[] { SolidArchivePath },
            new[] { MultiVolumeArchivePath },
        };
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Verbose()
                        .CreateLogger();

            ArchiveContents = DPIntegrationArchiveHelpers.CreateArchiveContents(ArchiveContentsPath);
            Create7zArchiveOnDisk(RegularArchivePath, ArchiveContentsPath, DefaultOpts);
            Create7zArchiveOnDisk(SolidArchivePath, ArchiveContentsPath, DefaultOpts.SkipLast(1));
            Create7zArchiveOnDisk(MultiVolumeArchivePathInit, ArchiveContentsPath, DefaultOpts.Append("-v6m"));
            if (!File.Exists(MultiVolumeArchivePath2)) throw new Exception("Failed to create multivolume archive");
            Create7zArchiveOnDisk(EncryptedArchivePath, ArchiveContentsPath, DefaultOpts.Append("-pPASSWORD")
                                                                                    .Append("-mhe"));
            Directory.CreateDirectory(ExtractPath);
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

        public static void SetupTargetPathsForTemp(DPArchive arc, string basePath)
        {
            foreach (var file in arc.Contents.Values)
                file.TargetPath = Path.Combine(basePath, Path.GetFileNameWithoutExtension(arc.FileName), file.Path);
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

        public static void Create7zArchiveOnDisk(string savePath, string contentsPath, IEnumerable<string> appendedArgs)
        {
            if (string.IsNullOrWhiteSpace(contentsPath)) throw new ArgumentException("No content path was provided", nameof(contentsPath));
            if (!contentsPath.EndsWith('*')) contentsPath = Path.Join(contentsPath, "*");
            var opts = string.Join(' ', appendedArgs);
            var args = $"a {opts} \"{savePath}\" \"{contentsPath}\"";
            var proc = new Process() { StartInfo = new() { 
                    FileName = "7za.exe",
                    Arguments = args,
                    UseShellExecute = false, 
                    RedirectStandardOutput = true, 
                    RedirectStandardError = true, 
                    CreateNoWindow = true 
                }
            };
            proc.Start();
            proc.WaitForExit();

            Log.Information("Create7zArchiveOnDisk() args: {args}", args);
            Log.Information("7z output: {output}", proc.StandardOutput.ReadToEnd());
            if (!proc.StandardError.EndOfStream) Log.Error(proc.StandardError.ReadToEnd());

            if (opts.Contains("-v")) return;
            
            // Check if the file exists, if it doesn't throw an error.
            if (!File.Exists(savePath))
                throw new Exception($"Failed to create 7z archive on disk at {savePath}");
        }

        [TestMethod]
        public void DP7zExtractorTest()
        {
            var l = Mock.Of<ILogger>();
            var f = new ProcessFactory();
            var e = new DP7zExtractor(l, f);
            Assert.AreEqual(l, e.Logger);
            Assert.AreEqual(f, e.Factory);
        }

        [TestMethod]
        [DynamicData(nameof(ArchiveEnumerable), DynamicDataSourceType.Property)]
        public void ExtractTest(string path)
        {
            var fi = FileSystem.CreateFileInfo(path);
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_FilesNotWhitelisted()
        {
            var scope = new DPFileScopeSettings(Array.Empty<string>(), new[] { Path.Combine(ExtractPath, "regular") }, true);
            var fi = new DPFileSystem(scope).CreateFileInfo(RegularArchivePath);
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(0), ErroredFiles = arc.Contents.Values.ToDictionary(x => x, x => ""), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_AfterExtract()
        {
            var fi = FileSystem.CreateFileInfo(RegularArchivePath);
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);
            e.Extract(settings);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_AfterExtractTemp()
        {
            var fi = FileSystem.CreateFileInfo(RegularArchivePath);
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);
            e.ExtractToTemp(settings);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }


        [TestMethod]
        [DynamicData(nameof(ArchiveEnumerable), DynamicDataSourceType.Property)]
        public void ExtractToTempTest(string path)
        {
            var fi = FileSystem.CreateFileInfo(path);
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            SetupTargetPathsForTemp(arc, ExtractPath);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings, true);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);

        }
        [TestMethod]
        public void ExtractToTempTest_AfterExtract()
        {
            var fi = FileSystem.CreateFileInfo(RegularArchivePath);
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            SetupTargetPathsForTemp(arc, ExtractPath);
            e.Extract(settings);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings, true);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        [DynamicData(nameof(ArchiveEnumerable), DynamicDataSourceType.Property)]
        public void PeekTest(string path)
        {
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory());
            var fi = FileSystem.CreateFileInfo(path);
            var arc = new DPArchive(fi);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);

            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractorSetPathsCorrectly(arc, arc.Contents.Values.Select(x => x.Path));
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void PeekTest_Encrypted()
        {
            var e = new DP7zExtractor();
            var fi = FileSystem.CreateFileInfo(EncryptedArchivePath);
            var arc = new DPArchive(fi);

            // Testing Peek() here:
            DPArchiveTestHelpers.RunAndAssertPeekEvents(e, arc);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_CancelledBeforeOp()
        {
            var fi = FileSystem.CreateFileInfo(RegularArchivePath);
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory());
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            e.CancellationToken = new CancellationToken(true);
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

        [TestMethod]
        public void ExtractTest_CancelledDuringExtractOp()
        {
            var processFactory = new Mock<IProcessFactory>();
            var proc = new ProcessWrapper();
            proc.OutputDataReceived += _ => 
            processFactory.SetupSequence(x => x.Create()).Returns(new ProcessWrapper())
                                                         .Returns(proc);

            var fi = FileSystem.CreateFileInfo(RegularArchivePath);
            var cts = new CancellationTokenSource();
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory()) { CancellationToken = cts.Token };
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(arc.Contents.Values), ErroredFiles = new(0), Settings = settings };
            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);
            proc.OutputDataReceived += _ => cts.Cancel(true);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);

            DPArchiveTestHelpers.AssertReport(expectedReport, report);
            DPIntegrationArchiveHelpers.AssertDefaultContentsNonDAZ(arc);
            DPArchiveTestHelpers.AssertExtractFileInfosCorrectlySet(arc.Contents.Values);
            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }
        [TestMethod]
        public void ExtractTest_CancelledDuringMoveOp()
        {
            var processFactory = new Mock<IProcessFactory>();
            var proc = new ProcessWrapper();
            proc.OutputDataReceived += _ => 
            processFactory.SetupSequence(x => x.Create()).Returns(new ProcessWrapper())
                                                         .Returns(proc);

            var fi = FileSystem.CreateFileInfo(RegularArchivePath);
            var cts = new CancellationTokenSource();
            var e = new DP7zExtractor(Log.Logger, new ProcessFactory()) { CancellationToken = cts.Token };
            var arc = new DPArchive(fi) { Extractor = e };
            arc.PeekContents();
            var settings = new DPExtractSettings(ExtractPath, arc.Contents.Values);
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new(1) { arc.Contents.First().Value }, ErroredFiles = new(0), Settings = settings };

            DPArchiveTestHelpers.SetupTargetPaths(arc, ExtractPath);
            e.MoveProgress += (_, __) => cts.Cancel(true);

            // Testing Extract() here:
            var report = RunAndAssertExtractEvents(e, settings);

            Assert.AreEqual(arc.FileSystem, e.FileSystem);
        }

    }
}