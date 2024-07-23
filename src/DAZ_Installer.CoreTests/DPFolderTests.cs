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
    public class DPFolderTests
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
        public void DPFolder_ChildFolder()
        {
            var parentFolder = new DPFolder("Content", Archive, null);

            var folder = new DPFolder("Content/Folder", Archive, parentFolder);

            Assert.AreEqual(parentFolder, folder.Parent);

            CollectionAssert.Contains(Archive.Folders.Values, folder);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder);
        }

        [TestMethod]
        public void DPFolder_RootFolderTest()
        {
            var folder = new DPFolder("Content", Archive, null);

            Assert.IsNull(folder.Parent);
            CollectionAssert.Contains(Archive.Folders.Values, folder);
            CollectionAssert.Contains(Archive.RootFolders, folder);

        }

        [TestMethod]
        [DataRow("Content")]
        [DataRow("Content/")]
        [DataRow("Content/Folder")]
        public void DPFolder_SetsPathCorrectly(string path)
        {
            var folder = new DPFolder(path, Archive, null);

            Assert.AreEqual(PathHelper.CleanDirPath(path), folder.Path);
        }


        [TestMethod]
        public void DPFolder_CreatesMissingParents()
        {
            var childFolder = new DPFolder("Content/data/TheRealSolly", Archive, null);

            Assert.IsNotNull(childFolder.Parent);
            Assert.IsNotNull(childFolder.Parent.Parent);
        }

        [TestMethod]
        public void CreateFoldersForFileTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);

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
        public void CreateFoldersForFile_RootExistsTest()
        {
            var rootFolder = new DPFolder("Content", Archive, null);
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);

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

            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);

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

        [TestMethod]
        public void CreateFoldersForFile_ParentFolderExistsTest()
        {
            var parentFolder = new DPFolder("Content/data/TheRealSolly", Archive, null);

            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);

            Assert.AreEqual(folder, parentFolder);
            // Assert that the folder returned is the directory of a.txt
            Assert.AreEqual(PathHelper.CleanDirPath("Content/data/TheRealSolly"), folder.Path);
            // Assert that all of the child directories are made.
            Assert.AreEqual(PathHelper.CleanDirPath("Content/data"), folder.Parent!.Path);
            Assert.IsNotNull(folder.Parent.Parent);
            Assert.AreEqual(PathHelper.CleanDirPath("Content"), folder.Parent.Parent.Path);

            // Assert that all of the child directories can be accessed are part of the Archive.
            CollectionAssert.AreEquivalent(new[] { folder, folder.Parent, folder.Parent.Parent }, Archive.Folders.Values);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder);

            // Assert that the subfolders are all correct.
            CollectionAssert.AreEqual(new[] { folder }, folder.Parent.Subfolders);
            CollectionAssert.AreEqual(new[] { folder.Parent }, folder.Parent.Parent.Subfolders);

            Assert.AreEqual(Archive.Folders[PathHelper.NormalizePath("Content/data")], folder.Parent);
            Assert.AreEqual(Archive.Folders["Content"], folder.Parent.Parent);
        }

        [TestMethod]
        public void UpdateChildrenRelativePathsTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);
            var settings = new DPProcessSettings("O:/", "O:/My Library", InstallOptions.ManifestOnly, new HashSet<string>() { "data" }, new Dictionary<string, string>(), false);
            folder.Parent!.IsContentFolder = true;

            folder.UpdateChildrenRelativePaths(settings);

            Assert.AreEqual(PathHelper.NormalizePath("data/TheRealSolly/a.txt"), file.RelativeTargetPath);
            Assert.AreEqual(PathHelper.NormalizePath("data/TheRealSolly/a.txt"), file.RelativePathToContentFolder);
        }

        [TestMethod]
        public void UpdateChildrenRelativePaths_NoContentFolderTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);
            var settings = new DPProcessSettings("O:/",
                                                 "O:/My Library",
                                                 InstallOptions.ManifestOnly,
                                                 new HashSet<string>() { "data" },
                                                 new Dictionary<string, string>(),
                                                 false);

            folder.UpdateChildrenRelativePaths(settings);

            Assert.AreEqual(string.Empty, file.RelativeTargetPath);
            Assert.AreEqual(string.Empty, file.RelativePathToContentFolder);
        }

        [TestMethod]
        public void UpdateChildrenRelativePaths_ContentFolderRedirect()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);
            var settings = new DPProcessSettings("O:/",
                                                 "O:/My Library",
                                                 InstallOptions.ManifestOnly,
                                                 new HashSet<string>() { "data" },
                                                 new Dictionary<string, string> { { "data", "redirect dir" } },
                                                 false);
            folder.Parent!.IsContentFolder = true;

            folder.UpdateChildrenRelativePaths(settings);

            Assert.AreEqual(PathHelper.NormalizePath("redirect dir/TheRealSolly/a.txt"), file.RelativeTargetPath);
            Assert.AreEqual(PathHelper.NormalizePath("data/TheRealSolly/a.txt"), file.RelativePathToContentFolder);
        }

        [TestMethod]
        public void CalculateChildRelativePathTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);

            var result = folder.CalculateChildRelativePath(file);

            Assert.AreEqual(PathHelper.GetRelativePathOfRelativeParent(file.Path, folder.Path), result);
        }

        [TestMethod]
        public void CalculateChildRelativeTargetPathTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);
            var settings = new DPProcessSettings("O:/",
                                                 "O:/My Library",
                                                 InstallOptions.ManifestOnly,
                                                 new HashSet<string>() { "data" },
                                                 new Dictionary<string, string> { },
                                                 false);
            folder.Parent!.IsContentFolder = true;
            file.RelativePathToContentFolder = "data/TheRealSolly/a.txt";

            var result = folder.Parent.CalculateChildRelativeTargetPath(file, settings);

            Assert.AreEqual(PathHelper.NormalizePath("data/TheRealSolly/a.txt"), result);
        }

        [TestMethod]
        public void CalculateChildRelativeTargetPath_ContentRedirectTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);
            var settings = new DPProcessSettings("O:/",
                                                 "O:/My Library",
                                                 InstallOptions.ManifestOnly,
                                                 new HashSet<string>() { "data" },
                                                 new Dictionary<string, string> { { "data", "redirect dir" } },
                                                 false);
            folder.Parent!.IsContentFolder = true;

            var result = folder.Parent.CalculateChildRelativeTargetPath(file, settings);

            Assert.AreEqual(PathHelper.NormalizePath("redirect dir/TheRealSolly/a.txt"), result);
        }

        [TestMethod]
        public void CalculateChildRelativeTargetPath_NotContentFolderTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);
            var settings = new DPProcessSettings("O:/",
                                                 "O:/My Library",
                                                 InstallOptions.ManifestOnly,
                                                 new HashSet<string>() { "data" },
                                                 new Dictionary<string, string> { },
                                                 false);

            var result = folder.CalculateChildRelativeTargetPath(file, settings);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void CalculateChildRelativeTargetPath_AlreadyCalculatedNotContentFolderTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);
            var settings = new DPProcessSettings("O:/",
                                                 "O:/My Library",
                                                 InstallOptions.ManifestOnly,
                                                 new HashSet<string>() { "data" },
                                                 new Dictionary<string, string> { { "data", "redirect dir" } },
                                                 false);
            file.RelativePathToContentFolder = file.RelativeTargetPath = "test";

            var result = folder.CalculateChildRelativeTargetPath(file, settings);

            Assert.AreEqual("test", result);
        }

        [TestMethod]
        public void CalculateChildRelativeTargetPath_AlreadyCalculatedTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);
            var settings = new DPProcessSettings("O:/",
                                                 "O:/My Library",
                                                 InstallOptions.ManifestOnly,
                                                 new HashSet<string>() { "data" },
                                                 new Dictionary<string, string> { { "data", "redirect dir" } },
                                                 false);
            file.RelativePathToContentFolder = file.RelativeTargetPath = "test";

            var result = folder.CalculateChildRelativeTargetPath(file, settings);

            Assert.AreEqual("test", result);
        }

        [TestMethod]
        public void CalculateChildRelativeTargetPath_NullTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            var file = new DPFile("Content/data/TheRealSolly/a.txt", Archive, folder);
            var settings = new DPProcessSettings("O:/",
                                                 "O:/My Library",
                                                 InstallOptions.ManifestOnly);

            Assert.ThrowsException<ArgumentNullException>(() => folder.CalculateChildRelativeTargetPath(file, settings));
        }

        [TestMethod]
        public void GetContentFolderTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            folder.IsContentFolder = true;

            Assert.AreEqual(folder, folder.GetContentFolder());
        }

        [TestMethod]
        public void GetContentFolder_ParentFolderTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            folder.Parent.IsContentFolder = true;

            Assert.AreEqual(folder.Parent, folder.GetContentFolder());
        }

        [TestMethod]
        public void GetContentFolder_RootFolderTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);
            folder.Parent!.Parent!.IsContentFolder = true;

            Assert.AreEqual(folder.Parent.Parent, folder.GetContentFolder());
        }

        [TestMethod]
        public void GetContentFolder_NoContentFolderTest()
        {
            var folder = DPFolder.CreateFoldersForFile("Content/data/TheRealSolly/a.txt", Archive);

            Assert.IsNull(folder.GetContentFolder());
        }

        [TestMethod]
        public void AddChild_FolderTest()
        {
            var folder = new DPFolder("Content", Archive, null);
            var folder1 = new DPFolder("Content2", Archive, null);

            folder.AddChild(folder1);

            CollectionAssert.Contains(folder.Subfolders, folder1);
        }

        [TestMethod]
        public void AddChild_FileTest()
        {
            var folder = new DPFolder("Content", Archive, null);
            var file = new DPFile("some file", Archive, null);

            folder.AddChild(file);

            CollectionAssert.Contains(folder.Contents.ToList(), file);
        }

        
        [TestMethod]
        public void AddChild_InvalidTypeTest()
        {
            var folder = new DPFolder("Content", Archive, null);
            var mockObject = Mock.Of<DPAbstractNode>();

            Assert.ThrowsException<ArgumentException>(() => folder.AddChild(mockObject));
        }

        [TestMethod]
        public void RemoveChild_FolderTest()
        {
            var parentFolder = new DPFolder("Content", Archive, null);
            var childFolder = new DPFolder("Content/Subfolder", Archive, parentFolder);

            parentFolder.RemoveChild(childFolder);

            CollectionAssert.DoesNotContain(parentFolder.Subfolders, childFolder);
        }

        [TestMethod]
        public void RemoveChild_FileTest()
        {
            var folder = new DPFolder("Content", Archive, null);
            var file = new DPFile("Content/file.txt", Archive, folder);

            folder.RemoveChild(file);

            CollectionAssert.DoesNotContain(folder.Contents.ToList(), file);
        }

        [TestMethod]
        public void RemoveChild_InvalidTypeTest()
        {
            var folder = new DPFolder("Content", Archive, null);
            var mockObject = Mock.Of<DPAbstractNode>();

            Assert.ThrowsException<ArgumentException>(() => folder.RemoveChild(mockObject));
        }

        [TestMethod]
        public void UpdateParent_FromNoParentToParentTest()
        {
            var folder = new DPFolder("Content", Archive, null);
            var parentFolder = new DPFolder("Content2", Archive, null);

            folder.Parent = parentFolder; // This calls UpdateParent

            Assert.AreEqual(parentFolder, folder.Parent);
            CollectionAssert.AreEqual(new[] { folder }, parentFolder.Subfolders);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder);
        }

        [TestMethod]
        public void UpdateParent_FromNoParentToNoParent_FindsParentTest()
        {
            var folder = new DPFolder("Content", Archive, null);
            var parentFolder = new DPFolder("Content2", Archive, null);
            Archive.Folders["Content2/Child"] = Archive.Folders["Content"];
            Archive.Folders.Remove("Content");
            folder.Path = "Content2/Child";

            folder.Parent = null; // This calls UpdateParent

            Assert.AreEqual(parentFolder, folder.Parent);
            CollectionAssert.AreEqual(new[] { folder }, parentFolder.Subfolders);
            CollectionAssert.AreEqual(new[] { parentFolder }, Archive.RootFolders);
            CollectionAssert.AreEquivalent(new[] { folder, parentFolder }, Archive.Folders.Values.ToList());
        }

        [TestMethod]
        public void UpdateParent_FromNoParentToNoParent_CreatesParentsTest()
        {
            var folder = new DPFolder("Content", Archive, null);
            Archive.Folders["Content/data/TheRealSolly"] = Archive.Folders["Content"];
            folder.Path = "Content/data/TheRealSolly";
            Archive.Folders.Remove("Content");

            folder.Parent = null; // This calls UpdateParent

            Assert.IsNotNull(folder.Parent);
            Assert.IsNotNull(folder.Parent.Parent);
            Assert.AreEqual(folder.Parent.FileName, "data");
            Assert.AreEqual(folder.Parent.Parent.FileName, "Content");
            CollectionAssert.AreEqual(new[] { folder }, folder.Parent.Subfolders);
            CollectionAssert.AreEqual(new[] { folder.Parent }, folder.Parent.Parent.Subfolders);
            CollectionAssert.DoesNotContain(Archive.RootFolders, folder);
            CollectionAssert.AreEquivalent(new[] { folder, folder.Parent, folder.Parent.Parent }, Archive.Folders.Values);
        }

        [TestMethod]
        public void UpdateParent_FromParentToParent_Test()
        {
            var folder = new DPFolder("Content", Archive, null);
            var parentFolder = new DPFolder("Content2", Archive, null);

            folder.Parent = parentFolder; // This calls UpdateParent

            Assert.AreEqual(parentFolder, folder.Parent);
            CollectionAssert.AreEqual(new[] { folder }, parentFolder.Subfolders);
            CollectionAssert.AreEqual(new[] { parentFolder }, Archive.RootFolders);
            CollectionAssert.AreEquivalent(new[] { folder, parentFolder }, Archive.Folders.Values);
        }

        [TestMethod]
        public void UpdateParent_FromParentToNoParent_Test()
        {
            var folder = new DPFolder("Content/Child", Archive, null);
            folder.Path = "Content";
            var parentFolder = folder.Parent;

            folder.Parent = null; // This calls UpdateParent

            Assert.IsNull(folder.Parent);
            CollectionAssert.IsNotSubsetOf(new[] { folder }, parentFolder!.Subfolders);
            CollectionAssert.AreEquivalent(new[] { folder, parentFolder }, Archive.RootFolders);
            CollectionAssert.AreEquivalent(new[] { folder, parentFolder }, Archive.Folders.Values);
        }

        [TestMethod]
        public void IsPartOfContentFolder_NullParentTest()
        {
            var folder = new DPFolder("Content", Archive, null);

            Assert.IsFalse(folder.IsPartOfContentFolder);
        }

        [TestMethod]
        public void IsPartOfContentFolder_FolderIsContentFolderTest()
        {
            var folder = new DPFolder("Content/doesn't/matter", Archive, null);
            folder.IsContentFolder = true;
            folder.Parent!.Parent!.IsContentFolder = true;

            Assert.IsFalse(folder.IsPartOfContentFolder);
        }

        [TestMethod]
        public void IsPartOfContentFolder_ParentIsContentFolderTest()
        {
            var folder = new DPFolder("Content/doesn't/matter", Archive, null);
            folder.Parent!.IsContentFolder = true;

            Assert.IsTrue(folder.IsPartOfContentFolder);
        }

        [TestMethod]
        public void IsPartOfContentFolder_ParentIsPartOfContentFolderTest()
        {
            var folder = new DPFolder("Content/doesn't/matter", Archive, null);
            folder.Parent!.Parent!.IsContentFolder = true;

            Assert.IsTrue(folder.IsPartOfContentFolder);
        }
    }
}