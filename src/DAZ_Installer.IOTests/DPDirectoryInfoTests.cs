using DAZ_Installer.IO.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Diagnostics;

namespace DAZ_Installer.IO.Tests
{
    [TestClass]
#pragma warning disable CS0618 // FakeDirectoryInfo is obsolete for production, not testing.
    public class DPDirectoryInfoTests
    {
        private static FakeFileSystem unlimitedCtx = new FakeFileSystem(DPFileScopeSettings.All);
        private static FakeFileSystem noCtx = new FakeFileSystem(DPFileScopeSettings.None);
        private static DPDirectoryInfo existingDir = null!;
        private static DPDirectoryInfo nonexistantDir = null!;
        private static DPDirectoryInfo outOfScope = null!;
        private static DPDirectoryInfo destOutOfScope = null!;
        private static string initExistingDirectoryPath = null!;
        private static string initNonexistantDirectoryPath = null!;
        private static string initOutOfScopeDirectoryPath = null!;
        private static string initDestOutOfScopeDirectoryPath = null!;
        private static string tempDir = null!;

        [ClassInitialize]
        public static void ClassSetup(TestContext t)
        {
            tempDir = Path.Combine(t.DeploymentDirectory, "DAZ_Installer.IO.Tests.DPDirectoryInfoTests");

            initExistingDirectoryPath = Path.Combine(tempDir, "exist");
            initNonexistantDirectoryPath = Path.Combine(tempDir, "nonexistant");
            initOutOfScopeDirectoryPath = Path.Combine(tempDir, "outofscope");
            initDestOutOfScopeDirectoryPath = Path.Combine(tempDir, "destoutofscope");
        }

        [TestInitialize]
        public void TestInitialize()
        {
            existingDir = new DPDirectoryInfo(new FakeDirectoryInfo(initExistingDirectoryPath), unlimitedCtx);
            nonexistantDir = new DPDirectoryInfo(new FakeDirectoryInfo(initNonexistantDirectoryPath), unlimitedCtx);
            outOfScope = new DPDirectoryInfo(new FakeDirectoryInfo(initOutOfScopeDirectoryPath), noCtx);
            destOutOfScope = new DPDirectoryInfo(new FakeDirectoryInfo(initOutOfScopeDirectoryPath), 
                new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(new string[] { initOutOfScopeDirectoryPath }, Array.Empty<string>()))
            );

        }

        [TestMethod]
        public void CreateTest() => existingDir.Create();
        [TestMethod]
        public void CreateTest_OutOfScope() => Assert.ThrowsException<OutOfScopeException>(outOfScope.Create);

