using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;
using System.Threading.Tasks;
using Moq;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Serilog.Debugging;
using System.Runtime.CompilerServices;


namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPDSXParserTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration().Enrich.FromLogContext()
                                                  .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                                                  .MinimumLevel.Information()
                                                  .CreateLogger();
        }
        [TestMethod]
        public void DPDSXParserTest()
        {
            var sr = new StreamReader(Stream.Null);
            var parser = new DPDSXParser(sr, Log.Logger, 21);

            Assert.AreEqual(Log.Logger, parser.Logger);
            Assert.AreEqual(21, parser.BufferSize);
        }

        [TestMethod]
        public void GetDSXFile_ElementTest()
        {
            const string content = @"<DAZInstallManifest></DAZInstallManifest>";

            var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            var parser = new DPDSXParser(sr, Log.Logger, content.Length);
            var file = parser.GetDSXFile();

            var manifestNodeList = file.nonSelfClosingElements["DAZInstallManifest"];
            Assert.IsNotNull(manifestNodeList);
            Assert.AreEqual(1, manifestNodeList.Count);
            var manifestNode = manifestNodeList.First();
            Assert.IsNotNull(manifestNode);
            Assert.AreEqual("DAZInstallManifest", manifestNode.TagName);
            Assert.AreEqual(0, manifestNode.Children.Count);
            Assert.AreEqual(0, manifestNode.attributes.Count);
            Assert.AreEqual(0, file.selfClosingElements.Count);
        }

        [TestMethod]
        public void GetDSXFile_ElementWithAttributeTest()
        {
            const string content = @"<DAZInstallManifest VERSION=""0.1""></DAZInstallManifest>";

            var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            var parser = new DPDSXParser(sr, Log.Logger, content.Length);
            var file = parser.GetDSXFile();

            var manifestNodeList = file.nonSelfClosingElements["DAZInstallManifest"];
            Assert.IsNotNull(manifestNodeList);
            Assert.AreEqual(1, manifestNodeList.Count);
            var manifestNode = manifestNodeList.First();
            Assert.IsNotNull(manifestNode);
            Assert.AreEqual("DAZInstallManifest", manifestNode.TagName);
            Assert.AreEqual(0, manifestNode.Children.Count);
            Assert.AreEqual(1, manifestNode.attributes.Count);
            Assert.AreEqual("0.1", manifestNode.attributes["VERSION"]);
            Assert.AreEqual(0, file.selfClosingElements.Count);
            Assert.AreEqual(1, file.nonSelfClosingElements.Count);
        }
        [TestMethod]
        public void GetDSXFile_ElementWithTextEnclosingTest()
        {
            Assert.Inconclusive("This is a known issue. The parser does not support text enclosed within an element.");
            const string content = @"
            <DAZInstallManifest>
                <Element>Test</Element>
            </DAZInstallManifest>";

            var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            var parser = new DPDSXParser(sr, Log.Logger, content.Length);
            var file = parser.GetDSXFile();

            var manifestNodeList = file.nonSelfClosingElements["DAZInstallManifest"];
            Assert.IsNotNull(manifestNodeList);
            Assert.AreEqual(1, manifestNodeList.Count);
            var manifestNode = manifestNodeList.First();
            Assert.IsNotNull(manifestNode);
            Assert.AreEqual("DAZInstallManifest", manifestNode.TagName);
            Assert.AreEqual(1, manifestNode.Children.Count);
            Assert.AreEqual(0, manifestNode.attributes.Count);

            var elementNode = manifestNode.Children.First();
            Assert.AreEqual("Test", elementNode.InnerText.ToString());
            Assert.AreEqual(0, elementNode.Children.Count);
            Assert.AreEqual(0, elementNode.attributes.Count);
            Assert.AreEqual(manifestNode, elementNode.Parent);

            Assert.AreEqual(0, file.selfClosingElements.Count);
            Assert.AreEqual(2, file.nonSelfClosingElements.Count);
        }

        [TestMethod]
        public void GetDSXFile_SelfClosingTest()
        {
            const string content = @"<DAZInstallManifest/>";

            var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            var parser = new DPDSXParser(sr, Log.Logger, content.Length);
            var file = parser.GetDSXFile();

            var manifestNodeList = file.selfClosingElements["DAZInstallManifest"];
            Assert.IsNotNull(manifestNodeList);
            Assert.AreEqual(1, manifestNodeList.Count);
            var manifestNode = manifestNodeList.First();
            Assert.IsNotNull(manifestNode);
            Assert.AreEqual("DAZInstallManifest", manifestNode.TagName);
            Assert.AreEqual(0, manifestNode.Children.Count);
            Assert.AreEqual(0, manifestNode.attributes.Count);
            Assert.AreEqual(1, file.selfClosingElements.Count);
            Assert.AreEqual(0, file.nonSelfClosingElements.Count);
        }

        [TestMethod]
        public void GetDSXFile_MultipleSelfClosingTest()
        {
            const string content = @"<DAZInstallManifest/><DAZInstallManifest/>";

            var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            var parser = new DPDSXParser(sr, Log.Logger, content.Length);
            var file = parser.GetDSXFile();

            var manifestNodeList = file.selfClosingElements["DAZInstallManifest"];
            Assert.IsNotNull(manifestNodeList);
            Assert.AreEqual(2, manifestNodeList.Count);
            var manifestNode = manifestNodeList.First();
            Assert.IsNotNull(manifestNode);
            Assert.AreEqual("DAZInstallManifest", manifestNode.TagName);
            Assert.AreEqual(0, manifestNode.Children.Count);
            Assert.AreEqual(0, manifestNode.attributes.Count);
            Assert.AreEqual(1, file.selfClosingElements.Count);
            Assert.AreEqual(0, file.nonSelfClosingElements.Count);
        }

        [TestMethod]
        public void GetDSXFile_SelfClosingWithAttributesTest()
        {
            const string content = @"<DAZInstallManifest VALUE=""TestVal"" CATEGORY=""Test""/>";

            var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            var parser = new DPDSXParser(sr, Log.Logger, content.Length);
            var file = parser.GetDSXFile();

            var manifestNodeList = file.selfClosingElements["DAZInstallManifest"];
            Assert.IsNotNull(manifestNodeList);
            Assert.AreEqual(1, manifestNodeList.Count);
            var manifestNode = manifestNodeList.First();
            Assert.IsNotNull(manifestNode);
            Assert.AreEqual("DAZInstallManifest", manifestNode.TagName);
            Assert.AreEqual(0, manifestNode.Children.Count);
            CollectionAssert.AreEqual(new[] { "VALUE", "CATEGORY" }, manifestNode.attributes.Keys.ToArray());
            CollectionAssert.AreEqual(new[] { "TestVal", "Test" }, manifestNode.attributes.Values.ToArray());
            Assert.AreEqual(1, file.selfClosingElements.Count);
            Assert.AreEqual(0, file.nonSelfClosingElements.Count);
        }

        [TestMethod]
        public void GetDSXFile_NestedElementsTest()
        {
            const string content = @"
            <Root LEVEL=""1"">
                <Nested1 LEVEL=""2"">
                    <Nested2 LEVEL=""3""/>
                </Nested1>
            </Root>
            ";

            var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            var parser = new DPDSXParser(sr, Log.Logger, content.Length);
            var file = parser.GetDSXFile();

            var manifestNodeList = file.nonSelfClosingElements["Root"];
            Assert.IsNotNull(manifestNodeList);
            Assert.AreEqual(1, manifestNodeList.Count);
            var manifestNode = manifestNodeList.First();
            Assert.IsNotNull(manifestNode);

            // Assert Root
            Assert.AreEqual("Root", manifestNode.TagName);
            Assert.IsFalse(manifestNode.IsSelfClosingElement);
            Assert.AreEqual(1, manifestNode.Children.Count);
            Assert.IsNull(manifestNode.Parent);
            Assert.AreEqual(1, manifestNode.attributes.Count);
            Assert.AreEqual("1", manifestNode.attributes["LEVEL"]);

            // Assert Nested1
            var nested1 = manifestNode.Children.First();
            Assert.IsFalse(nested1.IsSelfClosingElement);
            Assert.AreEqual("Nested1", nested1.TagName);
            Assert.AreEqual(1, nested1.Children.Count);
            Assert.AreEqual(manifestNode, nested1.Parent);
            Assert.AreEqual(1, nested1.attributes.Count);
            Assert.AreEqual("2", nested1.attributes["LEVEL"]);

            // Assert Nested2
            var nested2 = nested1.Children.First();
            Assert.IsTrue(nested2.IsSelfClosingElement);
            Assert.AreEqual("Nested2", nested2.TagName);
            Assert.AreEqual(0, nested2.Children.Count);
            Assert.AreEqual(nested1, nested2.Parent);
            Assert.AreEqual(1, nested2.attributes.Count);
            Assert.AreEqual("3", nested2.attributes["LEVEL"]);

            Assert.AreEqual(1, file.selfClosingElements.Count);
            Assert.AreEqual(2, file.nonSelfClosingElements.Count);
        }

        [TestMethod]
        public void GetDSXFile_CombinationTest()
        {
            const string content = @"
            <DAZInstallManifest VERSION=""0.1"">
             <File TARGET=""Content"" ACTION=""Install"" VALUE=""Content/data/TheRealSolly/test.jpg""/>
             <File TARGET=""Content"" ACTION=""Install"" VALUE=""test.jpg""/>
             <Special TARGET=""Content"" ACTION=""Install"" VALUE=""Something totally/\random that \\// might break""/>
            </DAZInstallManifest>
            ";

            var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(content)));
            var parser = new DPDSXParser(sr, Log.Logger, content.Length);
            var file = parser.GetDSXFile();

            var manifestNodeList = file.nonSelfClosingElements["DAZInstallManifest"];
            Assert.IsNotNull(manifestNodeList);
            Assert.AreEqual(1, manifestNodeList.Count);

            var manifestNode = manifestNodeList.First();
            Assert.IsNotNull(manifestNode);
            Assert.AreEqual("DAZInstallManifest", manifestNode.TagName);
            Assert.AreEqual(3, manifestNode.Children.Count);
            Assert.AreEqual(1, manifestNode.attributes.Count);
            Assert.AreEqual("0.1", manifestNode.attributes["VERSION"]);

            var filesList = file.selfClosingElements["File"];
            Assert.IsNotNull(filesList);
            Assert.AreEqual(2, filesList.Count);

            var file1 = filesList.First();
            Assert.AreEqual("File", file1.TagName);
            Assert.AreEqual(0, file1.Children.Count);
            Assert.AreEqual(3, file1.attributes.Count);
            Assert.AreEqual("Content", file1.attributes["TARGET"]);
            Assert.AreEqual("Install", file1.attributes["ACTION"]);
            Assert.AreEqual("Content/data/TheRealSolly/test.jpg", file1.attributes["VALUE"]);
            Assert.AreEqual(manifestNode, file1.Parent);

            var file2 = filesList.Last();
            Assert.AreEqual("File", file2.TagName);
            Assert.AreEqual(0, file2.Children.Count);
            Assert.AreEqual(3, file2.attributes.Count);
            Assert.AreEqual("Content", file2.attributes["TARGET"]);
            Assert.AreEqual("Install", file2.attributes["ACTION"]);
            Assert.AreEqual("test.jpg", file2.attributes["VALUE"]);
            Assert.AreEqual(manifestNode, file2.Parent);

            var specialList = file.selfClosingElements["Special"];
            Assert.IsNotNull(specialList);
            Assert.AreEqual(1, specialList.Count);
            
            var special = specialList.First();
            Assert.AreEqual("Special", special.TagName);
            Assert.AreEqual(0, special.Children.Count);
            Assert.AreEqual(3, special.attributes.Count);
            Assert.AreEqual("Content", special.attributes["TARGET"]);
            Assert.AreEqual("Install", special.attributes["ACTION"]);
            Assert.AreEqual(@"Something totally/\random that \\// might break", special.attributes["VALUE"]);
            Assert.AreEqual(manifestNode, special.Parent);

            Assert.AreEqual(2, file.selfClosingElements.Count);
            Assert.AreEqual(1, file.nonSelfClosingElements.Count);
        }
    }
}