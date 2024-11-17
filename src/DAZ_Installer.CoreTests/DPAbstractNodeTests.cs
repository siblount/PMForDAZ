using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using DAZ_Installer.IO;

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPAbstractNodeTests
    {
        class AbstractNodeTestClass : DPAbstractNode
        {
            public override ILogger Logger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            protected override void UpdateParent(IDPFolder? parent) => throw new NotImplementedException();

            internal AbstractNodeTestClass(string path, DPArchive? arc) : base(path, arc) { }
        }
        [TestMethod]
        [DataRow("a.png", "png")]
        [DataRow("Content/file.rar", "rar")]
        [DataRow("Content/multi.dot.ext", "ext")]
        [DataRow("Content/multi", "")]
        [DataRow("", "")]
        public void GetExtensionTest(string path, string expected)
        {
            Assert.AreEqual(expected, DPAbstractNode.GetExtension(path));
        }

        [TestMethod]
        public void DPAbstractNodeTest()
        {
            var a = new DPArchive();
            var f = new AbstractNodeTestClass("path", a);
            Assert.AreEqual(a, f.AssociatedArchive);
            Assert.AreEqual("path", f.Path);
        }

        [TestMethod]
        public void DPAbstractNode_ThrowsOnNullPathTest()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AbstractNodeTestClass(null, null));
        }

        [TestMethod]
        public void NormalizedPathTest()
        {
            var f = new AbstractNodeTestClass("Content\\file.rar", null);
            Assert.AreEqual(PathHelper.NormalizePath("Content\\file.rar"), f.NormalizedPath);
        }

        [TestMethod]
        public void FileNameTest()
        {
            var f = new AbstractNodeTestClass("Content\\file.rar", null);
            Assert.AreEqual(PathHelper.GetFileName("Content\\file.rar"), f.FileName);
        }

        [TestMethod]
        public void ExtTest()
        {
            var f = new AbstractNodeTestClass("Content\\file.rar", null);
            Assert.AreEqual(DPAbstractNode.GetExtension("Content\\file.rar"), f.Ext);
        }

        [TestMethod]
        public void ParentTest()
        {
            var f = new AbstractNodeTestClass("Content\\file.rar", null);
            Assert.IsNull(f.Parent);
            Assert.ThrowsException<NotImplementedException>(() => f.Parent = null);
        }
    }
}