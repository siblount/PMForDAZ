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

        /// <summary>
        /// Asserts that the expected and actual reports are equal.
        /// </summary>
        /// <remarks>
        /// We only compare the file paths for extracted and errored files. 
        /// For expected, create a IDPFile (or a dummy file) with the expected paths.
        /// </remarks>
        /// <param name="p">The processor to assert the report on.</param> 
        /// <param name="expected">The expected args.</param>
        /// <param name="task">The assertable task to assert after the processor has executed.</param>
        internal static async Task AssertArchiveExit(DPProcessor p, DPArchiveExitArgs expected, DPProcessorTestHelpers.AssertableTask task)
        {
            var called = false;
            p.ArchiveExit += async (_, args) => {
                task.AddAssertion(() => Assert.IsNotNull(args.Archive.FileInfo, "Archive FileInfo is null"));
                task.AddAssertion(() => Assert.AreEqual(expected.Archive.FileName, args.Archive.FileName, "Archive filename mismatch"));
                task.AddAssertion(() => Assert.AreEqual(expected.Processed, args.Processed, "Processed mismatch"));
                if (args.Report is null && expected.Report is null) return;
                if ((args.Report is null && args.Report is not null) || (args.Report is not null && args.Report is null)) 
                    task.AddAssertion(() => Assert.Fail("Expected report is null but actual is not"));
                
                // Assert 
                task.AddAssertion(() => AssertExtractionSettings(expected.Report!.Settings, args.Report!.Settings));
                HashSet<string> actualFiles = new(args.Report!.ExtractedFiles.Select(x => x.Path));
                foreach (var file in expected.Report!.ExtractedFiles.Select(x => x.Path))
                {
                    if (!actualFiles.Contains(file))
                        task.AddAssertion(() => Assert.Fail($"Expected file '{file}' not found in actual files"));
                }
                Dictionary<string, string> actualErrors = new(args.Report.ErroredFiles.Select(x => new KeyValuePair<string, string>(x.Key.Path, x.Value)));
                foreach (var error in expected.Report.ErroredFiles)
                {
                    if (actualErrors.TryGetValue(error.Key.Path, out var actualError) && actualError != error.Value) {
                        task.AddAssertion(() => Assert.Fail($"Got unexpected error for file '{error.Key.Path}': {actualError}"));
                    }
                    else if (actualError is null) task.AddAssertion(() => Assert.Fail($"Expected error for file '{error.Key.Path}' not found in actual errors"));
                }
            };

            await task;

            Assert.IsTrue(called, "ArchiveExit event not called");
        }

        public static void AssertExtractionSettings(DPExtractSettings expected, DPExtractSettings actual) {
            Assert.AreEqual(expected.Archive.FileName, actual.Archive.FileName);
            Assert.AreEqual(expected.OverwriteFiles, actual.OverwriteFiles);
            Assert.AreEqual(expected.TempPath, actual.TempPath);
            var expectedFilesToExtractFileNames = new HashSet<string>(expected.FilesToExtract.Select(x => x.FileName));
            foreach (var file in actual.FilesToExtract)
            {
                if (!expectedFilesToExtractFileNames.Contains(file.FileName))
                    Assert.Fail($"Expected file '{file.FileName}' not found in actual files");
            }
        }

        /// <summary>
        /// Asserts that the extraction progress event is called with the expected values.
        /// </summary>
        /// <param name="p">The processor to assert the event on.</param>
        /// <param name="arcFileName">The archive name to assert on.</param>
        /// <param name="task">The assertable task to assert after the processor has executed.</param>
        internal static async Task AssertExtractionProgress(DPProcessor p, string arcFileName, DPProcessorTestHelpers.AssertableTask task) {
            var lastNum = 0;
            var called = false;
            p.ExtractProgress += async (_, args) => {
                if (args.Archive.FileName != arcFileName) return;
                called = true;
                if (args.File is null) Log.Warning("ExtractionProgress event called with null file");
                lastNum = args.ExtractionPercentage;
            };

            await task;

            Assert.IsTrue(called, "ExtractionProgress event not called");
            Assert.AreEqual(100, lastNum);
        }

        public static DPExtractSettings CreateExtractSettings(DPProcessSettings settings, List<string> relativeFilePaths) {
            var expectedTempBase = Path.Combine(settings.TempPath, @"DazProductInstaller\");
            return new DPExtractSettings(
                expectedTempBase,
                relativeFilePaths.Select(x => Path.Combine(expectedTempBase, x)).Select(x => new DPFile(x, null, null)).ToList(),
                true,
                new DPArchive(FileSystem.CreateFileInfo(ArchivePath))
            );
        }

        [TestMethod]
        public void ProcessArchiveTest()
        {
            var a = new DPArchive(FileSystem.CreateFileInfo(ArchivePath));
            var p = new DPProcessor() { FileSystem = FileSystem };
            var settings = CreateExtractSettings(DefaultProcessSettings, ArchiveContents);
            var expectedFiles = new[] { "data/TheRealSolly/data.dsf", "data/TheRealSolly/a.txt", "docs/b.txt" };
            var expectedReport = DPProcessorTestHelpers.CreateExtractionReport(settings, Enumerable.Empty<string>(), expectedFiles);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(ArchivePath, DefaultProcessSettings));

            p.ArchiveEnter += async (gp, args) => {
                processorTask.AddAssertion(() => Assert.AreEqual(ArchivePath, args.Archive.FileInfo.Path, "Archive FileInfo Path mismatch")); 
                processorTask.AddAssertion(() => Assert.AreEqual(p, gp));
            };
            var arc = new Mock<IDPArchive>();
            arc.Setup(x => x.FileName).Returns(Path.GetFileName(ArchivePath));

            var expectedArgs = new DPArchiveExitArgs(arc.Object, expectedReport, true);
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
                AssertArchiveExit(p, expectedArgs, processorTask),
                AssertExtractionProgress(p, a.FileName, processorTask),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            processorTask.RunSynchronously();

            processorTask.Assert();
        }
    }
}