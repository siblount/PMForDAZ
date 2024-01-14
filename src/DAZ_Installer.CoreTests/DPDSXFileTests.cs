using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAZ_Installer.IO.Fakes;

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPDSXFileTests
    {
        internal const string SupplementContent =
        @"<ProductSupplement VERSION=""0.1"">
            <ProductName VALUE=""Test Product""/>
            <InstallTypes VALUE=""Content""/>
            <ProductTags VALUE=""DAZStudio4_5""/>
        </ProductSupplement>";

        internal const string SupportContent =
        @"<?xml version=""1.0"" encoding=""UTF-8""?>
            <ContentDBInstall VERSION=""1.0"">
                <Products>
                    <Product VALUE=""Test Product"">
                        <StoreID VALUE=""DAZ 3D"" />
                        <GlobalID VALUE=""69""/>
                        <ProductToken VALUE=""82114"" />
                        <Artists>
                            <Artist VALUE=""TheRealSolly""/>
                            <Artist VALUE=""Sollybean"" />
                        </Artists>
                    </Product>
                </Products>
            </ContentDBInstall>";

        StreamReader SetupStreamReader(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return new StreamReader(stream);
        }

        [TestMethod]
        public void CheckContentsTest_Supplement()
        {
            var f = new DPDSXFile("doesnt matter", new(), null);
            var a = f.AssociatedArchive;
            using var sr = SetupStreamReader(SupplementContent);

            f.CheckContents(sr);

            Assert.AreEqual("Test Product", a.ProductInfo.ProductName);
            Assert.AreEqual("Test Product", a.ProductName);
        }

        [TestMethod]
        public void CheckContentsTest_Support()
        {
            var f = new DPDSXFile("doesnt matter", new(), null);
            var a = f.AssociatedArchive;
            using var sr = SetupStreamReader(SupportContent);

            f.CheckContents(sr);

            Assert.AreEqual("82114", f.ContentInfo.ID);
            Assert.AreEqual("82114", a.ProductInfo.SKU);
            CollectionAssert.AreEqual(new[] { "TheRealSolly", "Sollybean" }, a.ProductInfo.Authors.ToArray());
            CollectionAssert.AreEqual(new[] { "TheRealSolly", "Sollybean" }, f.ContentInfo.Authors.ToArray());

        }
    }
}