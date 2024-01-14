using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.Core.Extraction.Fakes;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO;
using DAZ_Installer.CoreTests.Extraction;
using Serilog;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using static DAZ_Installer.Core.Tests.Helpers.DPDestinationDeterminerTestHelpers;
using System.Reflection;
using System.IO;
using System.Xml.Linq;

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPDestinationDeterminerTests
    {
        // Supplement and Manifest should not be included.
        public static readonly string[] DefaultManifestPaths = new[] { "Manifest.dsx", "Supplement.dsx", "Content/data/TheReaolSolly/a.txt", "Content/docs/TheRealSolly/b.txt" };
        public static readonly string[] DefaultNonDazPaths = new[] { "data/TheRealSolly/a.txt", "docs/TheRealSolly/b.txt", "should not be included.txt" };
        private static readonly MockOptions DazArchiveOpts = new();
        private static readonly MockOptions NonDazOpts = new() { paths = DefaultNonDazPaths };
        private static readonly DPProcessSettings ManifestProcessSettings = new("Z:/",
                                                                                "Z:/",
                                                                                InstallOptions.ManifestOnly,
                                                                                DPProcessor.DefaultContentFolders.ToHashSet(),
                                                                                new Dictionary<string, string>(DPProcessor.DefaultRedirects));
        private static readonly DPProcessSettings AutoProcessSettings = ManifestProcessSettings with { InstallOption = InstallOptions.Automatic };
        private static readonly DPProcessSettings BothProcessSettings = ManifestProcessSettings with { InstallOption = InstallOptions.ManifestAndAuto };

        // TODO: Test DPDestinationDeterminer when a folder is not included in the content redirect folders map.
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();
        }
        public struct MockOptions
        {
            public bool partialFileInfo = true;
            public bool partialDPFileInfo = true;
            public bool partialZipArchiveEntry = true;
            public bool partialFileSystem = true;
            public string[] paths = DefaultManifestPaths;

            public MockOptions() { }
        }

        private static DPArchive NewMockedArchive(MockOptions options, out Mock<FakeDPFileInfo> fakeDPFileInfo, out Mock<FakeFileInfo> fakeFileInfo, out Mock<FakeFileSystem> fakeFileSystem)
        {
            var fs = new Mock<FakeFileSystem>(DPFileScopeSettings.All) { CallBase = options.partialFileSystem };
            fs.Object.PartialMock = options.partialFileSystem;
            fakeFileSystem = fs;
            fakeFileInfo = new Mock<FakeFileInfo>("Z:/test.zip") { CallBase = options.partialFileInfo };
            fakeDPFileInfo = new Mock<FakeDPFileInfo>(fakeFileInfo.Object, fs.Object, null) { CallBase = options.partialFileInfo };
            var arc = new DPArchive(string.Empty, Log.Logger.ForContext<DPArchive>(), fakeDPFileInfo.Object, Mock.Of<DPAbstractExtractor>());
            foreach (var file in options.paths)
            {
                var dpFile = DPFile.CreateNewFile(file, arc, null);
                var fi = fakeFileSystem.Object.CreateFileInfo(file);
                dpFile.FileInfo = fi;
                Exception? ex = null;
                Mock.Get(fi).Setup(x => x.TryAndFixOpenRead(out It.Ref<Stream>.IsAny, out ex))
                            .Callback((out Stream s, out Exception ex) =>
                            {
                                s = DPArchiveTestHelpers.DetermineFileStream(dpFile, arc, arc.Contents.Values.Where(x => x.Parent is not null).Select(x => x.Path));
                                ex = null;
                            })
                            .Returns(true);
            }
            return arc;
        }
        
        [TestMethod]
        public void DetermineDestinationsTest_ManifestDAZArchive()
        {
            var tc = new {
                Paths = DefaultManifestPaths,
                Settings = ManifestProcessSettings,
                Options = DazArchiveOpts,
                ExpectedTargetPaths = new[] { "Z:/data/TheReaolSolly/a.txt", "Z:/Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathsTargetPaths = new[] { "data/TheReaolSolly/a.txt", "Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathToContentFolders = new[] { "data/TheReaolSolly/a.txt", "docs/TheRealSolly/b.txt" },
                ExpectedContentFoldersMarked = new[] { "Content/data", "Content/docs" },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(arc.Contents.Values.Where(x => x.Parent is not null));
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_ManifestNonDAZArchive()
        {
            var tc = new
            {
                Paths = DefaultNonDazPaths,
                Settings = ManifestProcessSettings,
                Options = NonDazOpts,
                ExpectedTargetPaths = new string[] { },
                ExpectedRelativePathsTargetPaths = new string[] { },
                ExpectedRelativePathToContentFolders = new string[] { },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            var expected = new HashSet<DPFile>(0);
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
        }

        [TestMethod]
        public void DetermineDestinationsTest_AutoDAZArchive()
        {
            var tc = new
            {
                Paths = DefaultManifestPaths,
                Settings = AutoProcessSettings,
                Options = DazArchiveOpts,
                ExpectedTargetPaths = new[] { "Z:/data/TheReaolSolly/a.txt", "Z:/Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathsTargetPaths = new[] { "data/TheReaolSolly/a.txt", "Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathToContentFolders = new[] { "data/TheReaolSolly/a.txt", "docs/TheRealSolly/b.txt" },
                ExpectedContentFoldersMarked = new[] { "Content/data", "Content/docs" },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(arc.Contents.Values.Where(x => x.Parent is not null));
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_AutoNonDAZArchive()
        {
            var tc = new
            {
                Paths = DefaultNonDazPaths,
                Settings = AutoProcessSettings,
                Options = NonDazOpts,
                ExpectedTargetPaths = new[] { "Z:/data/TheRealSolly/a.txt", "Z:/Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathsTargetPaths = new[] { "data/TheRealSolly/a.txt", "Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathToContentFolders = new[] { "data/TheRealSolly/a.txt", "docs/TheRealSolly/b.txt" },
                ExpectedContentFoldersMarked = new[] { "data", "docs" },
            };

            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(arc.Contents.Values.Where(x => x.Parent is not null));
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_BothDAZArchive()
        {
            var tc = new
            {
                Paths = DefaultManifestPaths,
                Settings = BothProcessSettings,
                Options = DazArchiveOpts,
                ExpectedTargetPaths = new[] { "Z:/data/TheReaolSolly/a.txt", "Z:/Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathsTargetPaths = new[] { "data/TheReaolSolly/a.txt", "Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathToContentFolders = new[] { "data/TheReaolSolly/a.txt", "docs/TheRealSolly/b.txt" },
                ExpectedContentFoldersMarked = new[] { "Content/data", "Content/docs" },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(arc.Contents.Values.Where(x => x.Parent is not null));
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_BothNonDAZArchive()
        {
            var tc = new
            {
                Paths = DefaultNonDazPaths,
                Settings = BothProcessSettings,
                Options = NonDazOpts,
                ExpectedTargetPaths = new[] { "Z:/data/TheRealSolly/a.txt", "Z:/Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathsTargetPaths = new[] { "data/TheRealSolly/a.txt", "Documentation/TheRealSolly/b.txt" },
                ExpectedRelativePathToContentFolders = new[] { "data/TheRealSolly/a.txt", "docs/TheRealSolly/b.txt" },
                ExpectedContentFoldersMarked = new[] { "data", "docs" },
            };

            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(arc.Contents.Values.Where(x => x.Parent is not null));
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_NoRedirectsDAZManifest()
        {
            var tc = new
            {
                Paths = DefaultManifestPaths,
                Settings = ManifestProcessSettings with { ContentRedirectFolders = new() },
                Options = DazArchiveOpts,
                ExpectedTargetPaths = new[] { "Z:/data/TheReaolSolly/a.txt", "Z:/docs/TheRealSolly/b.txt" },
                ExpectedRelativePathsTargetPaths = new[] { "data/TheReaolSolly/a.txt", "docs/TheRealSolly/b.txt" },
                ExpectedRelativePathToContentFolders = new[] { "data/TheReaolSolly/a.txt", "docs/TheRealSolly/b.txt" },
                ExpectedContentFoldersMarked = new[] { "Content/data", "Content/docs" },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(arc.Contents.Values.Where(x => x.Parent is not null));
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_NoRedirectsDAZManifestAuto()
        {
            var tc = new
            {
                Paths = DefaultManifestPaths,
                Settings = AutoProcessSettings with { ContentRedirectFolders = new() },
                Options = DazArchiveOpts,
                ExpectedTargetPaths = new[] { "Z:/data/TheReaolSolly/a.txt" },
                ExpectedRelativePathsTargetPaths = new[] { "data/TheReaolSolly/a.txt" },
                ExpectedRelativePathToContentFolders = new[] { "data/TheReaolSolly/a.txt" },
                ExpectedContentFoldersMarked = new[] { "Content/data" },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>( new[] { arc.Contents[PathHelper.NormalizePath("Content/data/TheReaolSolly/a.txt")] });
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_NoRedirectsDAZManifestBoth()
        {
            var tc = new
            {
                Paths = DefaultManifestPaths,
                Settings = BothProcessSettings with { ContentRedirectFolders = new() },
                Options = DazArchiveOpts,
                ExpectedTargetPaths = new[] { "Z:/data/TheReaolSolly/a.txt", "Z:/docs/TheRealSolly/b.txt" },
                ExpectedRelativePathsTargetPaths = new[] { "data/TheReaolSolly/a.txt", "docs/TheRealSolly/b.txt" },
                ExpectedRelativePathToContentFolders = new[] { "data/TheReaolSolly/a.txt", "docs/TheRealSolly/b.txt" },
                ExpectedContentFoldersMarked = new[] { "Content/data", "Content/docs" },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(arc.Contents.Values.Where(x => x.Parent is not null));
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_ContentFolderTree()
        {
            var paths = new string[] { "data/data/a.txt", "docs/docs/b.txt", "data/data/docs/docs/c.txt" };
            var tc = new
            {
                Paths = paths,
                Settings = AutoProcessSettings,
                Options = new MockOptions() { paths = paths },
                ExpectedTargetPaths = new[] { "Z:/data/data/a.txt", "Z:/Documentation/docs/b.txt", "Z:/data/data/docs/docs/c.txt" },
                ExpectedRelativePathsTargetPaths = new[] { "data/data/a.txt", "Documentation/docs/b.txt", "data/data/docs/docs/c.txt" },
                ExpectedRelativePathToContentFolders = new[] { "data/data/a.txt", "docs/docs/b.txt", "data/data/docs/docs/c.txt" },
                ExpectedContentFoldersMarked = new[] { "data", "docs" },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(arc.Contents.Values.Where(x => x.Parent is not null));
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_NoDirs()
        {
            var paths = new string[] { "a.txt", "b.txt", "c.txt" };
            var tc = new
            {
                Paths = paths,
                Settings = AutoProcessSettings,
                Options = new MockOptions() { paths = paths },
                ExpectedTargetPaths = new string[] {  },
                ExpectedRelativePathsTargetPaths = new string[] {  },
                ExpectedRelativePathToContentFolders = new string[] {  },
                ExpectedContentFoldersMarked = new string[] { },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(0);
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }

        [TestMethod]
        public void DetermineDestinationsTest_NoDirsEmptyContentFolderSet()
        {
            var paths = new string[] { "a.txt", "b.txt", "c.txt" };
            var tc = new
            {
                Paths = paths,
                Settings = AutoProcessSettings with { ContentFolders = new HashSet<string>() { "" } },
                Options = new MockOptions() { paths = paths },
                ExpectedTargetPaths = new string[] { },
                ExpectedRelativePathsTargetPaths = new string[] { },
                ExpectedRelativePathToContentFolders = new string[] { },
                ExpectedContentFoldersMarked = new string[] { },
            };
            var arc = NewMockedArchive(tc.Options, out var _, out _, out _);
            var determiner = new DPDestinationDeterminer();
            DPArchiveTestHelpers.SetupTargetPaths(arc, "Z:/");

            var actual = determiner.DetermineDestinations(arc, tc.Settings);

            // Everything under the "Contents" folder should be extracted.
            var expected = new HashSet<DPFile>(0);
            AssertDestinations(expected, actual, "Z:/");
            AssertTargetPaths(actual, tc.ExpectedTargetPaths);

            // Additional assertions for ExpectedRelativePathsTargetPaths, ExpectedRelativePathToContentFolders, and ExpectedContentFoldersMarked
            AssertRelativePaths(arc, tc.ExpectedRelativePathsTargetPaths, tc.ExpectedRelativePathToContentFolders);
            AssertContentFolders(arc, tc.ExpectedContentFoldersMarked);
        }
    }
}