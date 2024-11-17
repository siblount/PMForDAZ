using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Serilog;
using DAZ_Installer.Core.Extraction;
using Moq;
using DAZ_Installer.Core.Tests.Fakes;

#pragma warning disable CS0618 // Disable Obsolete warning as intended for testing purposes.

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPFolderFactoryTests
    {
        public FakeDPArchive Archive = null!;
        public FakeFileSystem FileSystem = new();
        public DPAbstractExtractor Extractor = Mock.Of<DPAbstractExtractor>();

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
            FileSystem = new FakeFileSystem();
            Archive = new FakeDPArchive();
        }

        [TestMethod]
        public void CreateFolderTest()
        {
            var fakeFactory = Mock.Of<IDPFolderFactory>();
            Archive.FolderFactory = fakeFactory;
            var folder = DPFolderFactory.Instance.CreateFolder("Content", Archive, null);

            Assert.IsNotNull(folder);
            Assert.AreEqual("Content", folder.Path);
            Assert.AreEqual("Content", folder.NormalizedPath);
            Assert.IsNull(folder.Parent);
            Assert.AreEqual(Archive, folder.AssociatedArchive);
            Assert.AreEqual(fakeFactory, folder.FolderFactory);
        }

        [TestMethod]
        public void CreateFoldersTest()
        {
            var folder = DPFolderFactory.Instance.CreateFolders("Content/data/TheRealSolly/a.txt", Archive);

            // Assert that the folder returned is the directory of a.txt
            Assert.AreEqual(PathHelper.CleanDirPath("Content/data/TheRealSolly"), folder.Path);
            // Assert that all of the child directories are made.
            Assert.IsNotNull(folder.Parent);
            Assert.AreEqual(PathHelper.CleanDirPath("Content/data"), folder.Parent.Path);
            Assert.IsNotNull(folder.Parent.Parent);
            Assert.AreEqual(PathHelper.CleanDirPath("Content"), folder.Parent.Parent.Path);
            // Assert that all of the child directories can be accessed are part of the Archive.
            CollectionAssert.Contains(Archive.Folders.Values, folder);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder);
            CollectionAssert.Contains(Archive.Folders.Values, folder.Parent);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder.Parent);
            CollectionAssert.Contains(Archive.Folders.Values, folder.Parent.Parent);
            CollectionAssert.Contains(Archive.RootFolders, folder.Parent.Parent);

            Assert.AreEqual(Archive.Folders[PathHelper.NormalizePath("Content/data/TheRealSolly")], folder);
            Assert.AreEqual(Archive.Folders[PathHelper.NormalizePath("Content/data")], folder.Parent);
            Assert.AreEqual(Archive.Folders["Content"], folder.Parent.Parent);
        }

        [TestMethod]
        public void CreateFolder_RootExistsTest()
        {
            var rootFolder = new DPFolder("Content", Archive, null);
            var folder = DPFolderFactory.Instance.CreateFolders("Content/data/TheRealSolly/a.txt", Archive);

            // Assert that the folder returned is the directory of a.txt
            Assert.AreEqual(PathHelper.CleanDirPath("Content/data/TheRealSolly"), folder.Path);
            // Assert that all of the child directories are made.
            Assert.IsNotNull(folder.Parent);
            Assert.AreEqual(PathHelper.CleanDirPath("Content/data"), folder.Parent.Path);
            Assert.IsNotNull(folder.Parent.Parent);
            Assert.AreEqual(PathHelper.CleanDirPath("Content"), folder.Parent.Parent.Path);

            // Assert that the root folder refers to the same rootFolder object.
            Assert.AreEqual(rootFolder, folder.Parent.Parent);
            // Assert that all of the child directories can be accessed are part of the Archive.
            CollectionAssert.Contains(Archive.Folders.Values, folder);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder);
            CollectionAssert.Contains(Archive.Folders.Values, folder.Parent);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder.Parent);
            CollectionAssert.Contains(Archive.Folders.Values, folder.Parent.Parent);
            CollectionAssert.Contains(Archive.RootFolders, folder.Parent.Parent);
            Assert.AreEqual(1, Archive.RootFolders.Count);

            Assert.AreEqual(Archive.Folders[PathHelper.NormalizePath("Content/data/TheRealSolly")], folder);
            Assert.AreEqual(Archive.Folders[PathHelper.NormalizePath("Content/data")], folder.Parent);
            Assert.AreEqual(Archive.Folders["Content"], folder.Parent.Parent);
        }

        [TestMethod]
        public void CreateFoldersForFile_MidFolderExistsTest()
        {
            var midFolder = new DPFolder("Content/data", Archive, null);

            var folder = DPFolderFactory.Instance.CreateFolders("Content/data/TheRealSolly/a.txt", Archive);

            Assert.IsNotNull(folder);
            // Assert that the folder returned is the directory of a.txt
            Assert.AreEqual(PathHelper.CleanDirPath("Content/data/TheRealSolly"), folder.Path);
            // Assert that all of the child directories are made.
            Assert.AreEqual(midFolder, folder.Parent);
            Assert.AreEqual(PathHelper.CleanDirPath("Content/data"), folder.Parent!.Path);
            Assert.IsNotNull(folder.Parent.Parent);
            Assert.AreEqual(PathHelper.CleanDirPath("Content"), folder.Parent.Parent.Path);
            // Assert that the mid folder refers to the same rootFolder object.
            Assert.AreEqual(midFolder, folder.Parent);
            // Assert that all of the child directories can be accessed are part of the Archive.
            CollectionAssert.Contains(Archive.Folders.Values, folder);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder);
            CollectionAssert.Contains(Archive.Folders.Values, folder.Parent);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder.Parent);
            CollectionAssert.Contains(Archive.Folders.Values, folder.Parent.Parent);
            CollectionAssert.Contains(Archive.RootFolders, folder.Parent.Parent);

            Assert.AreEqual(Archive.Folders[PathHelper.NormalizePath("Content/data/TheRealSolly")], folder);
            Assert.AreEqual(Archive.Folders[PathHelper.NormalizePath("Content/data")], folder.Parent);
            Assert.AreEqual(Archive.Folders["Content"], folder.Parent.Parent);
        }
    }
}