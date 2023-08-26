using DAZ_Installer.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace DAZ_Installer.IO.Tests
{
    [TestClass]
    public class DPIOContextTests
    {
        private static DPIOContext randomCtx = new DPIOContext();

        [TestMethod]
        public void DPIOContextTest()
        {
            Assert.AreSame(DPFileScopeSettings.None, DPIOContext.None.Scope);
            Assert.AreSame(DPFileScopeSettings.All, new DPIOContext().Scope);
            var e = DPFileScopeSettings.CreateUltraStrict(new[] { "john" }, Array.Empty<string>());
            Assert.AreSame(e, new DPIOContext(e).Scope);
        }

        [TestInitialize]
        public void Init()
        {
            randomCtx = new DPIOContext();
        }

        [TestMethod]
        public void CreateDirectoryInfoTest()
        {
            Assert.AreSame(randomCtx.CreateDirectoryInfo("a").Context, randomCtx);
        }

        [TestMethod]
        public void CreateFileInfoTest()
        {
            Assert.AreSame(randomCtx.CreateDirectoryInfo("a.txt").Context, randomCtx);
        }

        [TestMethod]
        public void ToDPFileInfoTest()
        {
            Assert.AreSame(randomCtx.ToDPFileInfo(new FileInfo("a.txt")).Context, randomCtx);
        }

        [TestMethod]
        public void TODPDirectoryInfoTest()
        {
            Assert.AreSame(randomCtx.TODPDirectoryInfo(new DirectoryInfo("a")).Context, randomCtx);
        }

        [TestMethod]
        public void ChangeScopeToTest()
        {
            for (var i = 0; i < 5; i++)
            {
                randomCtx.CreateFileInfo(i.ToString());
            }
            var newScope = DPFileScopeSettings.CreateUltraStrict(new[] { "1", "2" }, new[] { "3", "4" });
            randomCtx.ChangeScopeTo(newScope);
            foreach (var node in randomCtx.GetNodes())
            {
                Assert.AreEqual(randomCtx, node.Context);
                Assert.AreEqual(newScope, node.Context.Scope);
            }
        }
        [TestMethod]
        public void ChangeScopeToTest_Recursive()
        {
            for (var i = 0; i < 5; i++)
            {
                var a = randomCtx.CreateFileInfo(i.ToString());
                _ = a.Directory; // prompt to create a directory.
            }
            var newScope = DPFileScopeSettings.CreateUltraStrict(new[] { "1", "2" }, new[] { "3", "4" });
            randomCtx.ChangeScopeTo(newScope);
            foreach (var node in randomCtx.GetNodes())
            {
                Assert.AreEqual(randomCtx, node.Context);
                Assert.AreEqual(newScope, node.Context.Scope);
            }
        }

        [TestMethod]
        public void ClearTest()
        {
            const byte n = 5;
            var arr = new DPIONodeBase[n];
            for (var i = 0; i < n; i++)
            {
                arr[i] = randomCtx.CreateFileInfo(i.ToString());
            }
            randomCtx.Clear();
            for (var i = 0; i < n; i++)
            {
                Assert.AreSame(arr[i].Context, DPIOContext.None);
            }
            Assert.IsTrue(randomCtx.GetNodes().Count == 0);
        }

        [TestMethod]
        public void RegisterNodeTest()
        {
            randomCtx.CreateFileInfo("Z:/My/Secret/Stash/001/0012g23g34-txrwge.bin");
            Assert.IsTrue(randomCtx.GetNodes().Count == 3);
        }
    }
}