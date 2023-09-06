using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Serilog;
using DAZ_Installer.Core.Extraction;
using Moq;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.IO;

#pragma warning disable 618
namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPProcessorTests
    {
        static IEnumerable<string> DefaultContents => DPProcessorTestHelpers.DefaultContents;
        static readonly DPProcessSettings DefaultProcessSettings = new("A:/", "B:/", InstallOptions.ManifestAndAuto);

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();
        }

        [TestMethod]
        public void ProcessArchiveTest()
        {
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var _, out _, out _, out var fs);
            var p = DPProcessorTestHelpers.SetupProcessor(a, fs.Object, out _, out _);
            var settings = DPProcessorTestHelpers.CreateExtractSettings(DefaultContents, a);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectArchiveProcessed = new() { { a.FileName, DPProcessorTestHelpers.CreateExtractionReport(settings, Enumerable.Empty<string>(), DPProcessorTestHelpers.CalculateExpectedFiles(DefaultContents)) } }
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);

            p.ProcessArchive(a, DefaultProcessSettings);

            DPProcessorTestHelpers.AssertCommon(p);
            //CollectionAssert.Contains(a.ProductInfo.Tags.ToArray(), new[] { "Gentlemen's Library", "TheRealSolly", "solomon1blount@gmail.com", "www.thesolomonchronicles.com", a.FileName });
        }

        [TestMethod]
        public void ProcessArchiveTest_AfterProcess()
        {
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var _, out _, out _, out var fs);
            var p = DPProcessorTestHelpers.SetupProcessor(a, fs.Object, out _, out _);
            var settings = DPProcessorTestHelpers.CreateExtractSettings(DefaultContents, a);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectArchiveProcessed = new() { { a.FileName, DPProcessorTestHelpers.CreateExtractionReport(settings, Enumerable.Empty<string>(), DPProcessorTestHelpers.CalculateExpectedFiles(DefaultContents)) } },
                ExpectedArchiveCount = 2,
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);

            p.ProcessArchive(a, DefaultProcessSettings);
            p.ProcessArchive(a, DefaultProcessSettings);


            DPProcessorTestHelpers.AssertCommon(p, Times.Exactly(2));
            //CollectionAssert.Contains(a.ProductInfo.Tags.ToArray(), new[] { "Gentlemen's Library", "TheRealSolly", "solomon1blount@gmail.com", "www.thesolomonchronicles.com", a.FileName });
        }

        [TestMethod]
        public void ProcessArchiveTest_AfterProcessError()
        {
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var _, out _, out _, out var fs);
            var p = DPProcessorTestHelpers.SetupProcessor(a, fs.Object, out _, out _);
            var settings = DPProcessorTestHelpers.CreateExtractSettings(DefaultContents, a);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectArchiveProcessed = new() { { a.FileName, DPProcessorTestHelpers.CreateExtractionReport(settings, Enumerable.Empty<string>(), DPProcessorTestHelpers.CalculateExpectedFiles(DefaultContents)) } },
                ExpectedArchiveCount = 2,
                ExpectedProcessErrorCount = 1,
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);

            var calledOnce = false;
            var mock = new Mock<FakeDPDriveInfo>(new FakeFileSystem(), "A:/") { CallBase = true }.Object;
            fs.Setup(x => x.CreateDriveInfo(It.IsRegex(@"A:/"))).Returns(() =>
            {
                if (!calledOnce)
                {
                    calledOnce = true;
                    throw new Exception("CreateDrive-a-doo");
                }
                return mock;
            });
            p.ProcessArchive(a, DefaultProcessSettings);
            p.ProcessArchive(a, DefaultProcessSettings);


            DPProcessorTestHelpers.AssertCommon(p, Times.Once());
            //CollectionAssert.Contains(a.ProductInfo.Tags.ToArray(), new[] { "Gentlemen's Library", "TheRealSolly", "solomon1blount@gmail.com", "www.thesolomonchronicles.com", a.FileName });
        }



        [TestMethod]
        [DataRow("null", "null")]
        [DataRow("null", "A:/")]
        [DataRow("", "A:/")]
        [DataRow("", "null")]
        [DataRow("A:/", "null")]

        public void ProcessArchiveTest_InvalidProcessSettings(string? temp, string? dest)
        {
            if (temp == "null") temp = null;
            if (dest == "null") dest = null; 
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var _, out _, out _, out var fs);
            var p = DPProcessorTestHelpers.SetupProcessor(a, fs.Object, out _, out _);
            var settings = DPProcessorTestHelpers.CreateExtractSettings(DefaultContents, a);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectArchiveProcessed = new() { { a.FileName, DPProcessorTestHelpers.CreateExtractionReport(settings, Enumerable.Empty<string>(), DPProcessorTestHelpers.CalculateExpectedFiles(DefaultContents)) } }
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);

            try
            {
                p.ProcessArchive(a, new DPProcessSettings(temp, dest, InstallOptions.Automatic));
                Assert.Fail("Expected exception, got none.");
            } catch { }
        }

        [TestMethod]
        public void ProcessArchiveTest_PeekError()
        {
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var e, out _, out _, out var fs);
            var p = DPProcessorTestHelpers.SetupProcessor(a, fs.Object, out _, out _);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectedProcessErrorCount = 1,
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);
            e.Setup(x => x.Peek(It.IsAny<DPArchive>())).Throws(new Exception("Peek-a-boo"));
            p.ArchiveExit += (_, p) => Assert.IsFalse(p.Processed);
            p.ProcessArchive(a, DefaultProcessSettings);
        }
        [TestMethod]
        public void ProcessArchiveTest_ExtractError()
        {
            Func<DPExtractionReport> extractFunc = () => throw new Exception("Extract-a-doo");
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions with { ExtractFunc = extractFunc }, out var e, out _, out _, out var fs);
            var p = DPProcessorTestHelpers.SetupProcessor(a, fs.Object, out _, out _);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectedProcessErrorCount = 1,
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);
            p.ArchiveExit += (_, p) => Assert.IsFalse(p.Processed);
            p.ProcessArchive(a, DefaultProcessSettings);
        }
        [TestMethod]
        public void ProcessArchiveTest_ExtractToTempError()
        {
            Func<DPExtractionReport> extractFunc = () => throw new Exception("Extract-a-doo");
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions with { ExtractToTempFunc = extractFunc }, out var e, out _, out _, out var fs);
            var p = DPProcessorTestHelpers.SetupProcessor(a, fs.Object, out _, out _);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectedProcessErrorCount = 1,
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);
            p.ArchiveExit += (_, p) => Assert.IsFalse(p.Processed);
            p.ProcessArchive(a, DefaultProcessSettings);
        }
        [TestMethod]
        public void ProcessArchiveTest_OutOfStorage()
        {
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var e, out _, out _, out var fs);
            var p = DPProcessorTestHelpers.SetupProcessor(a, fs.Object, out _, out _);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectedProcessErrorCount = 1,
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);
            p.ArchiveExit += (_, p) => Assert.IsFalse(p.Processed);
            var fakeDriveInfo = new Mock<FakeDPDriveInfo>(fs.Object, "N:/");
            fs.Setup(x => x.CreateDriveInfo(It.IsAny<string>())).Returns(fakeDriveInfo.Object);
            fakeDriveInfo.Object.AvailableFreeSpace = 0;
            p.ProcessArchive(a, DefaultProcessSettings);
        }

        [TestMethod]
        public void ProcessArchiveTest_OutOfStorageFixedButExtractError()
        {
            Func<DPExtractionReport> extractErrorFunc = () => throw new Exception("no u");
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions with { ExtractFunc = extractErrorFunc }, out var e, out _, out _, out var fs);
            var p = DPProcessorTestHelpers.SetupProcessor(a, fs.Object, out _, out _);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectedProcessErrorCount = 1,
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);
            p.ArchiveExit += (_, p) => Assert.IsFalse(p.Processed);
            var fakeDriveInfo = new Mock<FakeDPDriveInfo>(fs.Object, "N:/");
            fakeDriveInfo.Object.AvailableFreeSpace = 0;
            fs.Setup(x => x.CreateDriveInfo(It.IsAny<string>())).Returns(() =>
            {
                if (fakeDriveInfo.Object.AvailableFreeSpace == 0) fakeDriveInfo.SetupProperty(x => x.AvailableFreeSpace, long.MaxValue);
                return fakeDriveInfo.Object;
            });
            p.ProcessArchive(a, DefaultProcessSettings);
        }

        // Invalid Process Settings
        // 
    }
}