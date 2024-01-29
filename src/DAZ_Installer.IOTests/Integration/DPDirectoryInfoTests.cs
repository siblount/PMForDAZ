using DAZ_Installer.IO.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DAZ_Installer.IO.Integration.Tests
{
    [TestClass]
#pragma warning disable CS0618 // This is for testing, as intended.
    public class DPDirectoryInfoTests
    {
        private static DPDirectoryInfo dirWithFile = null!;
        private static DPDirectoryInfo emptyDir = null!;
        private static DPFileInfo file = null!;
        private static string tempDir = Path.Combine(Path.GetTempPath(), "DAZ_Installer.IO.Integration.Tests.DPFileInfoTests");
        private static DPFileSystem defaultFS = new DPFileSystem(new DPFileScopeSettings(Array.Empty<string>(), new[] { tempDir }, false));

        [ClassInitialize]
        public static void ClassSetup(TestContext t)
        {
            Directory.CreateDirectory(tempDir);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Directory.CreateDirectory(tempDir);
            dirWithFile = new DPDirectoryInfo(Path.Combine(tempDir, "dirwithfile"), defaultFS);
            dirWithFile.TryCreate();
            file = new DPFileInfo(Path.Combine(dirWithFile.Path, "file.txt"), defaultFS);
            var success = file.TryCreate(out var stream);
            stream?.Dispose();
            if (!success) throw new Exception("Failed to create file.");
            emptyDir = new DPDirectoryInfo(Path.Combine(tempDir, "empty"), defaultFS);
            emptyDir.TryCreate();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            dirWithFile.TryDelete(true);
            emptyDir.TryDelete(true);
        }

        [TestMethod]
        public void SendToRecycleBinTest()
        {
            var result = dirWithFile.SendToRecycleBin();
            Assert.IsTrue(result);
            Assert.IsFalse(File.Exists(dirWithFile.Path));
        }
        [TestMethod]
        public void TrySendToRecycleBinTest()
        {
            var result = dirWithFile.TrySendToRecycleBin(out var ex);
            Assert.IsTrue(result);
            Assert.IsNull(ex);
            Assert.IsFalse(File.Exists(dirWithFile.Path));
        }
        [TestMethod]
        public void TryAndFixSendToRecycleBinTest()
        {
            var result = dirWithFile.TryAndFixSendToRecycleBin(out var ex);
            Assert.IsTrue(result);
            Assert.IsNull(ex);
            Assert.IsFalse(File.Exists(dirWithFile.Path));
        }
    }
}