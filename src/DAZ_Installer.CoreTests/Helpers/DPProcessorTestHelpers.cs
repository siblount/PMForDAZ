using DAZ_Installer.Core.Extraction;
using DAZ_Installer.CoreTests.Extraction;
using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace DAZ_Installer.Core.Tests
{
    [Obsolete("For testing purposes only.")]
    internal class DPProcessorTestHelpers
    {
        /// <summary>
        /// Contains the DefaultContents from <see cref="DPArchiveTestHelpers"/> and the manifest and supplement."/>
        /// </summary>
        public static IEnumerable<string> DefaultContents => new string[] { "Manifest.dsx", "Supplement.dsx", "Contents/a.txt", "Contents/b.txt", "Contents/Documents/c.png", "Contents/Documents/d.txt", "Contents/e.duf", "Contents/f.duf", "bullshit.png" };
        public static MockOptions DefaultMockOptions => new();
        public static AssertOptions DefaultAssertOptions => new();
        public struct MockOptions
        {
            /// <summary>
            /// Determines whether to partially mock the <see cref="FakeFileInfo"/> object.
            /// </summary>
            public bool partialFileInfo = true;
            /// <summary>
            /// Determines whether to partially mock the <see cref="IRAR"/> object.
            /// </summary>
            public bool partialRAR = true;
            /// <summary>
            /// Determines whether to partially mock the <see cref="FakeDPFileInfo"/> object.
            /// </summary>
            public bool partialDPFileInfo = true;
            /// <summary>
            /// Determines whether to partially mock each <see cref="IZipArchiveEntry"/> object.
            /// </summary>
            public bool partialZipArchiveEntry = true;
            /// <summary>
            /// Determines whether to partially mock the <see cref="FakeFileSystem"/> object.
            /// </summary>
            public bool partialFakeFileSystem = true;
            /// <summary>
            /// The paths to use for the <see cref="DPArchive"/>. Defaults to <see cref="DefaultContents"/>.
            /// </summary>
            [Obsolete("Not used")]
            public IEnumerable<string> paths = DefaultContents;
            /// <summary>
            /// The function to use for extracting to a temporary directory, if any.
            /// </summary>
            public Func<DPExtractionReport>? ExtractToTempFunc = null;
            /// <summary>
            /// The function to use for extracting, if any.
            /// </summary>
            public Func<DPExtractionReport>? ExtractFunc = null;

            public MockOptions() { }
        }

        /// <summary>
        /// Creates a new <see cref="DPArchive"/> with mocked dependencies.
        /// </summary>
        /// <remarks>
        /// Completely sets up the dependencies for the <see cref="DPArchive"/> and returns the mocked dependencies. 
        /// Partial mocks will allow you to keep the original behavior of the mocked object while still allowing you to override certain methods.
        /// If you do not wish to override any methods (or mock everything), make sure you change <paramref name="options"/> to False for partial fields.
        /// <br/>
        /// The <paramref name="extractor"/> is setup to return a <see cref="DPExtractionReport"/> with the files that were extracted.
        /// </remarks>
        /// <param name="options">The mock options to use for setting up the archive.</param>
        /// <param name="extractor">The mock extractor that the <see cref="DPArchive"/> will use.</param>
        /// <param name="fakeDPFileInfo">The mock <see cref="FakeDPFileInfo"/> that the <see cref="DPArchive"/> will use.</param>
        /// <param name="fakeFileInfo">The mock <see cref="FakeFileInfo"/> that the <paramref name="fakeDPFileInfo"/> will use.</param>
        /// <param name="fakeFileSystem">The mock <see cref="FakeFileSystem"/> that the <paramref name="fakeDPFileInfo"/> will use.</param>
        /// <returns>A new <see cref="DPArchive"/> with mocked dependencies.</returns>
        public static DPArchive NewMockedArchive(MockOptions options, out Mock<DPAbstractExtractor> extractor, out Mock<FakeDPFileInfo> fakeDPFileInfo, out Mock<FakeFileInfo> fakeFileInfo, out Mock<FakeFileSystem> fakeFileSystem)
        {
            var fs = new Mock<FakeFileSystem>() { CallBase = options.partialFakeFileSystem };
            fakeFileSystem = fs;
            fakeFileInfo = new Mock<FakeFileInfo>("Z:/test.rar") { CallBase = options.partialFileInfo };
            fakeDPFileInfo = new Mock<FakeDPFileInfo>(fakeFileInfo.Object, fakeFileSystem.Object, null!) { CallBase = options.partialDPFileInfo };
            extractor = new Mock<DPAbstractExtractor>();
            var arc = new DPArchive(string.Empty, Log.Logger.ForContext<DPArchive>(), fakeDPFileInfo.Object, extractor.Object);
            extractor.Setup(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings x) =>
            {
                if (options.ExtractToTempFunc is null) return handleExtract(x, fs.Object);
                return options.ExtractToTempFunc();
            });
            extractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings x) =>
            {
                if (options.ExtractFunc is null) return handleExtract(x, fs.Object);
                return options.ExtractFunc();
            });
            return arc;
        }

        /// <summary>
        /// Initializes the DPProcessor by fully setting up its depenencies and creating the entities in the archive.
        /// </summary>
        /// <remarks>
        /// <paramref name="destDerm"/> returns a mock destination determiner that is setup to simply 
        /// return the contents of the <paramref name="arc"/>. <paramref name="arc"/> is also used for setting up 
        /// entities with <see cref="DefaultContents"/>
        /// <br/>
        /// Also note that <paramref name="tagProvider"/> returns a mock tag provider that is not setup.
        /// </remarks>
        /// <param name="arc">The archive that will be used for processes.</param>
        /// <param name="system">The fake file system to use.</param>
        /// <param name="destDerm">Returns a mock destination determiner.</param>
        /// <param name="tagProvider">Returns a mock tag provider.</param>
        /// <returns>A <see cref="DPProcessor"/> that is ready to be tested.</returns>
        public static DPProcessor SetupProcessor(DPArchive arc, FakeFileSystem system, out Mock<AbstractDestinationDeterminer> destDerm, out Mock<AbstractTagProvider> tagProvider)
        {
            var d = destDerm = new Mock<AbstractDestinationDeterminer>();
            d.Setup(x => x.DetermineDestinations(It.IsAny<DPArchive>(), It.IsAny<DPProcessSettings>())).Returns(() => arc.Contents.Values.ToHashSet());
            var t = tagProvider = new Mock<AbstractTagProvider>();
            var p = new DPProcessor()
            {
                Logger = Log.Logger.ForContext<DPProcessor>(),
                FileSystem = system,
                DestinationDeterminer = d.Object,
                TagProvider = t.Object,
            };
            SetupEntities(DefaultContents, arc);
            UpdateFileInfos(new DPExtractSettings("A:/", arc.Contents.Values, archive: arc), system);
            return p;
        }

        /// <summary>
        /// Assert a mix of common things for the <see cref="DPProcessor"/>.
        /// </summary>
        /// <remarks>
        /// Asserts that the <see cref="DPProcessor.DestinationDeterminer"/> and <see cref="DPProcessor.TagProvider"/> have been called <paramref name="time"/>s.
        /// It also asserts that the <see cref="DPProcessor.CurrentArchive"/> is null.
        /// </remarks>
        /// <param name="processor">The processor to perform assertions on.</param>
        /// <param name="time">
        /// The amount of times <see cref="DPProcessor.DestinationDeterminer"/> 
        /// and <see cref="DPProcessor.TagProvider"/> have been called. 
        /// If null, it will be <see cref="Times.Once"/>.
        /// </param>
        public static void AssertCommon(DPProcessor processor, Times? time = null)
        {
            var times = time is not null ? time.Value : Times.Once();
            Mock.Get(processor.DestinationDeterminer).Verify(x => x.DetermineDestinations(It.IsAny<DPArchive>(), It.IsAny<DPProcessSettings>()), times);
            Mock.Get(processor.TagProvider).Verify(x => x.GetTags(It.IsAny<DPArchive>(), It.IsAny<DPProcessSettings>()), times);
            Assert.IsNull(processor.CurrentArchive);
        }

        /// <summary>
        /// The default callback function for
        /// <see cref="NewMockedArchive(MockOptions, out Mock{DPAbstractExtractor}, out Mock{FakeDPFileInfo}, out Mock{FakeFileInfo}, out Mock{FakeFileSystem})"/>
        /// </summary>
        /// <remarks>
        /// Updates the file infos and returns a new <see cref="DPExtractionReport"/> with the files that were extracted.
        /// </remarks>
        /// <param name="settings"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        private static DPExtractionReport handleExtract(DPExtractSettings settings, FakeFileSystem fs)
        {
            UpdateFileInfos(settings, fs);
            return new DPExtractionReport()
            {
                ErroredFiles = new(0),
                ExtractedFiles = settings.FilesToExtract.ToList(),
                Settings = settings
            };
        }

        /// <summary>
        /// Creates a new <see cref="DPFolder"/> or <see cref="DPFile"/> 
        /// for each path in the <paramref name="paths"/> and adds it to the <paramref name="arc"/>.
        /// </summary>
        /// <param name="paths">The full path of folders or files to create.</param>
        /// <param name="arc">The archive to add the entities to.</param>
        private static void SetupEntities(IEnumerable<string> paths, DPArchive arc)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(Path.GetFileName(path))) new DPFolder(path, arc, null);
                else DPFile.CreateNewFile(path, arc, null);
            }
        }

        /// <summary>
        /// Updates the <see cref="IDPFileInfo"/> for each file in the <paramref name="settings"/>.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="system"></param>
        private static void UpdateFileInfos(DPExtractSettings settings, FakeFileSystem system)
        {
            foreach (var file in settings.Archive.Contents.Values)
            {
                var path = string.IsNullOrEmpty(file.TargetPath) ? Path.Combine(settings.TempPath, file.Path) : file.TargetPath;
                file.FileInfo = system.CreateFileInfo(path);
                var mockFileInfo = Mock.Get(file.FileInfo);
                var stream = DPArchiveTestHelpers.DetermineFileStream(file, settings.Archive);
                Exception? ex = null;
                mockFileInfo.Setup(x => x.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out ex))
                            .Callback((out Stream s, out Exception ex) =>
                            {
                                s = DPArchiveTestHelpers.DetermineFileStream(file, settings.Archive);
                                ex = null!;
                            })
                            .Returns(true);
            }
        }

        /// <summary>
        /// Assert options for the <see cref="DPProcessor"/>.
        /// </summary>
        public struct AssertOptions
        {
            public int ExpectedProcessErrorCount = 0;
            public int ExpectedArchiveCount = 1;
            public int ExpectedFileErrorCount = 0;
            /// <summary>
            /// A dictionary of expected archives to be processed.
            /// </summary>
            /// <typeparam name="string">The archive name</typeparam>
            /// <typeparam name="DPExtractionReport">The expected report</typeparam>
            public Dictionary<string, DPExtractionReport>? ExpectArchiveProcessed = null;

            public AssertOptions() { }
        }

        /// <summary>
        /// Attaches common event handlers to the <paramref name="processor"/> with the given <paramref name="opts"/> to assert.
        /// </summary>
        /// <param name="processor">The processor to attach event handlers to</param>
        /// <param name="opts">The assert options to use.</param>
        public static void AttachCommonEventHandlers(DPProcessor processor, AssertOptions opts)
        {
            int arcEnterCount = 0, arcExitCount = 0;
            int processErrorCount = 0;
            processor.ArchiveEnter += (_, e) =>
            {
                if (++arcEnterCount > opts.ExpectedArchiveCount) Assert.Fail("Archive Enter called more than expected");
            };
            processor.ArchiveExit += (_, e) =>
            {
                arcExitCount++;
                if (opts.ExpectArchiveProcessed is null) return;
                if (!opts.ExpectArchiveProcessed.TryGetValue(e.Archive.FileName, out var wantReport)) return;
                if (e.Report is null) Log.Logger.Warning("Report is null");
                else AssertReport(wantReport, e.Report);
            };
            processor.ProcessError += (_, e) =>
            {
                if (++processErrorCount > opts.ExpectedProcessErrorCount) Assert.Fail("Process Error called more than expected");
            };
            processor.Finished += () =>
            {
                if (arcEnterCount != arcExitCount) Assert.Fail("Archive Enter and Exit counts do not match");
            };
        }

        /// <summary>
        /// Create a new <see cref="DPExtractionReport"/> with the given settings and files.
        /// </summary>
        /// <param name="settings">The settings to use</param>
        /// <param name="failedFiles">Expected failed file(names).</param>
        /// <param name="successFiles">Expected successful file(names).</param>
        /// <returns>A configured <see cref="DPExtractionReport"/> with dummy files.</returns>
        public static DPExtractionReport CreateExtractionReport(DPExtractSettings settings, IEnumerable<string> failedFiles, IEnumerable<string>? successFiles)
        {
            var fh = failedFiles.ToHashSet();
            var sh = successFiles?.ToHashSet() ?? new HashSet<string>();
            var extractedFiles = successFiles?.ToList() ?? settings.Archive.Contents.Values.Where(m => !fh.Contains(m.Path)).Select(x => x.Path);
            if (fh.Intersect(sh).Count() > 1) Assert.Inconclusive("Failed and Success files intersect");
            return new DPExtractionReport()
            {
                Settings = settings,
                ErroredFiles = failedFiles.ToDictionary(m => CreateDummyFile(m), _ => string.Empty),
                ExtractedFiles = successFiles?.Select(x => new DPFile(x, null, null, null, null!)).ToList() ?? new List<DPFile>()
            };
        }

        /// <summary>
        /// Creates a <see cref="DPFile"/> with null dependencies.
        /// </summary>
        /// <param name="path">The path to set for this file.</param>
        /// <returns>A file with null <see cref="DPArchive"/>, <see cref="DPFolder"/>, <see cref="IDPFileInfo"/>, and <see cref="ILogger"/>.</returns>
        public static DPFile CreateDummyFile(string path) => new(path, null, null, null, null!);
        /// <summary>
        /// Creates a <see cref="DPFolder"/> with null dependencies.
        /// </summary>
        /// <param name="path">The path to set for this folder.</param>
        /// <returns>A folder with null <see cref="DPArchive"/> and parent <see cref="DPFolder"/></returns>
        public static DPFolder CreateDummyFolder(string path) => new(path, null, null!);
        /// <summary>
        /// Asserts the <see cref="DPExtractionReport"/> objects are equal.
        /// </summary>
        /// <param name="want">The expected report</param>
        /// <param name="got">The result report from an operation</param>
        public static void AssertReport(DPExtractionReport want, DPExtractionReport got)
        {
            var a = want.ErroredFiles.Keys.Select(x => x.Path).ToArray();
            var b = got.ErroredFiles.Keys.Select(x => x.Path).ToArray();
            CollectionAssert.AreEqual(a, b, "Errored file paths are not equal");
            a = want.ExtractedFiles.Select(x => x.Path).ToArray();
            b = got.ExtractedFiles.Select(x => x.Path).ToArray();
            CollectionAssert.AreEqual(a, b, "Extracted file paths are not equal");

            // Settings
            Assert.AreSame(want.Settings.Archive, got.Settings.Archive, "Reports' archive are not the same");
            CollectionAssert.AreEqual(want.Settings.FilesToExtract.Select(x => x.Path).ToArray(), got.Settings.FilesToExtract.Select(x => x.Path).ToArray(), "Reports' files to extract are not the same");
            Assert.AreEqual(want.Settings.OverwriteFiles, got.Settings.OverwriteFiles, "Reports' overwrite files are not the same");
        }
        /// <summary>
        /// Calculates the expected files from a list of files.
        /// </summary>
        /// <remarks>
        /// It simply filters out any files that have an empty file name.
        /// </remarks>
        /// <param name="files">The list of files to calculate.</param>
        /// <returns>A list of expected filenames.</returns>
        public static List<string> CalculateExpectedFiles(IEnumerable<string> files) => files.Where(x => !string.IsNullOrEmpty(Path.GetFileName(x))).ToList();
        /// <summary>
        /// Creates a new <see cref="DPExtractSettings"/> with the given paths and archive.
        /// </summary>
        /// <param name="paths">The paths of the archive. Paths can be empty or null, it will be filtered out.</param>
        /// <param name="arc">The archive to extract.</param>
        /// <returns>A set-up <see cref="DPExtractSettings"/> object.</returns>
        public static DPExtractSettings CreateExtractSettings(IEnumerable<string> paths, DPArchive arc) => 
            new("A:/", paths.Where(x => !string.IsNullOrEmpty(Path.GetFileName(x))).Select(x => CreateDummyFile(x)), archive: arc);

    }
}
