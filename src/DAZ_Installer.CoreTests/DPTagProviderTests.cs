using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Serilog;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO.Fakes;
using Moq;
using DAZ_Installer.CoreTests.Extraction;

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPTagProviderTests
    {
        public static IEnumerable<string> DefaultContents => new string[] { "Manifest.dsx", "Supplement.dsx", "Contents/a.txt", "Contents/b.txt", "Contents/Documents/c.png", "Contents/Documents/d.txt", "Contents/e.duf", "Contents/f.duf", "bullshit.png" };
        public static MockOptions DefaultMockOptions => new();

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
            public bool partialRAR = true;
            public bool partialDPFileInfo = true;
            public bool partialZipArchiveEntry = true;
            public bool partialFakeFileSystem = true;
            public IEnumerable<string> paths = DefaultContents;
            public Func<DPExtractionReport>? ExtractToTempFunc = null;
            public Func<DPExtractionReport>? ExtractFunc = null;

            public MockOptions() { }
        }
        public static DPArchive NewMockedArchive(MockOptions options, out Mock<DPAbstractExtractor> extractor, out Mock<FakeDPFileInfo> fakeDPFileInfo, out Mock<FakeFileInfo> fakeFileInfo, out Mock<FakeFileSystem> fakeFileSystem)
        {
            var fs = new Mock<FakeFileSystem>() { CallBase = options.partialFakeFileSystem };
            fakeFileSystem = fs;
            fakeFileInfo = new Mock<FakeFileInfo>("Z:/test.rar") { CallBase = options.partialFileInfo };
            fakeDPFileInfo = new Mock<FakeDPFileInfo>(fakeFileInfo.Object, fakeFileSystem.Object, null) { CallBase = options.partialDPFileInfo };
            extractor = new Mock<DPAbstractExtractor>();
            var arc = new DPArchive(string.Empty, Log.Logger.ForContext<DPArchive>(), fakeDPFileInfo.Object, extractor.Object);
            extractor.Setup(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings x) =>
            {
                return options.ExtractToTempFunc is null ? handleExtract(x, fs.Object) : options.ExtractToTempFunc();
            });
            extractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings x) =>
            {
                return options.ExtractFunc is null ? handleExtract(x, fs.Object) : options.ExtractFunc();
            });
            SetupEntities(options.paths, arc);
            extractor.Object.Extract(new DPExtractSettings("Z:/", Enumerable.Empty<DPFile>(), true, arc));
            return arc;
        }

        private static void SetupEntities(IEnumerable<string> paths, DPArchive arc)
        {
            foreach (var path in paths)
            {
                if (string.IsNullOrEmpty(Path.GetFileName(path))) new DPFolder(path, arc, null);
                else DPFile.CreateNewFile(path, arc, null);
            }
        }

        private static void UpdateFileInfos(DPExtractSettings settings, FakeFileSystem system)
        {
            foreach (var file in settings.Archive.Contents.Values)
            {
                var path = string.IsNullOrEmpty(file.TargetPath) ? Path.Combine(settings.TempPath, file.Path) : file.TargetPath;
                file.FileInfo = system.CreateFileInfo(path);
                var mockFileInfo = Mock.Get(file.FileInfo);
                var stream = DPArchiveTestHelpers.DetermineFileStream(file, settings.Archive);
                Exception? ex = null;
                mockFileInfo.Setup(x => x.TryAndFixOpenRead(out It.Ref<Stream>.IsAny, out ex))
                            .Callback((out Stream s, out Exception ex) =>
                            {
                                s = DPArchiveTestHelpers.DetermineFileStream(file, settings.Archive);
                                ex = null;
                            })
                            .Returns(true);
            }
        }

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

        private static void AssertTagsEqual(DPArchive arc, HashSet<string> actual)
        {
            // Make sure that all of the ProductInfo tags are in the actual tags.
            foreach (var tag in arc.DazFiles.Where(x => x.Extracted))
            {
                var ci = tag.ContentInfo;
                Assert.IsTrue(actual.Contains(ci.ContentType.ToString()), "Content type not found in Tags.");
                Assert.IsTrue(actual.Contains(ci.Email), "Email not found in Tags.");
                Assert.IsTrue(actual.Contains(ci.Website), "Website not found in Tags.");
                foreach (var author in ci.Authors)
                    Assert.IsTrue(actual.Contains(author), "Author not found in Tags.");
            }
            foreach (var s in DPArchive.RegexSplitName(arc.ProductInfo.ProductName))
            {
                Assert.IsTrue(actual.Contains(s));
            }
        }

        [TestMethod]
        public void GetTagsTest()
        {
            var arc = NewMockedArchive(DefaultMockOptions, out var e, out var dpfi, out var fi, out var fs);
            var tp = new DPTagProvider();

            var result = tp.GetTags(arc, new DPProcessSettings("Z:/", "Z:/", InstallOptions.Automatic));

            AssertTagsEqual(arc, result);
        }

        [TestMethod]
        public void GetTagsTest_UnderscoreName()
        {
            var arc = NewMockedArchive(DefaultMockOptions, out var e, out var dpfi, out var fi, out var fs);
            fi.Object.FullName = "Z:/i_am_leg.rar";
            var tp = new DPTagProvider();

            var result = tp.GetTags(arc, new DPProcessSettings("Z:/", "Z:/", InstallOptions.Automatic));

            AssertTagsEqual(arc, result);
            Assert.IsTrue(result.Contains("i") && result.Contains("am") && result.Contains("leg"), "Underlined name not found in Tags.");
        }

        [TestMethod]
        public void GetTagsTest_DashName()
        {
            var arc = NewMockedArchive(DefaultMockOptions, out var e, out var dpfi, out var fi, out var fs);
            fi.Object.FullName = "Z:/i-am-leg.rar";
            var tp = new DPTagProvider();

            var result = tp.GetTags(arc, new DPProcessSettings("Z:/", "Z:/", InstallOptions.Automatic));

            AssertTagsEqual(arc, result);
            Assert.IsTrue(result.Contains("i") && result.Contains("am") && result.Contains("leg"), "Underlined name not found in Tags.");
        }

        [TestMethod]
        public void GetTagsTest_PlusName()
        {
            var arc = NewMockedArchive(DefaultMockOptions, out var e, out var dpfi, out var fi, out var fs);
            fi.Object.FullName = "Z:/i+am+leg.rar";
            var tp = new DPTagProvider();

            var result = tp.GetTags(arc, new DPProcessSettings("Z:/", "Z:/", InstallOptions.Automatic));

            AssertTagsEqual(arc, result);
            Assert.IsTrue(result.Contains("i") && result.Contains("am") && result.Contains("leg"), "Underlined name not found in Tags.");
        }

        [TestMethod]
        public void GetTagsTest_MixedName()
        {
            var arc = NewMockedArchive(DefaultMockOptions, out var e, out var dpfi, out var fi, out var fs);
            fi.Object.FullName = "Z:/a+b-c_d.rar";
            var tp = new DPTagProvider();

            var result = tp.GetTags(arc, new DPProcessSettings("Z:/", "Z:/", InstallOptions.Automatic));

            AssertTagsEqual(arc, result);
            Assert.IsTrue(result.Contains("a") && result.Contains("b") && result.Contains("c") && result.Contains("d"), "Underlined name not found in Tags.");
        }
    }
}