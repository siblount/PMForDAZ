using DAZ_Installer.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Serilog;
using DAZ_Installer.Core.Extraction;
using Moq;
using DAZ_Installer.IO;
using DAZ_Installer.Core.Tests.Fakes;
using System.Linq.Expressions;

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPArchiveTests
    {
        public Mock<IDPExtractorFactory> MockExtractorFactory = null!;
        public Mock<DPAbstractExtractor> MockExtractor = null!;
        public Mock<IDPFileInfo> MockDPFileInfo = null!;
        public IDPExtractorFactory ExtractorFactory => MockExtractorFactory.Object;
        public DPAbstractExtractor Extractor => MockExtractor.Object;
        public IDPFileInfo TestFileInfo => MockDPFileInfo.Object;
        private static readonly byte[] RAR5Header = [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00];
        private static readonly byte[] RAR4Header = [0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00];
        private static readonly byte[] SevenZHeader = [0x37, 0x7A, 0xBC, 0xAF];
        private static readonly byte[] ZipHeader = [0x50, 0x4B, 0x57, 0x69];
        private static readonly byte[] RAR4HeaderPadded = [.. RAR4Header, 0x00];
        private static readonly byte[] SevenZHeaderPadded = [.. SevenZHeader, 0x00, 0x00, 0x00, 0x00];
        private static readonly byte[] ZipHeaderPadded = [.. ZipHeader, 0x00, 0x00, 0x00, 0x00];


        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
                                                  .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                                                  .MinimumLevel.Information()
                                                  .CreateLogger();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            MockExtractorFactory = new Mock<IDPExtractorFactory>();
            MockExtractor = new Mock<DPAbstractExtractor>();
            MockExtractorFactory.Setup(x => x.CreateExtractor(It.IsAny<ArchiveFormat>())).Returns(MockExtractor.Object);
            MockDPFileInfo = new Mock<IDPFileInfo>();
            MockDPFileInfo.Setup(x => x.OpenRead()).Returns(new MemoryStream(ZipHeader));
        }

        private static bool CompareExtractSettings(DPExtractSettings expected, DPExtractSettings got) =>
                expected.Archive == got.Archive
                && Enumerable.SequenceEqual(expected.FilesToExtract, got.FilesToExtract)
                && expected.OverwriteFiles == got.OverwriteFiles
                && expected.TempPath == got.TempPath;

        private static Span<byte> GetHeaderData(string nameofHeader)
        {
            return nameofHeader switch
            {
                nameof(RAR4HeaderPadded) => (Span<byte>)RAR4HeaderPadded,
                nameof(RAR5Header) => (Span<byte>)RAR5Header,
                nameof(ZipHeaderPadded) => (Span<byte>)ZipHeaderPadded,
                nameof(SevenZHeaderPadded) => (Span<byte>)SevenZHeaderPadded,
                _ => [],
            };
        }

        [TestMethod]
        public void DPArchiveTest()
        {
            var archive = new DPArchive(TestFileInfo);

            Assert.IsInstanceOfType(archive.Extractor, typeof(DPZipExtractor));
            Assert.AreEqual(ArchiveFormat.PKZip, archive.ArchiveFormat);
            Assert.AreSame(TestFileInfo, archive.FileInfo);
        }

        [TestMethod]
        public void DPArchiveTest_NullParent()
        {
            var parentArchive = new DPArchive()
            {
                Extractor = Extractor,
                ExtractorFactory = ExtractorFactory,
            };
            var archive = new DPArchive("a.zip", parentArchive);

            Assert.AreSame(parentArchive.ExtractorFactory, archive.ExtractorFactory);
            Assert.AreSame(parentArchive.Extractor, Extractor);
            Assert.AreSame(archive.FileName, archive.RelativePathToContentFolder);
            Assert.AreEqual("a", archive.ProductInfo.ProductName);
        }

        [TestMethod]
        public void DPArchiveTest_NullParentExtractor()
        {
            var parentArchive = new DPArchive()
            {
                Extractor = Extractor,
                ExtractorFactory = null!,
            };
            var archive = new DPArchive("a.zip", parentArchive);

            Assert.AreSame(archive.ExtractorFactory, DPExtractorFactory.Singleton);
            Assert.IsInstanceOfType(archive.Extractor, typeof(DPZipExtractor));
            Assert.AreSame(archive.FileName, archive.RelativePathToContentFolder);
            Assert.AreEqual("a", archive.ProductInfo.ProductName);
        }

        [TestMethod]
        public void DPArchiveTest_Parent()
        {
            var parentArchive = new DPArchive()
            {
                Extractor = Extractor,
                ExtractorFactory = null!,
            };
            var parentFolder = Mock.Of<FakeDPFolder>();
            var archive = new DPArchive("a.zip", parentArchive, parentFolder);

            Assert.AreSame(archive.ExtractorFactory, DPExtractorFactory.Singleton);
            Assert.AreSame(parentArchive.Extractor, Extractor);
            Assert.AreSame(archive.FileName, archive.RelativePathToContentFolder);
            Assert.AreEqual("a", archive.ProductInfo.ProductName);
        }

        [TestMethod]
        [DataRow("7z", ArchiveFormat.SevenZ)]
        [DataRow("rar", ArchiveFormat.RAR)]
        [DataRow("zip", ArchiveFormat.PKZip)]
        [DataRow("001", ArchiveFormat.SevenZ)]
        [DataRow("a", ArchiveFormat.Unknown)]
        [DataRow("002", ArchiveFormat.Unknown)]
        [DataRow("", ArchiveFormat.Unknown)]
        [DataRow("r00", ArchiveFormat.Unknown)]

        public void DetermineArchiveFormatTest(string ext, ArchiveFormat expected)
        {
            Assert.AreEqual(expected, DPArchive.DetermineArchiveFormat(ext));
        }

        [TestMethod]
        public void ExtractAllContentsTest()
        {
            var archive = new DPArchive(TestFileInfo) { Extractor = Extractor };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            DPExtractionReport? report = null;
            var expectedSettings = new DPExtractSettings("A:/temp", [file1, file2], true, archive);
            
            MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings settings) =>
            {
                return report = new DPExtractionReport() { ExtractedFiles = [file1, file2], Settings = settings };
            });
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(true);

            var result = archive.ExtractAllContents("A:/temp", true);
            Assert.AreEqual(report, result);
        }

        [TestMethod]
        public void ExtractContentsTest_ArchiveNotOnDisk()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = null,
                AssociatedArchive = mockParentArchive.Object
            };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            DPExtractionReport? report = null;
            var expectedSettings = new DPExtractSettings("A:/temp", [file1, file2], true, mockParentArchive.Object);
            
            MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings settings) =>
            {
                return report = new DPExtractionReport() { ExtractedFiles = [file1, file2], Settings = settings };
            });
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ExtractedFiles = [archive] }); // ExtractToTemp calls this and checks if the success percentage is 1.
            var result = archive.ExtractContents(expectedSettings);

            Assert.AreEqual(report, result);
        }

        [TestMethod]
        public void ExtractContentsTest_ArchiveNotOnDisk2()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = TestFileInfo,
                AssociatedArchive = mockParentArchive.Object
            };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            DPExtractionReport? report = null;
            var expectedSettings = new DPExtractSettings("A:/temp", [file1, file2], true, archive);
            
            MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings settings) =>
            {
                return report = new DPExtractionReport() { ExtractedFiles = [file1, file2], Settings = settings };
            });
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ExtractedFiles = [archive] }); // Note: the actual content of ExtractedFiles doesn't matter,
                                                                                                // as long as the success percentage is 1.
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(false);

            var result = archive.ExtractContents(expectedSettings);

            Assert.AreEqual(report, result);
        }

        [TestMethod]
        public void ExtractContentsTest_ArchiveNotOnDisk2_Fail()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = TestFileInfo,
                AssociatedArchive = mockParentArchive.Object
            };
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ErroredFiles = { [Mock.Of<IDPFile>()] = "error" } });
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(false);

            Assert.ThrowsException<IOException>(() => archive.ExtractContents(new DPExtractSettings()));
        }

        [TestMethod]
        public void ExtractContentsTest_NoExtractor()
        {
            var archive = new DPArchive(TestFileInfo) { Extractor = null, FileInfo = TestFileInfo };
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(true);

            Assert.ThrowsException<InvalidOperationException>(() => archive.ExtractContents(new DPExtractSettings()));
        }

        [TestMethod]
        public void ExtractContentsTest()
        {
            var archive = new DPArchive(TestFileInfo) { Extractor = Extractor };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            DPExtractionReport? report = null;
            var expectedSettings = new DPExtractSettings("A:/temp", [file1], true, archive);
            
            MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings settings) =>
            {
                return report = new DPExtractionReport() { ExtractedFiles = [file1], Settings = settings };
            });
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(true);

            var result = archive.ExtractAllContents("A:/temp", true);

            Assert.AreEqual(report, result);

            MockExtractor.Verify(x => x.Extract(It.IsAny<DPExtractSettings>()), Times.Once());
        }

        [TestMethod]
        public void ExtractContentsToTempTest()
        {
            var archive = new DPArchive(TestFileInfo) { Extractor = Extractor };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            DPExtractionReport? report = null;
            var expectedSettings = new DPExtractSettings("A:/temp", [file1, file2], true, archive);
            MockExtractor.Setup(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings settings) =>
            {
                return report = new DPExtractionReport() { ExtractedFiles = [file1, file2], Settings = settings };
            });
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(true);

            var result = archive.ExtractContentsToTemp(expectedSettings);

            Assert.AreEqual(report, result);

            MockExtractor.Verify(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>()), Times.Once());
        }

        [TestMethod]
        public void ExtractContentsToTempTest_ArchiveNotOnDisk()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = null,
                AssociatedArchive = mockParentArchive.Object
            };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            DPExtractionReport? report = null;
            var expectedSettings = new DPExtractSettings("A:/temp", [file1, file2], true, archive);
            MockExtractor.Setup(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings settings) =>
            {
                return report = new DPExtractionReport() { ExtractedFiles = [file1, file2], Settings = settings };
            });
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ExtractedFiles = [Mock.Of<IDPFile>()] });

            var result = archive.ExtractContentsToTemp(expectedSettings);

            Assert.IsNotNull(report);
            Assert.AreEqual(report, result);

            MockExtractor.Verify(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>()), Times.Once());
        }

        [TestMethod]
        public void ExtractContentsToTempTest_ArchiveNotOnDisk2()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = TestFileInfo,
                AssociatedArchive = mockParentArchive.Object
            };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            DPExtractionReport? report = null;
            var expectedSettings = new DPExtractSettings("A:/temp", [file1, file2], true, mockParentArchive.Object);
            
            MockExtractor.Setup(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings settings) =>
            {
                return report = new DPExtractionReport() { ExtractedFiles = [file1, file2], Settings = settings };
            });
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ExtractedFiles = [Mock.Of<IDPFile>()] });
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(false);

            var result = archive.ExtractContentsToTemp(expectedSettings);

            Assert.AreEqual(report, result);
        }

        [TestMethod]
        public void ExtractContentsToTempTest_ArchiveNotOnDisk2_Fails()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = TestFileInfo,
                AssociatedArchive = mockParentArchive.Object
            };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            DPExtractionReport? report = null;
            var expectedSettings = new DPExtractSettings("A:/temp", [file1, file2], true, mockParentArchive.Object);

            MockExtractor.Setup(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings settings) =>
            {
                return report = new DPExtractionReport() { ExtractedFiles = [file1, file2], Settings = settings };
            });
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ErroredFiles = { [Mock.Of<IDPFile>()] = "error" } });
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(false);

            Assert.ThrowsException<IOException>(() => archive.ExtractContentsToTemp(expectedSettings));
        }

        [TestMethod]
        public void ExtractContentsToTempTest_NoExtractor()
        {
            var archive = new DPArchive(TestFileInfo) { Extractor = null, FileInfo = TestFileInfo };
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(true);

            Assert.ThrowsException<InvalidOperationException>(() => archive.ExtractContentsToTemp(new DPExtractSettings()));
        }

        [TestMethod]
        public void PeekContentsTest()
        {
            var archive = new DPArchive(TestFileInfo) { Extractor = Extractor, FileInfo = TestFileInfo };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(true);

            archive.PeekContents("A:/temp");

            MockExtractor.Verify(x => x.Peek(archive), Times.Once());
        }

        [TestMethod]
        public void PeekContentsTest_ArchiveNotOnDisk()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = null,
                AssociatedArchive = mockParentArchive.Object
            };
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ExtractedFiles = [Mock.Of<IDPFile>()] });

            archive.PeekContents("A:/temp");

            MockExtractor.Verify(x => x.Peek(archive), Times.Once());
        }

        [TestMethod]
        public void PeekContentsTest_ArchiveNotOnDisk2()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = TestFileInfo,
                AssociatedArchive = mockParentArchive.Object
            };
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ExtractedFiles = [Mock.Of<IDPFile>()] });
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(false);

            archive.PeekContents("A:/temp");

            MockExtractor.Verify(x => x.Peek(archive), Times.Once());
        }

        [TestMethod]
        public void PeekContentsTest_ArchiveNotOnDisk_NullTemp()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = null,
                AssociatedArchive = mockParentArchive.Object
            };
            var expectedSettings = new DPExtractSettings(Path.GetTempPath(), [archive], true, mockParentArchive.Object);
            
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ExtractedFiles = [Mock.Of<IDPFile>()] });

            archive.PeekContents(null);

            MockExtractor.Verify(x => x.Peek(archive), Times.Once());
        }

        [TestMethod]
        public void PeekContentsTest_ArchiveNotOnDisk_Fails()
        {
            var mockParentArchive = new Mock<FakeDPArchive>();
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = null,
                AssociatedArchive = mockParentArchive.Object
            };
            var expectedSettings = new DPExtractSettings("A:/temp", [], true, archive);
            
            mockParentArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>()))
                             .Returns(new DPExtractionReport() { ErroredFiles = { [Mock.Of<IDPFile>()] = "error" } });

            Assert.ThrowsException<IOException>(() => archive.PeekContents("A:/temp"));
        }

        [TestMethod]
        public void PeekContentsTest_NullExtractor()
        {
            var archive = new DPArchive(TestFileInfo) { Extractor = null, FileInfo = TestFileInfo };
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(true);

            Assert.ThrowsException<InvalidOperationException>(() => archive.PeekContents("A:/temp"));
        }

        [TestMethod]
        public void ExtractContentTest()
        {
            var archive = new DPArchive(TestFileInfo) { Extractor = Extractor };
            var file1 = new DPFile("dummy.txt", archive, null);
            var file2 = new DPFile("foo.bar", archive, null);
            DPExtractionReport? report = null;
            var expectedSettings = new DPExtractSettings("A:/temp", [file1], false, archive);
            
            MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>())).Returns((DPExtractSettings settings) =>
            {
                return report = new DPExtractionReport() { ExtractedFiles = [file1], Settings = settings };
            });
            MockDPFileInfo.SetupGet(x => x.Exists).Returns(true);

            Assert.IsTrue(archive.ExtractContent(file1, "A:/temp", false));
        }

        [TestMethod]
        public void ExtractContentTest_NotPartOfArchive()
        {
            var archive = new DPArchive(TestFileInfo)
            {
                Extractor = Extractor,
                FileInfo = null
            };
            var fileNotPartOfArchive = new DPFile("foo.bar", null, null);

            Assert.ThrowsException<ArgumentException>(() => archive.ExtractContent(fileNotPartOfArchive, "A:/temp", false));
        }

        [TestMethod]
        [DataRow(new byte[] { 0x50, 0x4B, 0x57, 0x69, 0x00, 0x00, 0x00, 0x00 }, ArchiveFormat.PKZip)]
        [DataRow(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00 }, ArchiveFormat.RAR)]
        [DataRow(new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00, 0x00 }, ArchiveFormat.RAR)]
        [DataRow(new byte[] { 0x37, 0x7A, 0xBC, 0xAF, 0x00, 0x00, 0x00, 0x00 }, ArchiveFormat.SevenZ)]
        [DataRow(new byte[] { 0xAA, 0xFF, 0xCC, 0xDD, 0x00, 0x00, 0x00, 0x00 }, ArchiveFormat.Unknown)]
        [DataRow(new byte[] { 0x00 }, ArchiveFormat.Unknown)] // Special code for error.
        [DataRow(new byte[] { 0x01 }, ArchiveFormat.Unknown)] // Special code for close streem
        public void DetermineArchiveFormatPreciseTest(byte[] bytes, ArchiveFormat expected)
        {
            var mockStream = new Mock<Stream>();
            Stream stream = bytes.Length == 1 ? mockStream.Object : new MemoryStream(bytes);
            mockStream.Setup(s => s.CanRead).Returns(true);
            mockStream.Setup(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()))
                      .Returns((byte[] buffer, int offset, int count) =>
                      {
                          if (bytes[0] == 0) // Special code for error.
                              throw new Exception("expected exception");
                          int bytesToCopy = Math.Min(buffer.Length, 0);
                          bytes.AsSpan(0, bytesToCopy).CopyTo(buffer);
                          return bytesToCopy;
                      });

            Assert.AreEqual(expected, DPArchive.DetermineArchiveFormatPrecise(stream, bytes[0] == 0x01));

            if (bytes[0] == 0x01)
            { // Special code for close stream
                mockStream.Verify(x => x.Close(), Times.AtLeastOnce());
            }
        }

        [TestMethod]
        [DataRow("test.rar", "", true)]
        [DataRow("test.part1.rar", "", true)]
        [DataRow("test.part2.rar", "", false)]
        [DataRow("test.zip", "", true)]
        [DataRow("test.7z", "", true)]
        [DataRow("test.001", "", true)]
        [DataRow("test.somerar4", nameof(RAR4HeaderPadded), true)]
        [DataRow("test.somerar5", nameof(RAR5Header), true)]
        [DataRow("test.somezip", nameof(ZipHeaderPadded), true)]
        [DataRow("test.some7z", nameof(SevenZHeaderPadded), true)]
        [DataRow("test.some001", nameof(SevenZHeaderPadded), true)]
        public void IsValidSupportedArchiveTest(string fileName, string nameofHeader, bool expected)
        {
            var header = GetHeaderData(nameofHeader);
            if (!string.IsNullOrEmpty(nameofHeader))
                MockDPFileInfo.Setup(x => x.OpenRead()).Returns(new MemoryStream(header.ToArray()));
            MockDPFileInfo.SetupGet(x => x.Name).Returns(fileName);

            Assert.AreEqual(expected, DPArchive.IsValidSupportedArchive(TestFileInfo));

            if (!string.IsNullOrEmpty(nameofHeader))
                MockDPFileInfo.Verify(x => x.OpenRead(), Times.Once());
        }

        [TestMethod]
        public void DetermineArchiveTypeTest_Product()
        {
            var archive = new DPArchive();
            _ = new DPFolder("Content", archive, null) { IsContentFolder = true };

            Assert.AreEqual(ArchiveType.Product, archive.DetermineArchiveType());
        }

        [TestMethod]
        public void DetermineArchiveTypeTest_Bundle()
        {
            var archive = new DPArchive();
            _ = new DPFolder("Content", archive, null);
            var nestedArchive = Mock.Of<IDPArchive>();
            archive.Contents.Add("doesnt matter", nestedArchive);

            Assert.AreEqual(ArchiveType.Bundle, archive.DetermineArchiveType());
        }

        [TestMethod]
        public void DetermineArchiveTypeTest_Unknown()
        {
            var archive = new DPArchive();
            _ = new DPFolder("Content", archive, null);
            var file = Mock.Of<IDPFile>();
            archive.Contents.Add("doesnt matter", file);

            Assert.AreEqual(ArchiveType.Unknown, archive.DetermineArchiveType());
        }

        [TestMethod]
        public void GetEstimateTagCountTest()
        {
            var archive = new DPArchive();
            _ = new DPFile("file1", archive, null) { Tags = ["John", "Doe"] };
            _ = new DPFile("file2", archive, null) { Tags = ["Jane", "Doe"] };
            archive.ProductInfo.Authors.Add("Janet");

            Assert.AreEqual(5, archive.GetEstimateTagCount());
        }

        [TestMethod]
        public void FindParentTest_DirectParentOfFile()
        {
            // Arrange
            var archive = new DPArchive();
            var rootFolder = new DPFolder("Content", archive, null);
            var subFolder1 = new DPFolder("Content/Folder1", archive, rootFolder);
            var file1 = new DPFile("Content/Folder1/file1.txt", archive, subFolder1);

            var result = archive.FindParent(file1);

            Assert.AreEqual(subFolder1, result);
        }

        [TestMethod]
        public void FindParentTest_DirectParentOfFolder()
        {
            // Arrange
            var archive = new DPArchive();
            var rootFolder = new DPFolder("Content", archive, null);
            var subFolder1 = new DPFolder("Content/Folder1", archive, rootFolder);

            var result = archive.FindParent(subFolder1);

            Assert.AreEqual(rootFolder, result);
        }

        [TestMethod]
        public void FindParentTest_ParentOfFolder_TrailingSlash()
        {
            // Arrange
            var archive = new DPArchive();
            var rootFolder = new DPFolder("Content", archive, null);
            var subFolder1 = new DPFolder("Content/Folder1/", archive, rootFolder);

            var result = archive.FindParent(subFolder1);

            Assert.AreEqual(rootFolder, result);
        }

        [TestMethod]
        public void FindParentTest_NonexistantParent()
        {
            // Arrange
            var archive = new DPArchive();
            var rootFolder = new DPFolder("Content", archive, null);
            var mockNonexistantFolder = new Mock<IDPFolder>();
            mockNonexistantFolder.SetupGet(x => x.Path).Returns("Nonexistant");

            var result = archive.FindParent(mockNonexistantFolder.Object);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void FindParentTest_EmptyPath()
        {
            // Arrange
            var archive = new DPArchive();
            var rootFolder = new DPFolder("Content", archive, null);
            var mockNonexistantFolder = new Mock<IDPFolder>();
            mockNonexistantFolder.SetupGet(x => x.Path).Returns("");

            var result = archive.FindParent(mockNonexistantFolder.Object);

            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public void FolderExistsTest()
        {
            var archive = new DPArchive();
            var rootFolder = new DPFolder("Content", archive, null);

            Assert.IsTrue(archive.FolderExists(rootFolder.NormalizedPath));
        }

        [TestMethod]
        public void FindFolderTest()
        {
            var archive = new DPArchive();
            var rootFolder = new DPFolder("Content", archive, null);
            var childFolder = new DPFolder("Content/test", archive, rootFolder);

            Assert.IsTrue(archive.FindFolder("Content/test", out var result));
            Assert.AreEqual(childFolder, result);
        }

        [TestMethod]
        public void FindFolderTest_FlippedSeperators()
        {
            var archive = new DPArchive();
            var rootFolder = new DPFolder("Content", archive, null);
            var childFolder = new DPFolder("Content/test", archive, rootFolder);

            Assert.IsTrue(archive.FindFolder("Content\\test", out var result));
            Assert.AreEqual(childFolder, result);
        }

        [TestMethod]
        public void FindFolderTest_DoesntExist()
        {
            var archive = new DPArchive();

            Assert.IsFalse(archive.FindFolder("foo\\bar", out var result));
            Assert.IsNull(result);
        }

        [TestMethod]
        [DataRow("Simple Product Name", new[] { "Simple", "Product", "Name" })]
        [DataRow("Complex+Product-Name_With Spaces", new[] { "Complex", "Product", "Name", "With", "Spaces" })]
        [DataRow("Product123", new[] { "Product123" })]
        [DataRow("123Product", new[] { "123Product" })]
        [DataRow("Product_123", new[] { "Product", "123" })]
        [DataRow("Product-123", new[] { "Product", "123" })]
        [DataRow("Product+123", new[] { "Product", "123" })]
        [DataRow("Product 123", new[] { "Product", "123" })]
        [DataRow("", new string[0])]
        [DataRow("   ", new string[0])]
        [DataRow("+_- ", new string[0])]
        [DataRow("A+B-C_D E", new[] { "A", "B", "C", "D", "E" })]
        [DataRow("CamelCaseProduct", new[] { "CamelCaseProduct" })]
        [DataRow("snake_case_product", new[] { "snake", "case", "product" })]
        [DataRow("UPPER_CASE_PRODUCT", new[] { "UPPER", "CASE", "PRODUCT" })]
        [DataRow("Mixed-Case_Product+Name", new[] { "Mixed", "Case", "Product", "Name" })]
        public void RegexSplitNameTest(string name, string[] expected)
        {
            CollectionAssert.AreEqual(expected, DPArchive.RegexSplitName(name).ToArray());
        }

        [TestMethod]
        public void FileNameTest_ParentArchive()
        {
            var archive = new DPArchive() { FileInfo = TestFileInfo };
            MockDPFileInfo.SetupGet(x => x.Name).Returns("Foo");

            Assert.AreEqual("Foo", archive.FileName);
        }

        [TestMethod]
        public void FileNameTest_NestedArchive()
        {
            var archive = new DPArchive() { FileInfo = null, Path = "Bar" };

            Assert.AreEqual("Bar", archive.FileName);
        }

        [TestMethod]
        public void ExtTest_ParentArchive()
        {
            var archive = new DPArchive() { FileInfo = TestFileInfo };
            MockDPFileInfo.SetupGet(x => x.Name).Returns("Bar.Foo");

            Assert.AreEqual("foo", archive.Ext);
        }

        [TestMethod]
        public void ExtTest_NestedArchive()
        {
            var archive = new DPArchive() { FileInfo = null, Path = "Foo.Bar" };

            Assert.AreEqual("bar", archive.Ext);
        }

        [TestMethod]
        public void FileSystemTest()
        {
            var archive = new DPArchive() { FileInfo = TestFileInfo };
            var fs = Mock.Of<AbstractFileSystem>();
            MockDPFileInfo.SetupGet(x => x.FileSystem).Returns(fs);

            Assert.AreEqual(fs, archive.FileSystem);
        }

        [TestMethod]
        public void FileSystemTest_NoFileInfo()
        {
            var archive = new DPArchive();

            Assert.IsInstanceOfType(archive.FileSystem, typeof(DPFileSystem));
            Assert.AreSame(DPFileScopeSettings.None, archive.FileSystem.Scope);
        }
    }
}