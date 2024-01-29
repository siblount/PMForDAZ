using DAZ_Installer.IO.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DAZ_Installer.IO.Integration.Tests
{
    [TestClass]
#pragma warning disable CS0618 // This is for testing, as intended.
    public class DPFileInfoTests
    {
        private static DPFileSystem unlimitedCtx = new DPFileSystem(DPFileScopeSettings.All);
        private static DPFileSystem noCtx = new DPFileSystem(DPFileScopeSettings.None);
        private static DPFileInfo existingFile = null!;
        private static DPFileInfo nonexistantFile = null!;
        private static DPFileInfo outOfScope = null!;
        private static DPFileInfo destOutOfScope = null!;
        private static string initExistingFilePath = null!;
        private static string tempDir = Path.Combine(Path.GetTempPath(), "DAZ_Installer.IO.Integration.Tests.DPFileInfoTests");
        private static DPFileSystem defaultFS = new DPFileSystem(new DPFileScopeSettings(Array.Empty<string>(), new[] { tempDir }, false));

        [ClassInitialize]
        public static void ClassSetup(TestContext t)
        {
            Directory.CreateDirectory(tempDir);
            initExistingFilePath = Path.Combine(tempDir, "exist.txt");
        }

        [TestInitialize]
        public void TestInitialize()
        {
            existingFile = new DPFileInfo(initExistingFilePath, defaultFS);
            existingFile.TryCreate(out var stream);
            stream?.Dispose();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            existingFile.TryDelete();
            nonexistantFile.TryDelete();
            outOfScope.TryDelete();
            destOutOfScope.TryDelete();
        }

        [TestMethod]
        public void SendToRecycleBinTest()
        {
            var result = existingFile.SendToRecycleBin();
            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(initExistingFilePath));
        }
        [TestMethod]
        public void TrySendToRecycleBinTest()
        {
            var result = existingFile.TrySendToRecycleBin(out var ex);
            Assert.IsTrue(result);
            Assert.IsNull(ex);
            Assert.IsFalse(File.Exists(initExistingFilePath));
        }
        [TestMethod]
        public void TryAndFixSendToRecycleBinTest()
        {
            var result = existingFile.TryAndFixSendToRecycleBin(out var ex);
            Assert.IsTrue(result);
            Assert.IsNull(ex);
            Assert.IsFalse(File.Exists(initExistingFilePath));
        }
    }
}