        [TestMethod]
        public void DeleteTest() => existingDir.Delete(false);
        [TestMethod]
        public void DeleteTest_RecursiveNoSubdirs()
        {
            var fakeDirInfo = new FakeDirectoryInfo(initExistingDirectoryPath);
            fakeDirInfo.Directories = new[] { new FakeDirectoryInfo(Path.Combine(initExistingDirectoryPath, "subdir")) };
            var dir = new DPDirectoryInfo(fakeDirInfo, unlimitedCtx);
            dir.Delete(true);

        }
        [TestMethod]
        public void DeleteTest_OutOfScope() => Assert.ThrowsException<OutOfScopeException>(() => outOfScope.Delete(false));
        [TestMethod]
        public void DeleteTest_OutOfScope_Recursive()
        {
            var fakeDirInfo = new FakeDirectoryInfo(initExistingDirectoryPath);
            var path = Path.Combine(initExistingDirectoryPath, "subdir");
            fakeDirInfo.Directories = new[] { new FakeDirectoryInfo(path) };
            var dir = new DPDirectoryInfo(fakeDirInfo, new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(Array.Empty<string>(), new[] { path })));
            Assert.ThrowsException<OutOfScopeException>(() => outOfScope.Delete(true));
        }
        [TestMethod]
        public void DeleteTest_SubDirOutOfScope_Recursive()
        {
            var fakeDirInfo = new FakeDirectoryInfo(initExistingDirectoryPath);
            var subDirPath = Path.Combine(initExistingDirectoryPath, "subdir");
            fakeDirInfo.Files = new[] { new FakeFileInfo(Path.Combine(subDirPath, "a.txt")) };
            var dir = new DPDirectoryInfo(fakeDirInfo, new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(Array.Empty<string>(), new[] { initExistingDirectoryPath })));
            Assert.ThrowsException<OutOfScopeException>(() => dir.Delete(true));
        }

        [TestMethod]
        public void MoveToTest() => existingDir.MoveTo("anywhere");
        [TestMethod]
        public void MoveToTest_Subdirs()
        {
            var fakeDirInfo = new FakeDirectoryInfo(initExistingDirectoryPath);
            fakeDirInfo.Directories = new[] { new FakeDirectoryInfo(Path.Combine(initExistingDirectoryPath, "subdir")) };
            var dir = new DPDirectoryInfo(fakeDirInfo, unlimitedCtx);
            dir.MoveTo("anywhere");
        }
        [TestMethod]
        public void MoveToTest_OutOfScope() => Assert.ThrowsException<OutOfScopeException>(() => outOfScope.MoveTo("anywhere"));
        [TestMethod]
        public void MoveToTest_OutOfScope_Subdirs()
        {
            var fakeDirInfo = new FakeDirectoryInfo(initExistingDirectoryPath);
            var path = Path.Combine(initExistingDirectoryPath, "subdir");
            fakeDirInfo.Directories = new[] { new FakeDirectoryInfo(path) };
            var dir = new DPDirectoryInfo(fakeDirInfo, new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(Array.Empty<string>(), new[] { path })));
            Assert.ThrowsException<OutOfScopeException>(() => outOfScope.MoveTo("anywhere"));
        }
        [TestMethod]
        public void MoveToTest_SubDirOutOfScope()
        {
            var fakeDirInfo = new FakeDirectoryInfo(initExistingDirectoryPath);
            fakeDirInfo.Directories = new[] { new FakeDirectoryInfo(Path.Combine(initExistingDirectoryPath, "subdir")) };
            var dir = new DPDirectoryInfo(fakeDirInfo, new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(Array.Empty<string>(), new[] { initExistingDirectoryPath })));
            Assert.ThrowsException<OutOfScopeException>(() => dir.MoveTo("anywhere"));
        }

        [TestMethod]
        public void PreviewCreateTest() => Assert.IsTrue(existingDir.PreviewCreate());
        [TestMethod]
        public void PreviewCreateTest_OutOfScope() => Assert.IsFalse(outOfScope.PreviewCreate());

        [TestMethod]
        public void PreviewDeleteTest() => Assert.IsTrue(existingDir.PreviewDelete(false));
        [TestMethod]
        public void PreviewDeleteTest_OutOfScope() => Assert.IsFalse(outOfScope.PreviewDelete(false));

        [TestMethod]
        public void PreviewMoveToTest() => Assert.IsTrue(existingDir.PreviewMoveTo("anywhere"));
        [TestMethod]
        public void PreviewMoveToTest_Subdirs()
        {
            var fakeDirInfo = new FakeDirectoryInfo(initExistingDirectoryPath);
            fakeDirInfo.Directories = new[] { new FakeDirectoryInfo(Path.Combine(initExistingDirectoryPath, "subdir")) };
            var dir = new DPDirectoryInfo(fakeDirInfo, unlimitedCtx);
            Assert.IsTrue(dir.TryMoveTo("anywhere"));
        }
        [TestMethod]
        public void PreviewMoveToTest_OutOfScope() => Assert.IsFalse(outOfScope.PreviewMoveTo("anywhere"));
        [TestMethod]
        public void PreviewMoveToTest_OutOfScope_Subdirs()
        {
            var fakeDirInfo = new FakeDirectoryInfo(initExistingDirectoryPath);
            var path = Path.Combine(initExistingDirectoryPath, "subdir");
            fakeDirInfo.Directories = new[] { new FakeDirectoryInfo(path) };
            var dir = new DPDirectoryInfo(fakeDirInfo, new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(Array.Empty<string>(), new[] { path })));
            Assert.IsFalse(dir.TryMoveTo("anywhere"));
        }
        [TestMethod]
        public void PreviewMoveToTest_SubDirOutOfScope()
        {
            var fakeDirInfo = new FakeDirectoryInfo(initExistingDirectoryPath);
            fakeDirInfo.Directories = new[] { new FakeDirectoryInfo(Path.Combine(initExistingDirectoryPath, "subdir")) };
            var dir = new DPDirectoryInfo(fakeDirInfo, new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(Array.Empty<string>(), new[] { initExistingDirectoryPath })));
            Assert.IsFalse(dir.TryMoveTo("anywhere"));
        }

        [TestMethod]
        public void TryCreateTest() => Assert.IsTrue(existingDir.TryCreate());
        [TestMethod]
        public void TryCreateTest_OutOfScope() => Assert.IsFalse(outOfScope.TryCreate());
        [TestMethod]
        public void TryCreateTest_UnexpectedError()
        {
            var fakeDirInfo = new Mock<FakeDirectoryInfo>("A:/Fake/Dir") { CallBase = true };
            fakeDirInfo.Setup(x => x.Create()).Throws(new Exception("gotcha bitch"));
            var dir = new DPDirectoryInfo(fakeDirInfo.Object, unlimitedCtx, null);
            Assert.IsFalse(dir.TryCreate());
        }

        [TestMethod]
        public void TryDeleteTest() => Assert.IsTrue(existingDir.TryDelete(true));
        [TestMethod]
        public void TryDeleteTest_OutOfScope() => Assert.IsFalse(outOfScope.TryDelete(true));
        [TestMethod]
        public void TryDeleteTest_UnexpectedError()
        {
            var fakeDirInfo = new Mock<FakeDirectoryInfo>("A:/Fake/Dir") { CallBase = true };
            fakeDirInfo.Setup(x => x.Delete(It.IsAny<bool>())).Throws(new Exception("gotcha bitch"));
            var dir = new DPDirectoryInfo(fakeDirInfo.Object, unlimitedCtx, null);
            Assert.IsFalse(dir.TryDelete(true));
        }

        [TestMethod]
        public void TryMoveToTest() => Assert.IsTrue(existingDir.TryMoveTo("anywhere"));
        [TestMethod]
        public void TryMoveToTest_OutOfScope() => Assert.IsFalse(outOfScope.TryMoveTo("anywhere"));
        [TestMethod]
        public void TryMoveToTest_UnexpectedError()
        {
            var fakeDirInfo = new Mock<FakeDirectoryInfo>("A:/Fake/Dir") { CallBase = true };
            fakeDirInfo.Setup(x => x.MoveTo("anywhere")).Throws(new Exception("gotcha bitch"));
            var dir = new DPDirectoryInfo(fakeDirInfo.Object, unlimitedCtx, null);
            Assert.IsFalse(dir.TryMoveTo("anywhere"));
        }
        [TestMethod]
        public void FileSystemContextTest()
        {
            var scope = new DPFileScopeSettings();
            var ctx = new FakeFileSystem(scope);
            var a = new DPDirectoryInfo(new FakeDirectoryInfo("a"), ctx);
            Assert.AreSame(a.FileSystem, ctx);
        }

        [TestMethod]
        public void FileSystemContextTest_DirListedInContext()
        {
            var f = new FakeDirectoryInfo("Z://a");
            f.Parent = new FakeDirectoryInfo("Z://");
            var dir = new DPDirectoryInfo(f, new FakeFileSystem());
            Assert.AreSame(dir!.FileSystem, dir.Parent.FileSystem);
        }
        [TestMethod]
        public void TryAndFixMoveToTest()
        {
            Assert.IsTrue(existingDir.TryAndFixMoveTo(Path.Combine(tempDir, "existing2"), out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixMoveToTest_Blacklisted()
        {
            Assert.IsFalse(outOfScope.TryAndFixMoveTo(Path.Combine(tempDir, "existing2"), out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixMoveToTest_NotExist()
        {
            var f = new FakeDirectoryInfo("Z://a");
            f.Exists = false;
            var fs = new DPDirectoryInfo(f, noCtx);

            Assert.IsFalse(fs.TryAndFixMoveTo(Path.Combine(tempDir, "existing2"), out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixMoveToTest_FixUnauthorizedSuccess()
        {
            var f = new Mock<FakeDirectoryInfo>("Z://a") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.MoveTo(It.IsAny<string>())).Callback(() =>
            {
                if (f.Object.Attributes.HasFlag(FileAttributes.ReadOnly) || f.Object.Attributes.HasFlag(FileAttributes.Hidden))
                    throw new UnauthorizedAccessException();
            });
            var fs = new DPDirectoryInfo(f.Object, unlimitedCtx, null);
            Assert.IsTrue(fs.TryAndFixMoveTo(Path.Combine(tempDir, "existing2"), out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixMoveToTest_FixUnauthorizedFail()
        {
            var f = new Mock<FakeDirectoryInfo>("Z://a") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.MoveTo(It.IsAny<string>())).Throws(new UnauthorizedAccessException());
            var fs = new DPDirectoryInfo(f.Object, unlimitedCtx, null);
            Assert.IsFalse(fs.TryAndFixMoveTo(Path.Combine(tempDir, "existing2"), out var ex));
            Assert.IsNotNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest()
        {
            Assert.IsTrue(existingDir.TryAndFixDelete(true, out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest_Blacklisted()
        {
            Assert.IsFalse(outOfScope.TryAndFixDelete(true, out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest_NotExist()
        {
            var f = new FakeDirectoryInfo("Z://a");
            f.Exists = false;
            var fs = new DPDirectoryInfo(f, noCtx);

            Assert.IsFalse(fs.TryAndFixDelete(true, out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest_FixUnauthorizedSuccess()
        {
            var f = new Mock<FakeDirectoryInfo>("Z://a") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.Delete(It.IsAny<bool>())).Callback(() =>
            {
                if (f.Object.Attributes.HasFlag(FileAttributes.ReadOnly) || f.Object.Attributes.HasFlag(FileAttributes.Hidden))
                    throw new UnauthorizedAccessException();
            });
            var fs = new DPDirectoryInfo(f.Object, unlimitedCtx, null);
            Assert.IsTrue(fs.TryAndFixDelete(true, out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest_FixUnauthorizedFail()
        {
            var f = new Mock<FakeDirectoryInfo>("Z://a") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.Delete(It.IsAny<bool>())).Throws(new UnauthorizedAccessException());
            var fs = new DPDirectoryInfo(f.Object, unlimitedCtx, null);
            Assert.IsFalse(fs.TryAndFixDelete(true, out var ex));
            Assert.IsNotNull(ex);
        }
    }
}