using System;
using System.Collections.Generic;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using DAZ_Installer.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;

namespace DAZ_Installer.Core.Tests.Integration
{
    [TestClass]
    public class DPMetadataReaderTests
    {
        public static readonly string TempPath = Path.Combine(Path.GetTempPath(), "DAZ_Installer.CoreTests", "Integration", "Test Subjects");
        public static readonly string TestSubjectsPath = Path.Combine(Environment.CurrentDirectory, "Integration", "Test Subjects");
        public static readonly string SupplementPath = Path.Combine(TestSubjectsPath, "ExampleSupplement.dsx");
        public static readonly string GzippedSupplementPath = Path.Combine(TempPath, "ExampleSupplementGzipped.dsx");
        public static readonly DPFileSystem FileSystem = new(new DPFileScopeSettings(new[] { SupplementPath, GzippedSupplementPath }, new[] { TempPath }, false, true));

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Debug()
                        .CreateLogger();
            Directory.CreateDirectory(TempPath);
            File.Copy(SupplementPath, GzippedSupplementPath);
            byte[] fileBytes = File.ReadAllBytes(GzippedSupplementPath);
            using FileStream fileStream = File.OpenWrite(GzippedSupplementPath);
            using GZipStream gzipStream = new(fileStream, CompressionMode.Compress);
            gzipStream.Write(fileBytes, 0, fileBytes.Length);
            gzipStream.Close();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            try
            {
                Directory.Delete(TempPath, true);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to delete temporary directory.");
            }
        }

        [TestMethod]
        public void ReadMetadata()
        {
            var archive = new DPArchive();
            var exampleDsxFile = new DPDSXFile(Path.GetFileName(SupplementPath), archive, null);
            exampleDsxFile.FileInfo = FileSystem.CreateFileInfo(SupplementPath);

            DPMetadataReader.Instance.ReadMetadata(new[] { exampleDsxFile }, CancellationToken.None);

            Assert.IsTrue(exampleDsxFile.ContentChecked);
            Assert.AreEqual("dForce Myles Beard for Genesis 8 Male(s)", archive.ProductInfo.ProductName);
        }

        [TestMethod]
        public void ReadMetadata_Gzipped()
        {
            var archive = new DPArchive();
            var exampleDsxFile = new DPDSXFile(Path.GetFileName(GzippedSupplementPath), archive, null);
            exampleDsxFile.FileInfo = FileSystem.CreateFileInfo(GzippedSupplementPath);
            // Double-check that the contents are gzipped. Otherwise, test is invalid.
            using var stream = exampleDsxFile.FileInfo.OpenRead();
            if (stream.ReadByte() != 0x1F || stream.ReadByte() != 0x8B)
            {
                stream.Seek(0, SeekOrigin.Begin);
                Log.Debug("GzippedSupplementPath got: 0x{0:x} 0x{1:x}", stream.ReadByte(), stream.ReadByte());
                Assert.Inconclusive("ExampleCompression is not gzipped compressed.");
            }
            stream.Close();

            DPMetadataReader.Instance.ReadMetadata(new[] { exampleDsxFile }, CancellationToken.None);

            Assert.IsTrue(exampleDsxFile.ContentChecked);
            Assert.AreEqual("dForce Myles Beard for Genesis 8 Male(s)", archive.ProductInfo.ProductName);
        }
    }
}
