using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DAZ_Installer.IO.Tests
{
    [TestClass]
    public class PathTransversalExceptionTests
    {
        [DataTestMethod]
        [DataRow("..\\")]

        public void ThrowIfTransversalDetectedTest_Throws(string path) => Assert.ThrowsException<PathTransversalException>(() => PathTransversalException.ThrowIfTransversalDetected(path));

        [DataTestMethod]
        [DataRow("C:\\Windows\\System32\\a.weird.directory\\a.jpg")]
        public void ThrowIfTransversalDetectedTest_NoThrow(string path) => PathTransversalException.ThrowIfTransversalDetected(path);

        [DataTestMethod]
        [DataRow("..\\")]
        public void ThrowIfTransversalDetectedTest_ThrowsNullMsg(string path)
        {
            try
            {
                PathTransversalException.ThrowIfTransversalDetected(path, null);
            }
            catch (PathTransversalException ex)
            {
                Assert.AreEqual($"Path tranversal detected for {path}", ex.Message);
            }
        }

        [DataTestMethod]
        [DataRow("..\\", "i am leg")]
        public void ThrowIfTransversalDetectedTest_ThrowsMsg(string path, string msg)
        {
            try
            {
                PathTransversalException.ThrowIfTransversalDetected(path, msg);
            }
            catch (PathTransversalException ex)
            {
                Assert.AreEqual(msg, ex.Message);
            }
        }

    }
}