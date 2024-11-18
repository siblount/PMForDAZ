using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAZ_Installer.Core;
using System;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Moq;
using DAZ_Installer.IO;
using System.IO.Compression;

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPMetadataReaderTests
    {
        private class TestStream : MemoryStream
        {
            public bool IsDisposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                IsDisposed = true;
                base.Dispose(disposing);
            }
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();
        }
        
        private DPDSXFileMock fileMock1;
        private DPDSXFileMock fileMock2;

        [TestInitialize]
        public void Setup()
        {
            fileMock1 = new();
            fileMock2 = new();
        }

        private struct DPDSXFileMock {
            public Mock<IDPDSXFile> MockDSXFile = new Mock<IDPDSXFile>();
            public Mock<IDPFileInfo> MockFileInfo = new Mock<IDPFileInfo>();
            public TestStream Stream = new TestStream();
            public IDPFileInfo FileInfo => MockFileInfo.Object;
            public IDPDSXFile File => MockDSXFile.Object;

            public DPDSXFileMock() {}
        }

        [TestMethod]
        public void ReadMetadata()
        {
            // Arrange
            fileMock1.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock1.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock1.FileInfo);
            fileMock1.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = fileMock1.Stream;
                                    ex = null;
                                })
                                .Returns(true);
            fileMock2.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock2.FileInfo);
            fileMock2.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock2.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = fileMock2.Stream;
                                    ex = null;
                                })
                                .Returns(true);

            // Act
            DPMetadataReader.Instance.ReadMetadata(new[] { fileMock1.File, fileMock2.File }, CancellationToken.None);

            // Assert
            fileMock1.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Once);
            fileMock2.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Once);
            Assert.IsTrue(fileMock1.Stream.IsDisposed);
            Assert.IsTrue(fileMock2.Stream.IsDisposed);
        }

        [TestMethod]
        public void ReadMetadata_Cancelled()
        {
            // Arrange
            CancellationTokenSource cts = new();
            
            fileMock1.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock1.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock1.FileInfo);
            fileMock1.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = fileMock1.Stream;
                                    cts.Cancel();
                                    ex = null;
                                })
                                .Returns(true);
            fileMock2.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock2.FileInfo);
            fileMock2.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock2.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = fileMock2.Stream;
                                    ex = null;
                                })
                                .Returns(true);

            // Act
            DPMetadataReader.Instance.ReadMetadata(new[] { fileMock1.File, fileMock2.File }, cts.Token);

            // Assert
            fileMock1.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Once);
            fileMock2.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Never);
            Assert.IsTrue(fileMock1.Stream.IsDisposed);
            Assert.IsFalse(fileMock2.Stream.IsDisposed);
            fileMock2.Stream.Dispose();
        }

        [TestMethod]
        public void ReadMetadata_GZipped()
        {
            // Arrange
            fileMock1.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock1.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock1.FileInfo);

            // Create a properly GZipped content
            using (var gzipStream = new GZipStream(fileMock1.Stream, CompressionMode.Compress, true))
            using (var writer = new StreamWriter(gzipStream, Encoding.UTF8))
            {
                writer.Write("success");
            }

            fileMock1.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    fileMock1.Stream.Position = 0; // Reset position to start
                                    s = fileMock1.Stream;
                                    ex = null;
                                })
                                .Returns(true);

            var file1StreamContents = "";
            fileMock1.MockDSXFile.Setup(x => x.CheckContents(It.IsAny<StreamReader>()))
                                .Callback((StreamReader s) => {
                                    file1StreamContents = s.ReadToEnd();
                                    Log.Information("Got contents: {0}", file1StreamContents);
                                });

            fileMock2.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock2.FileInfo);
            fileMock2.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock2.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = fileMock2.Stream;
                                    ex = null;
                                })
                                .Returns(true);

            // Act
            DPMetadataReader.Instance.ReadMetadata(new[] { fileMock1.File, fileMock2.File }, CancellationToken.None);

            // Assert
            fileMock1.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Once);
            fileMock2.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Once);
            Assert.AreEqual("success", file1StreamContents);
            Assert.IsTrue(fileMock1.Stream.IsDisposed);
            Assert.IsTrue(fileMock2.Stream.IsDisposed);
        }

        [TestMethod]
        public void ReadMetadata_FileOpenError_HandlesGracefully()
        {
            // Arrange
            fileMock1.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock1.FileInfo);
            fileMock1.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock1.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = null;
                                    ex = null;
                                })
                                .Returns(false);
            fileMock2.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock2.FileInfo);
            fileMock2.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock2.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = fileMock2.Stream;
                                    ex = null;
                                })
                                .Returns(true);

            // Act
            DPMetadataReader.Instance.ReadMetadata(new[] { fileMock1.File, fileMock2.File }, CancellationToken.None);

            // Assert
            fileMock1.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Never);
            fileMock2.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Once);
            Assert.IsTrue(fileMock2.Stream.IsDisposed);
        }

        [TestMethod]
        public void ReadMetadata_CheckContentsThrowsException()
        {
            // Arrange
            fileMock1.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock1.FileInfo);
            fileMock1.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock1.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = null;
                                    ex = null;
                                })
                                .Returns(true);
            fileMock1.MockDSXFile.Setup(x => x.CheckContents(It.IsAny<StreamReader>())).Throws(new Exception());
            fileMock2.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock2.FileInfo);
            fileMock2.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock2.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = fileMock2.Stream;
                                    ex = null;
                                })
                                .Returns(true);

            // Act
            DPMetadataReader.Instance.ReadMetadata(new[] { fileMock1.File, fileMock2.File }, CancellationToken.None);

            // Assert
            fileMock1.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Never);
            fileMock2.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Once);
            Assert.IsTrue(fileMock2.Stream.IsDisposed);
        }

        [TestMethod]
        public void ReadMetadata_NullStreamDSXFile()
        {
            // Arrange
            fileMock1.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock1.FileInfo);
            fileMock1.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock1.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = null;
                                    ex = null;
                                })
                                .Returns(true);
            fileMock2.MockFileInfo.SetupGet(x => x.Exists).Returns(true);
            fileMock2.MockDSXFile.SetupGet(x => x.FileInfo).Returns(fileMock2.FileInfo);
            fileMock2.MockFileInfo.Setup(f => f.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                                .Callback((out Stream? s, out Exception? ex) => {
                                    s = fileMock2.Stream;
                                    ex = null;
                                })
                                .Returns(true);

            // Act
            DPMetadataReader.Instance.ReadMetadata(new[] { fileMock1.File, fileMock2.File }, CancellationToken.None);

            // Assert
            fileMock1.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Never);
            fileMock2.MockDSXFile.Verify(d => d.CheckContents(It.IsAny<StreamReader>()), Times.Once);
            Assert.IsTrue(fileMock2.Stream.IsDisposed);
        }
    }
}