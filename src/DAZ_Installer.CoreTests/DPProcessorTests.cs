using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Serilog;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO.Fakes;
using NSubstitute.Extensions;

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
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var _, out _, out _);
            var p = DPProcessorTestHelpers.SetupProcessor(a, null);
            var settings = DPProcessorTestHelpers.CreateExtractSettings(DefaultContents, a);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectArchiveProcessed = new() { { a.FileName, DPProcessorTestHelpers.CreateExtractionReport(settings, Enumerable.Empty<string>(), DPProcessorTestHelpers.CalculateExpectedFiles(DefaultContents)) } }
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);

            p.ProcessArchive(a, DefaultProcessSettings);
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
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var _, out _, out _);
            var p = DPProcessorTestHelpers.SetupProcessor(a, null);
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
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var e, out _, out _);
            var p = DPProcessorTestHelpers.SetupProcessor(a, null);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectedProcessErrorCount = 1,
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);
            e.When(x => x.Peek(Arg.Any<DPArchive>())).Throw(new Exception("Peek-a-boo"));
            p.ArchiveExit += (_, p) => Assert.IsFalse(p.Processed);
            p.ProcessArchive(a, DefaultProcessSettings);
        }
        [TestMethod]
        public void ProcessArchiveTest_ExtractError()
        {
            Func<DPExtractionReport> extractFunc = () => throw new Exception("Extract-a-doo");
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions with { ExtractFunc = extractFunc }, out var e, out _, out _);
            var p = DPProcessorTestHelpers.SetupProcessor(a, null);
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
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions with { ExtractToTempFunc = extractFunc }, out var e, out _, out _);
            var p = DPProcessorTestHelpers.SetupProcessor(a, null);
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
            var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var e, out _, out _);
            var ctxFactory = Substitute.ForPartsOf<MockedFakeDPIOContextFactory>();
            var p = DPProcessorTestHelpers.SetupProcessor(a, ctxFactory);
            var ao = new DPProcessorTestHelpers.AssertOptions()
            {
                ExpectedProcessErrorCount = 1,
            };
            DPProcessorTestHelpers.AttachCommonEventHandlers(p, ao);
            p.ArchiveExit += (_, p) => Assert.IsFalse(p.Processed);
            ctxFactory.WhenForAnyArgs(x => x.CreateContext(default, default)).DoNotCallBase();
            var fakedIOContext = new MockedDPIOContext();
            fakedIOContext.availableFreeSpace = fakedIOContext.availableTotalSpace = 0;
            p.ContextFactory.CreateContext(default, default).ReturnsForAnyArgs(fakedIOContext);
            p.ProcessArchive(a, DefaultProcessSettings);
        }
        // Invalid Process Settings
        // 
    }
}