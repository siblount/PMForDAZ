using DAZ_Installer.IO.Fakes;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS0618
namespace DAZ_Installer.IO.Integration.Tests
{
    [TestClass]
    public class DPRecycleBinTests
    {
        private static DPFileInfo existingFile = null!;
        private static string initExistingFilePath = null!;
        private static string tempDir = Path.Combine(Path.GetTempPath(), "DAZ_Installer.IO.Integration.Tests.DPFileInfoTests");
        private static DPFileSystem defaultFS = new(new DPFileScopeSettings(Array.Empty<string>(), new[] { tempDir }, false));
        private static string initExistingDirPath = null!;
        private static DPDirectoryInfo existingDir = null!;


        [ClassInitialize]
        public static void ClassSetup(TestContext t)
        {
            Directory.CreateDirectory(tempDir);
            initExistingFilePath = Path.Combine(tempDir, "exist.txt");
            initExistingDirPath = Path.Combine(tempDir, "Test");
        }

        [TestInitialize]
        public void TestInitialize()
        {
            existingFile = new DPFileInfo(initExistingFilePath, defaultFS);
            existingFile.TryCreate(out var stream);
            stream?.Dispose();
            existingDir = new DPDirectoryInfo(initExistingDirPath, defaultFS);
            existingDir.Create();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            existingFile.TryDelete();
        }

        [TestMethod]
        public void SendToRecycleBin_FilePathTest()
        {
            if (!File.Exists(existingFile.Path)) 
                Assert.Inconclusive("Existing file did not exist");
            var result = DPRecycleBin.SendToRecycleBin(existingFile.Path);

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(existingFile.Path));
        }

        [TestMethod]
        public void SendToRecycleBin_DirectoryPathTest()
        {
            if (!Directory.Exists(existingDir.Path))
                Assert.Inconclusive("Existing directory did not exist");
            existingFile.MoveTo(Path.Combine(existingDir.Path, "exist.txt"), true);

            if (!existingFile.Exists) Assert.Inconclusive();

            var result = DPRecycleBin.SendToRecycleBin(existingDir.Path);

            Assert.IsTrue(result);
            Assert.IsFalse(Directory.Exists(existingDir.Path));
            Assert.IsFalse(File.Exists(existingFile.Path));
        }

