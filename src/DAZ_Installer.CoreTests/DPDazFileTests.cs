using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPDazFileTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();
        }

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
        public void ReadContentsTest()
        {
            // Arrange
            var f = new DPDazFile("doesnt matter", new DPArchive(), null);
            const string DSFContents =
            @"{
                ""file_version"" : ""0.6.0.0"",
                ""asset_info"" : {
                    ""id"" : ""/data/data.dsf"",
                    ""type"" : ""prop"",
                    ""contributor"" : {
                        ""author"" : ""TheRealSolly"",
                        ""email"" : ""solomon1blount@gmail.com"",
                        ""website"" : ""www.thesolomonchronicles.com""
                    },
                    ""revision"" : ""1.0"",
                    ""modified"" : ""2020-12-06T00:04:11Z""
                }
            }";

            // Act
            using var sr = SetupStreamReader(DSFContents);
            f.ReadContents(sr);

            // Assert
            Assert.AreEqual("TheRealSolly", f.ContentInfo.Authors[0]);
            Assert.AreEqual(ContentType.Prop, f.ContentInfo.ContentType);
            Assert.AreEqual("www.thesolomonchronicles.com", f.ContentInfo.Website);
            Assert.AreEqual("solomon1blount@gmail.com", f.ContentInfo.Email);
        }
    }
}
