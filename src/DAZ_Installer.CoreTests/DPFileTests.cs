using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.IO;
using Serilog;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using DAZ_Installer.Core.Extraction;
using Moq;

#pragma warning disable CS0618
namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPFileTests
    {
        public DPAbstractExtractor Extractor = null!;
        public FakeFileSystem FileSystem = null!;
        public FakeDPFileInfo FakeDPFileInfo = null!;
        public DPArchive Archive = null!;

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
            Extractor = Mock.Of<DPAbstractExtractor>();
            FileSystem = new FakeFileSystem(DPFileScopeSettings.None);
            FakeDPFileInfo = FileSystem.CreateFileInfo("test");
            Archive = new DPArchive("test.zip", Log.Logger, FileSystem.CreateFileInfo("test.zip"), Extractor, null, null);
        }


        [TestMethod]
        public void DPFile_RootFileTest()
        {
            var file = new DPFile("file.jpg", Archive, null);

            Assert.IsNull(file.Parent);
            CollectionAssert.AreEqual(Array.Empty<DPFolder>(), Archive.Folders.Values);
            CollectionAssert.AreEqual(Array.Empty<DPFolder>(), Archive.RootFolders);
            CollectionAssert.AreEqual(new[] { file }, Archive.Contents.Values);
            CollectionAssert.AreEqual(new[] { file }, Archive.RootContents);

            CollectionAssert.AreEqual(new[] { "file.jpg" }, file.Tags);
            Assert.AreEqual(Archive, file.AssociatedArchive);
        }

        [TestMethod]
        public void DPFile_FileTest()
        {
            var parent = new DPFolder("Content", Archive, null);
            var file = new DPFile("Content/file.jpg", Archive, null);

            Assert.AreEqual(parent, file.Parent);
            CollectionAssert.AreEqual(new[] { parent }, Archive.Folders.Values);
            CollectionAssert.AreEqual(new[] { parent }, Archive.RootFolders);
            CollectionAssert.AreEqual(new[] { file }, Archive.Contents.Values);
            CollectionAssert.AreEqual(Array.Empty<DPFile>(), Archive.RootContents);

            CollectionAssert.AreEqual(new[] { "file.jpg" }, file.Tags);
            Assert.AreEqual(Archive, file.AssociatedArchive);
        }


        [TestMethod]
        public void DPFile_CreatesMissingParents()
        {
            var file = new DPFile("Content/file.jpg", Archive, null);

            Assert.IsNotNull(file.Parent);
            CollectionAssert.AreEqual(new[] { file.Parent }, Archive.Folders.Values);
            CollectionAssert.AreEqual(new[] { file.Parent }, Archive.RootFolders);
            CollectionAssert.AreEqual(new[] { file }, Archive.Contents.Values);
            CollectionAssert.AreEqual(Array.Empty<DPFile>(), Archive.RootContents);

            CollectionAssert.AreEqual(new[] { "file.jpg" }, file.Tags);
            Assert.AreEqual(Archive, file.AssociatedArchive);
        }

        [TestMethod]
        [DataRow("the quick brown fox jumps over the lazy dog.jpg")]
        [DataRow("a_b_c.jpg")]
        [DataRow("Content/abcdefg.jpg")]
        [DataRow("Content/data/TheRealSolly/a-b-c-d.jpg")]
        public void DPFile_CorrectTags(string path)
        {
            var file = new DPFile(path, Archive, null);
            CollectionAssert.AreEquivalent(path.Split('/')[^1].Split(' '), file.Tags);
        }

        [TestMethod]
        public void CreateNewFile_DPDazFileTest()
        {
            var file = DPFile.CreateNewFile("test.duf", Archive, null);
            Assert.IsInstanceOfType(file, typeof(DPDazFile));

            file = DPFile.CreateNewFile("test.dsf", Archive, null);
            Assert.IsInstanceOfType(file, typeof(DPDazFile));
        }

        [TestMethod]
        public void CreateNewFile_DPDSXFileTest()
        {
            var file = DPFile.CreateNewFile("test.dsx", Archive, null);
            Assert.IsInstanceOfType(file, typeof(DPDSXFile));
        }

        [TestMethod]
        public void CreateNewFile_DPArchiveTest()
        {
            foreach (var ext in DPFile.AcceptableImportFormats)
            {
                var file = DPFile.CreateNewFile($"test.{ext}", Archive, null);
                Assert.IsInstanceOfType(file, typeof(DPArchive));
            }
        }

        [TestMethod]
        public void CreateNewFile_UnknownFileTest()
        {
            var file = DPFile.CreateNewFile("test.33t3", Archive, null);
            Assert.IsInstanceOfType(file, typeof(DPFile));
        }

        [TestMethod]
        public void MoveToTest()
        {
            var mockFileInfo = new Mock<IDPFileInfo>();
            var file = new DPFile("Content/file.jpg", Archive, null, mockFileInfo.Object, Log.Logger);

            file.MoveTo("Content/file2.jpg");

            mockFileInfo.Verify(x => x.MoveTo("Content/file2.jpg", true), Times.Once);
        }

        [TestMethod]
        public void DeleteTest()
        {
            var mockFileInfo = new Mock<IDPFileInfo>();
            var file = new DPFile("Content/file.jpg", Archive, null, mockFileInfo.Object, Log.Logger);

            file.Delete();

            mockFileInfo.Verify(x => x.Delete(), Times.Once);
        }

        [TestMethod]
        public void ExtractTest()
        {
            var mockExtractor = Mock.Get(Extractor);
            mockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>()))
                         .Returns(new DPExtractionReport() { ExtractedFiles = new List<DPFile>() { new() } });

            var file = new DPFile("Content/file.jpg", Archive, null);

            var result = file.Extract(new DPExtractSettings());

            mockExtractor.Verify(x => x.Extract(It.IsAny<DPExtractSettings>()), Times.Once);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Extract_NullArchiveTest()
        {
            var file = new DPFile("Content/file.jpg", null, null);

            Assert.IsFalse(file.Extract(new DPExtractSettings()));
        }

        [TestMethod]
        public void ExtractTest1()
        {
            var mockExtractor = Mock.Get(Extractor);
            mockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>()))
                         .Returns(new DPExtractionReport() { ExtractedFiles = new List<DPFile>() { new() } });

            var file = new DPFile("Content/file.jpg", Archive, null);

            var result = file.Extract(new DPExtractSettings(), "test dest");

            mockExtractor.Verify(x => x.Extract(It.IsAny<DPExtractSettings>()), Times.Once);
            Assert.IsTrue(result);
            Assert.AreEqual("test dest", file.TargetPath);
        }

        [TestMethod]
        public void ExtractToTempTest()
        {
            var mockExtractor = Mock.Get(Extractor);
            mockExtractor.Setup(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>()))
                         .Returns(new DPExtractionReport() { ExtractedFiles = new List<DPFile>() { new() } });

            var file = new DPFile("Content/file.jpg", Archive, null);

            var result = file.ExtractToTemp(new DPExtractSettings());

            mockExtractor.Verify(x => x.ExtractToTemp(It.IsAny<DPExtractSettings>()), Times.Once);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ExtractToTemp_NullArchiveTest()
        {
            var file = new DPFile("Content/file.jpg", null, null);

            Assert.IsFalse(file.ExtractToTemp(new DPExtractSettings()));
        }

        [TestMethod]
        public void GetContentType_ContentTypeTest()
        {
            foreach (var eName in Enum.GetNames(typeof(ContentType)))
            {
                var lowercasedName = eName.ToLower();
                var expected = Enum.Parse<ContentType>(eName);
                Assert.AreEqual(expected, DPFile.GetContentType(lowercasedName, new DPFile()));
            }
        }

        [TestMethod]
        public void GetContentType_NullFileTest()
        {
            Assert.AreEqual(ContentType.DAZ_File, DPFile.GetContentType(string.Empty, null));
        }

        [TestMethod]
        public void GetContentType_GeometryTest()
        {
            foreach (var ext in DPFile.GeometryFormats)
            {
                var file = new DPFile("test." + ext, Archive, null);
                Assert.AreEqual(ContentType.Geometry, DPFile.GetContentType(string.Empty, file));
            }
        }

        [TestMethod]
        public void GetContentType_MediaTest()
        {
            foreach (var ext in DPFile.MediaFormats)
            {
                var file = new DPFile("test." + ext, Archive, null);
                Assert.AreEqual(ContentType.Media, DPFile.GetContentType(string.Empty, file));
            }
        }

        [TestMethod]
        public void GetContentType_DocumentTest()
        {
            foreach (var ext in DPFile.DocumentFormats)
            {
                var file = new DPFile("test." + ext, Archive, null);
                Assert.AreEqual(ContentType.Document, DPFile.GetContentType(string.Empty, file));
            }
        }

        [TestMethod]
        public void GetContentType_ProgramTest()
        {
            foreach (var ext in DPFile.OtherFormats)
            {
                var file = new DPFile("test." + ext, Archive, null);
                Assert.AreEqual(ContentType.Program, DPFile.GetContentType(string.Empty, file));
            }
        }

        [TestMethod]
        public void GetContentType_DAZTest()
        {
            foreach (var ext in DPFile.DAZFormats)
            {
                var file = new DPFile("test." + ext, Archive, null);
                Assert.AreEqual(ContentType.DAZ_File, DPFile.GetContentType(string.Empty, file));
            }
        }

        [TestMethod]
        public void GetContentType_UnknownTest()
        {
            var file = new DPFile("test.test", Archive, null);
            Assert.AreEqual(ContentType.Unknown, DPFile.GetContentType(string.Empty, file));
        }

        [TestMethod]
        public void ValidImportExtensionTest()
        {
            foreach (var ext in DPFile.AcceptableImportFormats)
            {
                Assert.IsTrue(DPFile.ValidImportExtension(ext));
            }
            Assert.IsFalse(DPFile.ValidImportExtension("test"));
        }

        [TestMethod]
        public void UpdateParent_FromNoParentToParentTest()
        {
            var file = new DPFile("file.jpg", Archive, null);
            var parentFolder = new DPFolder("Content2", Archive, null);
            file.Path = "Content2/file.jpg";

            file.Parent = parentFolder; // This calls UpdateParent

            Assert.AreEqual(parentFolder, file.Parent);
            CollectionAssert.AreEqual(new[] { file }, parentFolder.Contents.ToList());
            CollectionAssert.AreEqual(new[] { file }, Archive.Contents.Values);
            Assert.AreEqual(0, Archive.RootContents.Count);
        }

        [TestMethod]
        public void UpdateParent_FromNoParentToNoParent_FindsParentTest()
        {
            var file = new DPFile("file.jpg", Archive, null);
            var parentFolder = new DPFolder("Content2", Archive, null);
            Archive.Contents["Content2/file.jpg"] = Archive.Contents["file.jpg"];
            Archive.Contents.Remove("file.jpg");
            file.Path = "Content2/file.jpg";

            file.Parent = null; // This calls UpdateParent

            Assert.AreEqual(parentFolder, file.Parent);
            CollectionAssert.AreEqual(new[] { file }, parentFolder.Contents.ToList());
            CollectionAssert.AreEqual(new[] { file }, Archive.Contents.Values);
            Assert.AreEqual(0, Archive.RootContents.Count);
            CollectionAssert.AreEqual(new[] { parentFolder }, Archive.RootFolders);
            CollectionAssert.AreEquivalent(new[] { parentFolder }, Archive.Folders.Values.ToList());
        }

        [TestMethod]
        public void UpdateParent_FromNoParentToNoParent_CreatesParentsTest()
        {
            var file = new DPFile("file.jpg", Archive, null);
            Archive.Contents["Content/data/file.jpg"] = Archive.Contents["file.jpg"];
            Archive.Contents.Remove("file.jpg");
            file.Path = "Content/data/file.jpg";

            file.Parent = null; // This calls UpdateParent

            Assert.IsNotNull(file.Parent);
            Assert.IsNotNull(file.Parent.Parent);
            Assert.AreEqual(PathHelper.CleanDirPath("Content/data"), file.Parent.Path);
            Assert.AreEqual("Content", file.Parent.Parent.Path);

            CollectionAssert.AreEqual(new[] { file }, file.Parent.Contents.ToList());
            CollectionAssert.AreEqual(new[] { file }, Archive.Contents.Values);
            Assert.AreEqual(0, Archive.RootContents.Count);
            CollectionAssert.AreEqual(new[] { file.Parent, }, file.Parent.Parent.Subfolders);
            CollectionAssert.AreEquivalent(new[] { file.Parent, file.Parent.Parent }, Archive.Folders.Values);
        }

        [TestMethod]
        public void UpdateParent_FromParentToParent_Test()
        {
            var folder = new DPFolder("Content", Archive, null);
            var file = new DPFile("Content/file.jpg", Archive, null);
            var parentFolder = new DPFolder("Content2", Archive, null);
            file.Path = "Content2/file.jpg";
            Archive.Contents["Content2/file.jpg"] = Archive.Contents["Content/file.jpg"];
            Archive.Contents.Remove("Content/file.jpg");

            file.Parent = parentFolder; // This calls UpdateParent

            Assert.AreEqual(parentFolder, file.Parent);
            CollectionAssert.AreEqual(new[] { file }, parentFolder.Contents.ToList());
            CollectionAssert.AreEqual(new[] { file }, Archive.Contents.Values);
            Assert.AreEqual(0, folder.Contents.Count);
            Assert.AreEqual(0, Archive.RootContents.Count);
        }

        [TestMethod]
        public void UpdateParent_FromParentToNoParent_Test()
        {
            var file = new DPFile("Content/file.jpg", Archive, null);
            var parentFolder = file.Parent!;
            file.Path = "file.jpg";
            Archive.Contents["file.jpg"] = Archive.Contents["Content/file.jpg"];
            Archive.Contents.Remove("Content/file.jpg");

            file.Parent = null;

            Assert.IsNull(file.Parent);
            CollectionAssert.AreEqual(new[] { file }, Archive.RootContents);
            CollectionAssert.AreEqual(new[] { file }, Archive.Contents.Values);
            Assert.AreEqual(0, parentFolder.Contents.Count);
            CollectionAssert.AreEqual(new[] { parentFolder }, Archive.Folders.Values);
            CollectionAssert.AreEqual(new[] { parentFolder }, Archive.RootFolders);
        }
    }
}