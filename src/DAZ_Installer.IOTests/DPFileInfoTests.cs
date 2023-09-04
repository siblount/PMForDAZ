using DAZ_Installer.IO.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DAZ_Installer.IO.Tests
{
    [TestClass]
#pragma warning disable CS0618 // This is for testing, as intended.
    public class DPFileInfoTests
    {
        private static FakeFileSystem unlimitedCtx = new FakeFileSystem(DPFileScopeSettings.All);
        private static FakeFileSystem noCtx = new FakeFileSystem(DPFileScopeSettings.None);
        private static DPFileInfo existingFile = null!;
        private static DPFileInfo nonexistantFile = null!;
        private static DPFileInfo outOfScope = null!;
        private static DPFileInfo destOutOfScope = null!;
        private static string initExistingFilePath = null!;
        private static string initNonexistantFilePath = null!;
        private static string initOutOfScopeFilePath = null!;
        private static string initDestOutOfScopeFilePath = null!;
        private static string tempDir = null!;

        [ClassInitialize]
        public static void ClassSetup(TestContext t)
        {
            tempDir = Path.Combine(t.DeploymentDirectory, "DAZ_Installer.IO.Tests.DPFileInfoTests");

            initExistingFilePath = Path.Combine(tempDir, "exist.txt");
            initNonexistantFilePath = Path.Combine(tempDir, "nonexistant.txt");
            initOutOfScopeFilePath = Path.Combine(tempDir, "outofscope.txt");
            initDestOutOfScopeFilePath = Path.Combine(tempDir, "destoutofscope.txt");
        }

        [TestInitialize]
        public void TestInitialize()
        {
            existingFile = new DPFileInfo(new FakeFileInfo(initExistingFilePath), unlimitedCtx);
            nonexistantFile = new DPFileInfo(new FakeFileInfo(initNonexistantFilePath), unlimitedCtx);
            outOfScope = new DPFileInfo(new FakeFileInfo(initOutOfScopeFilePath), noCtx);
            destOutOfScope = new DPFileInfo(new FakeFileInfo(initOutOfScopeFilePath),
                new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(new string[] { initOutOfScopeFilePath }, Array.Empty<string>()))
            );
        }

        [TestMethod]
        public void CopyToTest_ExistingFile()
        {
            IDPFileInfo f = existingFile.CopyTo(Path.Combine(tempDir, "existing.txt"), true);
            Assert.AreEqual(f.Path, Path.Combine(tempDir, "existing.txt"));
        }
        [TestMethod]
        public void CopyToTest_OutOfScope_ExistingFile()
        {
            var tempFilePath = Path.Combine(tempDir, "existing.txt");
            var specificSettings = DPFileScopeSettings.CreateUltraStrict(new string[] { tempFilePath }, Array.Empty<string>());
            var f = new DPFileInfo(existingFile.Path, new FakeFileSystem(specificSettings));
            Assert.ThrowsException<OutOfScopeException>(() => f.CopyTo(Path.Combine(tempDir, "existing.txt"), true));
        }
        [TestMethod]
        public void CopyToTest_NonexistantFile() => nonexistantFile.CopyTo(Path.Combine(tempDir, "nonexistant.txt"), true);
        [TestMethod]
        public void CopyToTest_OutOfScope_SourceFile() => Assert.ThrowsException<OutOfScopeException>(() => outOfScope.CopyTo(Path.Combine(tempDir, "existing.txt"), true));

        [TestMethod]
        public void CreateTest_ExistingFile() => existingFile.Create().Dispose();
        [TestMethod]
        public void CreateTest_NonexistantFile() => nonexistantFile.Create().Dispose();
        [TestMethod]
        public void CreateTest_OutOfScopeFile() => Assert.ThrowsException<OutOfScopeException>(() => outOfScope.Create().Dispose());

        [TestMethod]
        public void MoveToTest_ExistingFile() => existingFile.MoveTo(Path.Combine(tempDir, "existing2.txt"), true);
        [TestMethod]
        public void MoveToTest_SourceOutOfScope_ExistingFile()
        {
            var dest = Path.Combine(tempDir, "existing2.txt");
            var a = new DPFileInfo(existingFile.Path,
                new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(new[] { dest }, new[] { Path.GetDirectoryName(dest)! }))
            );
            Assert.ThrowsException<OutOfScopeException>(() => a.MoveTo(dest, true));
        }
        [TestMethod]
        public void MoveToTest_DestOutOfScope_ExistingFile()
        {
            var dest = Path.Combine(tempDir, "existing2.txt");
            var a = new DPFileInfo(existingFile.Path,
                new FakeFileSystem(DPFileScopeSettings.CreateUltraStrict(new[] { existingFile.Path }, Array.Empty<string>()))
            );
            Assert.ThrowsException<OutOfScopeException>(() => a.MoveTo(dest, true));
        }
        [TestMethod]
        public void MoveToTest_NonexistantFile()
        {
            try
            {
                var dest = Path.Combine(tempDir, "existing2.txt");
                nonexistantFile.MoveTo(dest, true);
            }
            catch (OutOfScopeException ex)
            {
                Assert.Fail($"OutOfScopeException thrown when it should not have been: {ex}");
            }
            catch { }
        }
        [TestMethod]
        public void MoveToTest_OutOfScopeFile() => Assert.ThrowsException<OutOfScopeException>(() => outOfScope.MoveTo("", true));

        [TestMethod]
        public void OpenReadTest()
        {
            existingFile.OpenRead().Dispose();
            outOfScope.OpenRead().Dispose();
            try
            {
                nonexistantFile.OpenRead().Dispose();
            }
            catch (OutOfScopeException ex)
            {
                Assert.Fail($"OutOfScopeException thrown when it should not have been: {ex}");
            }
            catch { }
        }

        [TestMethod]
        public void OpenWriteTest_ExistingFile() => existingFile.OpenWrite().Dispose();

        [TestMethod]
        public void OpenWriteTest_NonexistantFile() => nonexistantFile.OpenWrite().Dispose();

        [TestMethod]
        public void OpenWriteTest_OutOfScopeFile() => Assert.ThrowsException<OutOfScopeException>(outOfScope.OpenWrite);

        [DataTestMethod]
        [DataRow(FileMode.CreateNew, FileAccess.Read)]
        [DataRow(FileMode.CreateNew, FileAccess.ReadWrite)]
        [DataRow(FileMode.CreateNew, FileAccess.Write)]
        [DataRow(FileMode.Create, FileAccess.Read)]
        [DataRow(FileMode.Create, FileAccess.ReadWrite)]
        [DataRow(FileMode.Create, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.ReadWrite)]
        [DataRow(FileMode.Open, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.Read)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.ReadWrite)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Read)]
        [DataRow(FileMode.Truncate, FileAccess.ReadWrite)]
        [DataRow(FileMode.Truncate, FileAccess.Write)]
        [DataRow(FileMode.Truncate, FileAccess.Read)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite)]
        [DataRow(FileMode.Append, FileAccess.Write)]
        [DataRow(FileMode.Append, FileAccess.Read)]

        public void OpenTest_ExistingFile_NoThrowUnauthorzied(FileMode mode, FileAccess access)
        {
            // we only care about if it throws OutOfScopeException by us, not the system.
            try
            {
                existingFile.Open(mode, access).Dispose();
            }
            catch (OutOfScopeException e) when (e.Message.Contains("Access to the path")) { Assert.Fail(); }
            catch { }
        }

        [DataTestMethod]
        [DataRow(FileMode.CreateNew, FileAccess.Read)]
        [DataRow(FileMode.CreateNew, FileAccess.ReadWrite)]
        [DataRow(FileMode.CreateNew, FileAccess.Write)]
        [DataRow(FileMode.Create, FileAccess.Read)]
        [DataRow(FileMode.Create, FileAccess.ReadWrite)]
        [DataRow(FileMode.Create, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.ReadWrite)]
        [DataRow(FileMode.Open, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.Read)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.ReadWrite)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Read)]
        [DataRow(FileMode.Truncate, FileAccess.ReadWrite)]
        [DataRow(FileMode.Truncate, FileAccess.Write)]
        [DataRow(FileMode.Truncate, FileAccess.Read)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite)]
        [DataRow(FileMode.Append, FileAccess.Write)]
        [DataRow(FileMode.Append, FileAccess.Read)]
        public void OpenTest_Nonexistant_NoThrow(FileMode mode, FileAccess access)
        {
            // we only care about if it throws OutOfScopeException by us, not the system.
            try
            {
                existingFile.Open(mode, access).Dispose();
            }
            catch (OutOfScopeException e) when (e.Message.Contains("Access to the path")) { Assert.Fail(); }
            catch { }
        }

        [DataTestMethod]
        [DataRow(FileMode.Open, FileAccess.ReadWrite)]
        [DataRow(FileMode.Open, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.Read)]
        public void OpenTest_OutOfScope_NoThrow(FileMode mode, FileAccess access)
        {
            // we only care about if it throws OutOfScopeException by us, not the system.
            try
            {
                outOfScope.Open(mode, access).Dispose();
            }
            catch (OutOfScopeException e) when (e.Message.Contains("Access to the path")) { Assert.Fail(); }
            catch { }
        }

        [DataTestMethod]
        [DataRow(FileMode.CreateNew, FileAccess.Read)]
        [DataRow(FileMode.CreateNew, FileAccess.ReadWrite)]
        [DataRow(FileMode.CreateNew, FileAccess.Write)]
        [DataRow(FileMode.Create, FileAccess.Read)]
        [DataRow(FileMode.Create, FileAccess.ReadWrite)]
        [DataRow(FileMode.Create, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.ReadWrite)]
        [DataRow(FileMode.Open, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.ReadWrite)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Read)]
        [DataRow(FileMode.Truncate, FileAccess.ReadWrite)]
        [DataRow(FileMode.Truncate, FileAccess.Write)]
        [DataRow(FileMode.Truncate, FileAccess.Read)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite)]
        [DataRow(FileMode.Append, FileAccess.Write)]
        [DataRow(FileMode.Append, FileAccess.Read)]
        public void OpenTest_OutOfScope_Throws(FileMode mode, FileAccess access) =>
            // we only care about if it throws OutOfScopeException by us, not the system.
            Assert.ThrowsException<OutOfScopeException>(() => outOfScope.Open(mode, access));

        [TestMethod]
        public void DeleteTest_ExistingFile() => existingFile.Delete();
        [TestMethod]
        public void DeleteTest_NonexistantFile() => nonexistantFile.Delete();
        [TestMethod]
        public void DeleteTest_OutOfScopeFile() => Assert.ThrowsException<OutOfScopeException>(outOfScope.Delete);
        [TestMethod]
        public void PreviewCreateTest_ExistingFile() => Assert.IsTrue(existingFile.PreviewCreate());
        [TestMethod]
        public void PreviewCreateTest_NonexistantFile() => Assert.IsTrue(nonexistantFile.PreviewCreate());
        [TestMethod]
        public void PreviewCreateTest_OutOfScopeFile() => Assert.IsFalse(outOfScope.PreviewCreate());
        [TestMethod]
        public void PreviewDeleteTest_ExistingFile() => Assert.IsTrue(existingFile.PreviewDelete());
        [TestMethod]
        public void PreviewDeleteTest_NonexistantFile() => Assert.IsTrue(nonexistantFile.PreviewDelete());
        [TestMethod]
        public void PreviewDeleteTest_OutOfScopeFile() => Assert.IsFalse(outOfScope.PreviewDelete());
        [DataTestMethod]
        [DataRow(FileMode.CreateNew, FileAccess.Read)]
        [DataRow(FileMode.CreateNew, FileAccess.ReadWrite)]
        [DataRow(FileMode.CreateNew, FileAccess.Write)]
        [DataRow(FileMode.Create, FileAccess.Read)]
        [DataRow(FileMode.Create, FileAccess.ReadWrite)]
        [DataRow(FileMode.Create, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.ReadWrite)]
        [DataRow(FileMode.Open, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.Read)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.ReadWrite)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Read)]
        [DataRow(FileMode.Truncate, FileAccess.ReadWrite)]
        [DataRow(FileMode.Truncate, FileAccess.Write)]
        [DataRow(FileMode.Truncate, FileAccess.Read)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite)]
        [DataRow(FileMode.Append, FileAccess.Write)]
        [DataRow(FileMode.Append, FileAccess.Read)]
        public void PreviewOpenTest_ExistingFile(FileMode mode, FileAccess access) => Assert.IsTrue(existingFile.PreviewOpen(mode, access));
        [DataTestMethod]
        [DataRow(FileMode.CreateNew, FileAccess.Read)]
        [DataRow(FileMode.CreateNew, FileAccess.ReadWrite)]
        [DataRow(FileMode.CreateNew, FileAccess.Write)]
        [DataRow(FileMode.Create, FileAccess.Read)]
        [DataRow(FileMode.Create, FileAccess.ReadWrite)]
        [DataRow(FileMode.Create, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.ReadWrite)]
        [DataRow(FileMode.Open, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.Read)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.ReadWrite)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Read)]
        [DataRow(FileMode.Truncate, FileAccess.ReadWrite)]
        [DataRow(FileMode.Truncate, FileAccess.Write)]
        [DataRow(FileMode.Truncate, FileAccess.Read)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite)]
        [DataRow(FileMode.Append, FileAccess.Write)]
        [DataRow(FileMode.Append, FileAccess.Read)]
        public void PreviewOpenTest_NonexistantFile(FileMode mode, FileAccess access) => Assert.IsTrue(nonexistantFile.PreviewOpen(mode, access));
        [DataTestMethod]
        [DataRow(FileMode.Open, FileAccess.Read)]
        public void PreviewOpenTest_OutOfScopeFile_True(FileMode mode, FileAccess access) => Assert.IsTrue(outOfScope.PreviewOpen(mode, access));
        [DataTestMethod]
        [DataRow(FileMode.CreateNew, FileAccess.Read)]
        [DataRow(FileMode.CreateNew, FileAccess.ReadWrite)]
        [DataRow(FileMode.CreateNew, FileAccess.Write)]
        [DataRow(FileMode.Create, FileAccess.Read)]
        [DataRow(FileMode.Create, FileAccess.ReadWrite)]
        [DataRow(FileMode.Create, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.ReadWrite)]
        [DataRow(FileMode.Open, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.ReadWrite)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Read)]
        [DataRow(FileMode.Truncate, FileAccess.ReadWrite)]
        [DataRow(FileMode.Truncate, FileAccess.Write)]
        [DataRow(FileMode.Truncate, FileAccess.Read)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite)]
        [DataRow(FileMode.Append, FileAccess.Write)]
        [DataRow(FileMode.Append, FileAccess.Read)]
        public void PreviewOpenTest_OutOfScopeFile_False(FileMode mode, FileAccess access) => Assert.IsFalse(outOfScope.PreviewOpen(mode, access));
        [TestMethod]
        public void PreviewMoveToTest_Scoped() => Assert.IsTrue(existingFile.PreviewMoveTo("anything", true));
        [TestMethod]
        public void PreviewMoveToTest() => Assert.IsTrue(existingFile.PreviewMoveTo("anything", true));
        [TestMethod]
        public void PreviewMoveToTest_SourceOutOfScope() => Assert.IsFalse(outOfScope.PreviewMoveTo("anything", true));
        [TestMethod]
        public void PreviewMoveToTest_DestOutOfScope() => Assert.IsFalse(destOutOfScope.PreviewMoveTo("anything", true));

        [TestMethod]
        public void PreviewCopyToTest_SourceOutOfScope() => Assert.IsFalse(outOfScope.PreviewCopyTo("anything", true));
        [TestMethod]
        public void PreviewCopyToTest_DestOutOfScope() => Assert.IsFalse(destOutOfScope.PreviewCopyTo("anything", true));

        [TestMethod]
        public void TryCreateTest()
        {
            Assert.IsTrue(existingFile.TryCreate(out Stream? c));
            c.Dispose();
        }

        [TestMethod]
        public void TryCreateTest_OutOfScope() => Assert.IsFalse(outOfScope.TryCreate(out Stream? _));

        [TestMethod]
        public void TryDeleteTest() => Assert.IsTrue(existingFile.TryDelete());
        [TestMethod]
        public void TryDeleteTest_OutOfScope() => Assert.IsFalse(outOfScope.TryDelete());

        [TestMethod]
        public void TryMoveToTest() => Assert.IsTrue(existingFile.TryMoveTo(Path.Combine(tempDir, "existing2.txt"), true));
        [TestMethod]
        public void TryMoveToTest_SourceOutOfScope() => Assert.IsFalse(outOfScope.TryMoveTo(Path.Combine(tempDir, "existing2.txt"), true));
        [TestMethod]
        public void TryMoveToTest_DestOutOfScope() => Assert.IsFalse(destOutOfScope.TryMoveTo(Path.Combine(tempDir, "existing2.txt"), true));

        [TestMethod]
        public void TryCopyToTest() => Assert.IsTrue(existingFile.TryCopyTo(Path.Combine(tempDir, "existing2.txt"), true, out IDPFileInfo? _));
        [TestMethod]
        public void TryCopyToTest_SourceOutOfScope() => Assert.IsFalse(outOfScope.TryCopyTo(Path.Combine(tempDir, "existing2.txt"), true, out IDPFileInfo? _));
        [TestMethod]
        public void TryCopyToTest_DestOutOfScope() => Assert.IsFalse(destOutOfScope.TryCopyTo(Path.Combine(tempDir, "existing2.txt"), true, out IDPFileInfo? _));

        [TestMethod]
        public void TryOpenReadTest()
        {
            Assert.IsTrue(existingFile.TryOpenRead(out Stream? c));
            c.Dispose();
        }

        [TestMethod]
        public void TryOpenWriteTest()
        {
            Assert.IsTrue(existingFile.TryOpenWrite(out Stream? c));
            c.Dispose();
        }
        [TestMethod]
        public void TryOpenWriteTest_OutOfScope() => Assert.IsFalse(outOfScope.TryOpenWrite(out Stream? c));

        [DataTestMethod]
        [DataRow(FileMode.CreateNew, FileAccess.Read)]
        [DataRow(FileMode.CreateNew, FileAccess.ReadWrite)]
        [DataRow(FileMode.CreateNew, FileAccess.Write)]
        [DataRow(FileMode.Create, FileAccess.Read)]
        [DataRow(FileMode.Create, FileAccess.ReadWrite)]
        [DataRow(FileMode.Create, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.ReadWrite)]
        [DataRow(FileMode.Open, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.Read)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.ReadWrite)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Read)]
        [DataRow(FileMode.Truncate, FileAccess.ReadWrite)]
        [DataRow(FileMode.Truncate, FileAccess.Write)]
        [DataRow(FileMode.Truncate, FileAccess.Read)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite)]
        [DataRow(FileMode.Append, FileAccess.Write)]
        [DataRow(FileMode.Append, FileAccess.Read)]
        public void TryOpenTest(FileMode mode, FileAccess access)
        {
            try
            {
                existingFile.Open(mode, access).Dispose();
            }
            catch (OutOfScopeException e) when (e.Message.Contains("Access to the path")) { Assert.Fail(); }
            catch { }
        }
        [TestMethod]
        [DataRow(FileMode.CreateNew, FileAccess.Read)]
        [DataRow(FileMode.CreateNew, FileAccess.ReadWrite)]
        [DataRow(FileMode.CreateNew, FileAccess.Write)]
        [DataRow(FileMode.Create, FileAccess.Read)]
        [DataRow(FileMode.Create, FileAccess.ReadWrite)]
        [DataRow(FileMode.Create, FileAccess.Write)]
        [DataRow(FileMode.Open, FileAccess.ReadWrite)]
        [DataRow(FileMode.Open, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.ReadWrite)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Write)]
        [DataRow(FileMode.OpenOrCreate, FileAccess.Read)]
        [DataRow(FileMode.Truncate, FileAccess.ReadWrite)]
        [DataRow(FileMode.Truncate, FileAccess.Write)]
        [DataRow(FileMode.Truncate, FileAccess.Read)]
        [DataRow(FileMode.Append, FileAccess.ReadWrite)]
        [DataRow(FileMode.Append, FileAccess.Write)]
        [DataRow(FileMode.Append, FileAccess.Read)]
        public void TryOpenTest_OutOfScope_Throws(FileMode mode, FileAccess access)
        {
            try
            {
                outOfScope.Open(mode, access).Dispose();
            }
            catch (OutOfScopeException e) when (e.Message.Contains("Access to the path")) { Assert.Fail(); }
            catch { }
        }
        [DataTestMethod]
        [DataRow(FileMode.Open, FileAccess.Read)]
        public void TryOpenTest_OutOfScope_NoThrow(FileMode mode, FileAccess access)
        {
            try
            {
                outOfScope.Open(mode, access).Dispose();
            }
            catch (OutOfScopeException e) when (e.Message.Contains("Access to the path")) { Assert.Fail(); }
            catch { }
        }
        [TestMethod]
        public void Directory_RetryOnNull()
        {
            var fake = new FakeFileInfo("D:/whoyomama/a.png");
            fake.Directory = new FakeDirectoryInfo("D:/whoyomama");
            var specific = new DPFileInfo(fake, noCtx, null);
            Assert.IsNotNull(specific.Directory);
        }
        [TestMethod]
        public void ContextTest()
        {
            var scope = new DPFileScopeSettings();
            var ctx = new FakeFileSystem(scope);
            var a = new DPFileInfo(new FakeFileInfo("a.txt"), ctx);
            Assert.AreSame(a.FileSystem, ctx);
        }
        [TestMethod]
        public void ContextTest_ContextChanged()
        {
            existingFile.FileSystem = noCtx;
            Assert.AreSame(noCtx, existingFile.FileSystem);
        }
        [TestMethod]
        public void ContextTest_DirListedInContext()
        {
            var f = new FakeFileInfo("Z://a.txt");
            f.Directory = new FakeDirectoryInfo("Z://");
            var file = new DPFileInfo(f, noCtx);
            Assert.AreSame(file!.FileSystem, file.Directory.FileSystem);
        }
        [TestMethod]
        public void ScopeTest()
        {
            var scope = new DPFileScopeSettings();
            var ctx = new FakeFileSystem(scope);
            var a = new DPFileInfo(new FakeFileInfo("a.txt"), ctx);
            Assert.AreSame(a.Scope, scope);
        }

        [TestMethod]
        public void TryAndFixMoveToTest()
        {
            Assert.IsTrue(existingFile.TryAndFixMoveTo(Path.Combine(tempDir, "existing2.txt"), true, out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixMoveToTest_Blacklisted()
        {
            Assert.IsFalse(outOfScope.TryAndFixMoveTo(Path.Combine(tempDir, "existing2.txt"), true, out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixMoveToTest_NotExist()
        {
            var f = new FakeFileInfo("Z://a.txt");
            f.Exists = false;
            var fs = new DPFileInfo(f, noCtx);
            
            Assert.IsFalse(fs.TryAndFixMoveTo(Path.Combine(tempDir, "existing2.txt"), true, out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixMoveToTest_FixUnauthorizedSuccess()
        {
            var f = new Mock<FakeFileInfo>("Z://a.txt") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.MoveTo(It.IsAny<string>(), It.IsAny<bool>())).Callback(() =>
            {
                if (f.Object.Attributes.HasFlag(FileAttributes.ReadOnly) || f.Object.Attributes.HasFlag(FileAttributes.Hidden))
                    throw new UnauthorizedAccessException();
            });
            var fs = new DPFileInfo(f.Object, unlimitedCtx, null);
            Assert.IsTrue(fs.TryAndFixMoveTo(Path.Combine(tempDir, "existing2.txt"), true, out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixMoveToTest_FixUnauthorizedFail()
        {
            var f = new Mock<FakeFileInfo>("Z://a.txt") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.MoveTo(It.IsAny<string>(), It.IsAny<bool>())).Throws(new UnauthorizedAccessException());
            var fs = new DPFileInfo(f.Object, unlimitedCtx, null);
            Assert.IsFalse(fs.TryAndFixMoveTo(Path.Combine(tempDir, "existing2.txt"), true, out var ex));
            Assert.IsNotNull(ex);
        }
        [TestMethod]
        public void TryAndFixCopyToTest()
        {
            Assert.IsTrue(existingFile.TryAndFixCopyTo(Path.Combine(tempDir, "existing2.txt"), true, out var stream, out var ex));
            Assert.IsNotNull(stream);
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixCopyToTest_Blacklisted()
        {
            Assert.IsFalse(outOfScope.TryAndFixCopyTo(Path.Combine(tempDir, "existing2.txt"), true, out var stream, out var ex));
            Assert.IsNull(ex);
            Assert.IsNull(stream);

        }
        [TestMethod]
        public void TryAndFixCopyToTest_NotExist()
        {
            var f = new FakeFileInfo("Z://a.txt");
            f.Exists = false;
            var fs = new DPFileInfo(f, noCtx);

            Assert.IsFalse(fs.TryAndFixCopyTo(Path.Combine(tempDir, "existing2.txt"), true, out var stream, out var ex));
            Assert.IsNull(ex);
            Assert.IsNull(stream);
        }
        [TestMethod]
        public void TryAndFixCopyToTest_FixUnauthorizedSuccess()
        {
            var f = new Mock<FakeFileInfo>("Z://a.txt") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.CopyTo(It.IsAny<string>(), It.IsAny<bool>())).Returns(() =>
            {
                if (f.Object.Attributes.HasFlag(FileAttributes.ReadOnly) || f.Object.Attributes.HasFlag(FileAttributes.Hidden))
                    throw new UnauthorizedAccessException();
                return new FakeFileInfo("Z://a.txt");
            });
            var fs = new DPFileInfo(f.Object, unlimitedCtx, null);

            Assert.IsTrue(fs.TryAndFixCopyTo(Path.Combine(tempDir, "existing2.txt"), true, out var stream, out var ex));
            Assert.IsNull(ex);
            Assert.IsNotNull(stream);
        }
        [TestMethod]
        public void TryAndFixCopyToTest_FixUnauthorizedFail()
        {
            var f = new Mock<FakeFileInfo>("Z://a.txt") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.CopyTo(It.IsAny<string>(), It.IsAny<bool>())).Throws(new UnauthorizedAccessException());
            var fs = new DPFileInfo(f.Object, unlimitedCtx, null);
            Assert.IsFalse(fs.TryAndFixCopyTo(Path.Combine(tempDir, "existing2.txt"), true, out var stream, out var ex));
            Assert.IsNotNull(ex);
            Assert.IsNull(stream);
        }
        [TestMethod]
        public void TryAndFixOpenTest()
        {
            Assert.IsTrue(existingFile.TryAndFixOpen(FileMode.Open, FileAccess.Read, out var stream, out var ex));
            Assert.IsNull(ex);
            Assert.IsNotNull(stream);
        }
        [TestMethod]
        public void TryAndFixOpenTest_Blacklisted()
        {
            Assert.IsFalse(outOfScope.TryAndFixOpen(FileMode.Open, FileAccess.Read, out var stream, out var ex));
            Assert.IsNull(ex);
            Assert.IsNull(stream);
        }
        [TestMethod]
        public void TryAndFixOpenTest_NotExist()
        {
            var f = new FakeFileInfo("Z://a.txt");
            f.Exists = false;
            var fs = new DPFileInfo(f, noCtx);

            Assert.IsFalse(fs.TryAndFixOpen(FileMode.Open, FileAccess.Read, out var stream, out var ex));
            Assert.IsNull(stream);
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixOpenTest_FixUnauthorizedSuccess()
        {
            var f = new Mock<FakeFileInfo>("Z://a.txt") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>())).Returns(() =>
            {
                if (f.Object.Attributes.HasFlag(FileAttributes.ReadOnly) || f.Object.Attributes.HasFlag(FileAttributes.Hidden))
                    throw new UnauthorizedAccessException();
                return Stream.Null;
            });
            var fs = new DPFileInfo(f.Object, unlimitedCtx, null);

            Assert.IsTrue(fs.TryAndFixOpen(FileMode.Open, FileAccess.Read, out var stream, out var ex));
            Assert.IsNotNull(stream);
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixOpenTest_FixUnauthorizedFail()
        {
            var f = new Mock<FakeFileInfo>("Z://a.txt") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>())).Throws(new UnauthorizedAccessException());
            var fs = new DPFileInfo(f.Object, unlimitedCtx, null);

            Assert.IsFalse(fs.TryAndFixOpen(FileMode.Open, FileAccess.Read, out var stream, out var ex));
            Assert.IsNull(stream);
            Assert.IsNotNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest()
        {
            Assert.IsTrue(existingFile.TryAndFixDelete(out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest_Blacklisted()
        {
            Assert.IsFalse(outOfScope.TryAndFixDelete(out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest_NotExist()
        {
            var f = new FakeFileInfo("Z://a.txt");
            f.Exists = false;
            var fs = new DPFileInfo(f, noCtx);

            Assert.IsFalse(fs.TryAndFixDelete(out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest_FixUnauthorizedSuccess()
        {
            var f = new Mock<FakeFileInfo>("Z://a.txt") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.Delete()).Callback(() =>
            {
                if (f.Object.Attributes.HasFlag(FileAttributes.ReadOnly) || f.Object.Attributes.HasFlag(FileAttributes.Hidden))
                    throw new UnauthorizedAccessException();
            });
            var fs = new DPFileInfo(f.Object, unlimitedCtx, null);

            Assert.IsTrue(fs.TryAndFixDelete(out var ex));
            Assert.IsNull(ex);
        }
        [TestMethod]
        public void TryAndFixDeleteTest_FixUnauthorizedFail()
        {
            var f = new Mock<FakeFileInfo>("Z://a.txt") { CallBase = true };
            f.Object.Attributes = FileAttributes.Hidden | FileAttributes.ReadOnly;
            f.Setup(x => x.Delete()).Throws(new UnauthorizedAccessException());
            var fs = new DPFileInfo(f.Object, unlimitedCtx, null);

            Assert.IsFalse(fs.TryAndFixDelete(out var ex));
            Assert.IsNotNull(ex);
        }
    }
}