        [TestMethod]
        public void SendToRecycleBin_FilePathsTest()
        {
            if (!File.Exists(existingFile.Path))
                Assert.Inconclusive("Existing file did not exist");
            var file2Path = Path.Combine(tempDir, "exist2.txt");
            File.Create(file2Path).Dispose();

            var result = DPRecycleBin.SendToRecycleBin(new[] { existingFile.Path, file2Path });

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(existingFile.Path));
            Assert.IsFalse(File.Exists(file2Path));
        }

        [TestMethod]
        public void SendToRecycleBin_DirsPathsTest()
        {
            if (!File.Exists(existingFile.Path))
                Assert.Inconclusive("Existing file did not exist");
            if (!Directory.Exists(existingDir.Path))
                Assert.Inconclusive("Existing directory did not exist");
            existingFile.MoveTo(Path.Combine(existingDir.Path, "exist.txt"), true);

            var dir = new DirectoryInfo(Path.Combine(existingDir.Path, "Test2"));
            dir.Create();
            var file2Path = Path.Combine(dir.FullName, "exist2.txt");
            File.Create(file2Path).Dispose();

            var result = DPRecycleBin.SendToRecycleBin(new[] { dir.FullName, existingDir.Path });

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(existingFile.Path));
            Assert.IsFalse(File.Exists(file2Path));
            Assert.IsFalse(Directory.Exists(existingDir.Path));
            Assert.IsFalse(Directory.Exists(dir.FullName));
        }

        [TestMethod]
        public void SendToRecycleBin_MixedPathsTest()
        {
            if (!File.Exists(existingFile.Path))
                Assert.Inconclusive("Existing file did not exist");
            if (!Directory.Exists(existingDir.Path))
                Assert.Inconclusive("Existing directory did not exist");

            var dir = new DirectoryInfo(Path.Combine(existingDir.Path, "Test2"));
            dir.Create();
            var file2Path = Path.Combine(dir.FullName, "exist2.txt");
            File.Create(file2Path).Dispose();

            var result = DPRecycleBin.SendToRecycleBin(new[] { existingFile.Path, dir.FullName, existingDir.Path });

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(existingFile.Path));
            Assert.IsFalse(File.Exists(file2Path));
            Assert.IsFalse(Directory.Exists(existingDir.Path));
            Assert.IsFalse(Directory.Exists(dir.FullName));
        }

        [TestMethod]
        public void SendToRecycleBin_DPFileInfoTest()
        {
            if (!File.Exists(existingFile.Path))
                Assert.Inconclusive("Existing file did not exist");

            var result = DPRecycleBin.SendToRecycleBin(existingFile);

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(existingFile.Path));
        }

        [TestMethod]
        public void SendToRecycleBin_DPDirectoryInfoTest()
        {
            if (!Directory.Exists(existingDir.Path))
                Assert.Inconclusive("Existing dir did not exist");
            existingFile.MoveTo(Path.Combine(existingDir.Path, "exist.txt"), true);


            var result = DPRecycleBin.SendToRecycleBin(existingDir);

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(existingFile.Path));
            Assert.IsFalse(Directory.Exists(existingDir.Path));
        }

        [TestMethod]
        public void SendToRecycleBin_DPDirectoryInfosTest()
        {
            if (!Directory.Exists(existingDir.Path))
                Assert.Inconclusive("Existing dir did not exist");
            existingFile.MoveTo(Path.Combine(existingDir.Path, "exist.txt"), true);
            var file2 = defaultFS.CreateFileInfo(Path.Combine(existingDir.Path, "exist2.txt"));
            file2.Create().Dispose();
            var dir = defaultFS.CreateDirectoryInfo(Path.Combine(existingDir.Path, "Test2"));
            dir.Create();

            var result = DPRecycleBin.SendToRecycleBin(existingDir);

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(existingFile.Path));
            Assert.IsFalse(Directory.Exists(existingDir.Path));
            Assert.IsFalse(File.Exists(file2.Path));
            Assert.IsFalse(Directory.Exists(dir.Path));
        }

        [TestMethod]
        public void SendToRecycleBin_DPFileInfosTest()
        {
            if (!File.Exists(existingFile.Path))
                Assert.Inconclusive("Existing dir did not exist");
            existingFile.MoveTo(Path.Combine(existingDir.Path, "exist.txt"), true);
            var file2 = defaultFS.CreateFileInfo(Path.Combine(existingDir.Path, "exist2.txt"));
            file2.Create().Dispose();

            var result = DPRecycleBin.SendToRecycleBin(new[] { existingFile, file2 });

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(existingFile.Path));
            Assert.IsFalse(File.Exists(file2.Path));
        }

        [TestMethod]
        public void SendToRecycleBin_MixedInfosTest()
        {
            if (!Directory.Exists(existingDir.Path))
                Assert.Inconclusive("Existing dir did not exist");
            var file2 = defaultFS.CreateFileInfo(Path.Combine(existingDir.Path, "exist2.txt"));
            file2.Create().Dispose();
            var dir = defaultFS.CreateDirectoryInfo(Path.Combine(existingDir.Path, "Test2"));
            dir.Create();

            var result = DPRecycleBin.SendToRecycleBin(new IDPIONode[] { existingFile, dir, file2 });

            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(existingFile.Path));
            Assert.IsFalse(File.Exists(file2.Path));
            Assert.IsFalse(Directory.Exists(dir.Path));
        }

        [TestMethod]
        public void SendToRecycleBin_EmptyDPIONodeListTest()
        {
            var result = DPRecycleBin.SendToRecycleBin(Array.Empty<IDPIONode>());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SendToRecycleBin_EmptyDPIONodeTest()
        {
            var fi = new Mock<IDPIONode>();
            fi.SetupGet(x => x.Exists).Returns(true);
            fi.SetupGet(x => x.Path).Returns(string.Empty);

            var result = DPRecycleBin.SendToRecycleBin(fi.Object);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SendToRecycleBin_EmptyStringListTest()
        {
            var result = DPRecycleBin.SendToRecycleBin(Array.Empty<string>());

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SendToRecycleBin_EmptyStringTest()
        {
            var result = DPRecycleBin.SendToRecycleBin(string.Empty);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SendToRecycleBin_IgnoresNonExistantNodesTest()
        {
            var fs = new FakeFileSystem(new DPFileScopeSettings(Array.Empty<string>(), new[] { tempDir }, false));
            var file = fs.CreateFileInfo(existingFile.Path);
            var dir = fs.CreateDirectoryInfo(existingDir.Path);
            Mock.Get(file).SetupGet(x => x.Exists).Returns(false);

            var result = DPRecycleBin.SendToRecycleBin(new IDPIONode[] { file, dir });

            Assert.IsTrue(result);
            Assert.IsFalse(Directory.Exists(existingDir.Path));
            Assert.IsTrue(File.Exists(existingFile.Path));
        }
    }
}