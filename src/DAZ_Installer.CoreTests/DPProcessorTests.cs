using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Serilog;
using DAZ_Installer.Core.Extraction;
using Moq;
using DAZ_Installer.IO.Fakes;
using DAZ_Installer.IO;
using DAZ_Installer.Core.Tests.Fakes;
using System.Xml.Xsl;

#pragma warning disable 618
namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPProcessorTests
    {
        static IEnumerable<string> DefaultContents => new[] { "content1", "content2", "content3" };
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
        public async Task ProcessArchiveTest()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            processorMocks.FakeDriveInfo.AvailableFreeSpace = long.MaxValue; // Satisfy DestinationHasEnoughSpace()
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };

            // Setup extractor to raise ExtractProgress event
            mocks.MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>()))
                               .Returns(new DPExtractionReport())
                               .Raises(x => x.ExtractProgress += null, arc, new DPExtractProgressArgs(100, arc, null));
            mocks.MockArchive.Setup(x => x.ExtractContents(It.IsAny<DPExtractSettings>()))
                             .Returns(expectedReport)
                             .Callback(() => mocks.Extractor.Extract(new DPExtractSettings())); // Used to call ExtractionProgress event
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>())).Returns(new DPExtractionReport());
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });

            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;
            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedExtractSettings = new DPExtractSettings() {
                TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                Archive = arc,
                FilesToExtract = new HashSet<IDPFile>(),
                OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
            };
            var expectedProgressArgs = new DPExtractProgressArgs(100, arc, null);

            // Setup DSX file
            var dsxFile = new Mock<FakeDPDSXFile>("a.dsx", null!, mocks.Archive, true) { CallBase = true };
            var dsxFileInfo = Mock.Get(mocks.FakeFileSystem.CreateFileInfo("a.dsx"));
            dsxFile.Object.FileInfo = dsxFileInfo.Object;
            dsxFileInfo.Setup(x => x.Exists).Returns(true);
            dsxFileInfo.Setup(x => x.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                        .Callback((out Stream s, out Exception? e) => {
                                s = new MemoryStream();
                                e = null;
                        }).Returns(true);
            
            var expectedTempExtractSettings = expectedExtractSettings;

            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.PreparingExtraction | ProcessorState.Peeking | ProcessorState.Extracting | ProcessorState.Analyzing;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertExtractionProgress(p, processorTask, expectedProgressArgs),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, true, expectedReport, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
            p.ArchiveCancellationSource.Cancel(); // Cancel afterward to test if CancellationToken is set properly.
            Assert.IsTrue(mocks.Extractor.CancellationToken.IsCancellationRequested, "Cancellation token was not set properly for extractor.");
            mocks.MockArchive.Verify(x => x.PeekContents(It.IsAny<string>()), Times.Once(), "Processor did not call PeekContents");
            DPProcessorTestHelpers.VerifyExtractContentsCalled(false, mocks.MockArchive, expectedExtractSettings, Times.Once());
            DPProcessorTestHelpers.VerifyExtractContentsCalled(true, mocks.MockArchive, expectedTempExtractSettings, Times.Once());
            mocks.MockArchive.VerifySet(x => x.Type = It.IsAny<ArchiveType>(), Times.Once(), "Processor did not set Archive.Type");
            processorMocks.MockTagProvider.Verify(x => x.GetTags(arc, DefaultProcessSettings), Times.Once(), "Processor did not call GetTags on the TagProvider with process settings");
            processorMocks.MockFakeTempDirectoryInfo.Verify(x => x.Delete(true), Times.Never(), "Processor did not attempt to delete temp directory.");
        }

        [TestMethod]
        public async Task ProcessArchiveTest_AfterProcess()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => {
                p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings);
                p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings);
            });

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            processorMocks.FakeDriveInfo.AvailableFreeSpace = long.MaxValue; // Satisfy DestinationHasEnoughSpace()
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };

            // Setup extractor to raise ExtractProgress event
            mocks.MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>()))
                               .Returns(new DPExtractionReport())
                               .Raises(x => x.ExtractProgress += null, arc, new DPExtractProgressArgs(100, arc, null));
            mocks.MockArchive.Setup(x => x.ExtractContents(It.IsAny<DPExtractSettings>()))
                             .Returns(expectedReport)
                             .Callback(() => mocks.Extractor.Extract(new DPExtractSettings())); // Used to call ExtractionProgress event
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>())).Returns(new DPExtractionReport());
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });

            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;
            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedExtractSettings = new DPExtractSettings() {
                TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                Archive = arc,
                FilesToExtract = new HashSet<IDPFile>(),
                OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
            };
            var expectedProgressArgs = new DPExtractProgressArgs(100, arc, null);

            // Setup DSX file
            var dsxFile = new Mock<FakeDPDSXFile>("a.dsx", null!, mocks.Archive, true) { CallBase = true };
            var dsxFileInfo = Mock.Get(mocks.FakeFileSystem.CreateFileInfo("a.dsx"));
            dsxFile.Object.FileInfo = dsxFileInfo.Object;
            dsxFileInfo.Setup(x => x.Exists).Returns(true);
            dsxFileInfo.Setup(x => x.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                        .Callback((out Stream s, out Exception? e) => {
                                s = new MemoryStream();
                                e = null;
                        }).Returns(true);
            
            var expectedTempExtractSettings = expectedExtractSettings;

            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.PreparingExtraction | ProcessorState.Peeking | ProcessorState.Extracting | ProcessorState.Analyzing;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertExtractionProgress(p, processorTask, expectedProgressArgs),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, true, expectedReport, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
            p.ArchiveCancellationSource.Cancel(); // Cancel afterward to test if CancellationToken is set properly.
            Assert.IsTrue(mocks.Extractor.CancellationToken.IsCancellationRequested, "Cancellation token was not set properly for extractor.");
            mocks.MockArchive.Verify(x => x.PeekContents(It.IsAny<string>()), Times.Exactly(2), "Processor did not call PeekContents");
            DPProcessorTestHelpers.VerifyExtractContentsCalled(false, mocks.MockArchive, expectedExtractSettings, Times.Exactly(2));
            DPProcessorTestHelpers.VerifyExtractContentsCalled(true, mocks.MockArchive, expectedTempExtractSettings, Times.Exactly(2));
            mocks.MockArchive.VerifySet(x => x.Type = It.IsAny<ArchiveType>(), Times.Exactly(2), "Processor did not set Archive.Type");
            processorMocks.MockTagProvider.Verify(x => x.GetTags(arc, DefaultProcessSettings), Times.Exactly(2), "Processor did not call GetTags on the TagProvider with process settings");
        }

        [TestMethod]
        public async Task ProcessArchiveTest_AfterProcessError()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => {
                p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings);
                p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings);
            });

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            processorMocks.FakeDriveInfo.AvailableFreeSpace = long.MaxValue; // Satisfy DestinationHasEnoughSpace()
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };

            // Setup extractor to raise ExtractProgress event
            mocks.MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>()))
                               .Returns(new DPExtractionReport())
                               .Raises(x => x.ExtractProgress += null, arc, new DPExtractProgressArgs(100, arc, null));
            var callCount = 0;
            mocks.MockArchive.Setup(x => x.ExtractContents(It.IsAny<DPExtractSettings>()))
                             .Returns(() => {
                                    if (callCount++ == 0) throw new Exception("Extract-a-doo");
                                    mocks.Extractor.Extract(new DPExtractSettings()); // Raise ExtractProgress event.
                                    return expectedReport;
                             });
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>())).Returns(new DPExtractionReport());
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });

            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;
            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedExtractSettings = new DPExtractSettings()
            {
                TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                Archive = arc,
                FilesToExtract = new HashSet<IDPFile>(),
                OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
            };
            var expectedProgressArgs = new DPExtractProgressArgs(100, arc, null);

            // Setup DSX file
            var dsxFile = new Mock<FakeDPDSXFile>("a.dsx", null!, mocks.Archive, true) { CallBase = true };
            var dsxFileInfo = Mock.Get(mocks.FakeFileSystem.CreateFileInfo("a.dsx"));
            dsxFile.Object.FileInfo = dsxFileInfo.Object;
            dsxFileInfo.Setup(x => x.Exists).Returns(true);
            dsxFileInfo.Setup(x => x.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                        .Callback((out Stream s, out Exception? e) => {
                            s = new MemoryStream();
                            e = null;
                        }).Returns(true);

            var expectedTempExtractSettings = expectedExtractSettings;

            var expectedProcessorStates1 = ProcessorState.Starting | ProcessorState.PreparingExtraction;
            var expectedProcessorStates2 = ProcessorState.Starting | ProcessorState.PreparingExtraction | ProcessorState.Peeking | ProcessorState.Extracting | ProcessorState.Analyzing;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertAnyState(p, arc, expectedProcessorStates1, processorTask),
                DPProcessorTestHelpers.AssertAnyState(p, arc, expectedProcessorStates2, processorTask),
                DPProcessorTestHelpers.AssertExtractionProgress(p, processorTask, expectedProgressArgs),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, true, expectedReport, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
            p.ArchiveCancellationSource.Cancel(); // Cancel afterward to test if CancellationToken is set properly.
            Assert.IsTrue(mocks.Extractor.CancellationToken.IsCancellationRequested, "Cancellation token was not set properly for extractor.");
            mocks.MockArchive.Verify(x => x.PeekContents(It.IsAny<string>()), Times.Exactly(2), "Processor did not call PeekContents");
            DPProcessorTestHelpers.VerifyExtractContentsCalled(false, mocks.MockArchive, expectedExtractSettings, Times.Exactly(2));
            DPProcessorTestHelpers.VerifyExtractContentsCalled(true, mocks.MockArchive, expectedTempExtractSettings, Times.Exactly(2));
            mocks.MockArchive.VerifySet(x => x.Type = It.IsAny<ArchiveType>(), Times.Once(), "Processor did not set Archive.Type");
            processorMocks.MockTagProvider.Verify(x => x.GetTags(arc, DefaultProcessSettings), Times.Once(), "Processor did not call GetTags on the TagProvider with process settings");
        }

        [TestMethod]
        public async Task ProcessArchiveTest_CancelOnArchiveEnter()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => {
                p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings);
                p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings);
            });

            var expectedProcessorStates = ProcessorState.Idle;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
            };
            p.ArchiveEnter += (_, __) =>
            {
                p.CancelProcessing();
                processorTask.AddAssertion(() => Assert.IsTrue(p.ArchiveCancellationSource.IsCancellationRequested, "Archive cancellation source reports not cancelled."));
                processorTask.AddAssertion(() => Assert.IsTrue(p.CancellationTokenSource.IsCancellationRequested, "Processor cancellation source reports not cancelled."));
            };

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
            mocks.MockArchive.Verify(x => x.PeekContents(It.IsAny<string>()), Times.Never(), "Processor did not call PeekContents");
        }

        [TestMethod]
        [DataRow("null", "null")]
        [DataRow("null", "A:/")]
        [DataRow("", "A:/")]
        [DataRow("", "null")]
        [DataRow("A:/", "null")]
        [DataRow("A:/", "B:/", true)]

        public void ProcessArchiveTest_InvalidProcessSettings(string? temp, string? dest, bool nullForceDests = false)
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var fftd = nullForceDests ? null : new Dictionary<IDPFile, string>();

            if (temp == "null")
                temp = null;
            if (dest == "null")
                dest = null;
            try
            {
                p.ProcessArchive(mocks.FakeDPFileInfo.Path, new DPProcessSettings(temp, dest, InstallOptions.Automatic, null, null, true) { ForceFileToDest = fftd });
            }
            catch (Exception e) when (e is not ArgumentNullException && e is not ArgumentException)
            {
                Assert.Fail($"Exception is not of type ArgumentNullException or ArgumentException: {e}");
            }
            catch (Exception) { return; }
            Assert.Fail("No exception was thrown.");
        }

        [TestMethod]
        public async Task ProcessArchiveTest_PeekError()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            processorMocks.FakeDriveInfo.AvailableFreeSpace = long.MaxValue; // Satisfy DestinationHasEnoughSpace()
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() {} };
            var expectedException = new Exception("Peek-a-doo");
            // Setup archive to throw exception when PeekContents is called.
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Throws(expectedException);

            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;
            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedExtractSettings = new DPExtractSettings() {
                TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                Archive = arc,
                FilesToExtract = new HashSet<IDPFile>(),
                OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
            };
            var expectedProgressArgs = new DPExtractProgressArgs(100, arc, null);
            var expectedTempExtractSettings = expectedExtractSettings;
            var expectedErrorArgs = new DPProcessorErrorArgs(expectedException, "Failed to peek into archive");
            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.Peeking;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
                DPProcessorTestHelpers.AssertProcessorError(p, processorTask, expectedErrorArgs)
            };

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
        }

        [TestMethod]
        public async Task ProcessArchiveTest_ExtractError()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            processorMocks.FakeDriveInfo.AvailableFreeSpace = long.MaxValue; // Satisfy DestinationHasEnoughSpace()
            var expectedException = new Exception("Extract-a-doo");

            // Setup archive to throw exception when ExtractContents is called.
            mocks.MockArchive.Setup(x => x.ExtractContents(It.IsAny<DPExtractSettings>()))
                             .Throws(expectedException);
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>())).Returns(new DPExtractionReport());
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });

            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;
            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedExtractSettings = new DPExtractSettings() {
                TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                Archive = arc,
                FilesToExtract = new HashSet<IDPFile>(),
                OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
            };
            
            var expectedTempExtractSettings = expectedExtractSettings;
            var expectedErrorArgs = new DPProcessorErrorArgs(expectedException, "Failed to extract contents for archive");
            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.PreparingExtraction | ProcessorState.Peeking | ProcessorState.Extracting;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
                DPProcessorTestHelpers.AssertProcessorError(p, processorTask, expectedErrorArgs),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
        }
        [TestMethod]
        public async Task ProcessArchiveTest_ExtractToTempError()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            processorMocks.FakeDriveInfo.AvailableFreeSpace = long.MaxValue; // Satisfy DestinationHasEnoughSpace()
            var expectedException = new Exception("Extract-a-doo");

            // Setup archive to throw exception when ExtractContentsToTemp is called.
            mocks.MockArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>())).Throws(expectedException);
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });

            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;
            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedExtractSettings = new DPExtractSettings()
            {
                TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                Archive = arc,
                FilesToExtract = new HashSet<IDPFile>(),
                OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
            };

            var expectedTempExtractSettings = expectedExtractSettings;
            var expectedErrorArgs = new DPProcessorErrorArgs(expectedException, "Failed to prepare for extraction");
            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.PreparingExtraction | ProcessorState.Peeking;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
                DPProcessorTestHelpers.AssertProcessorError(p, processorTask, expectedErrorArgs),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
        }

        [TestMethod]
        public async Task ProcessArchiveTest_OutOfStorage()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };
        
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });
            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;

            // Fake out of storage
            processorMocks.FakeDriveInfo.AvailableFreeSpace = 0; // Requirement #1 for triggering out of storage.
            mocks.Archive.TrueArchiveSize = ulong.MaxValue; // Requirement #2 for triggering out of storage.

            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.Peeking;
            var expectedErrorArgs = new DPProcessorErrorArgs(null, "Destination does not have enough space.") { Continuable = true };
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertProcessorError(p, processorTask, expectedErrorArgs),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            p.ProcessError += (_, __) => p.CancelProcessing();

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
        }

        [TestMethod]
        public async Task ProcessArchiveTest_OutOfStorage_NoEventHandler()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };
        
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });
            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;

            // Fake out of storage
            processorMocks.FakeDriveInfo.AvailableFreeSpace = 0; // Requirement #1 for triggering out of storage.
            mocks.Archive.TrueArchiveSize = ulong.MaxValue; // Requirement #2 for triggering out of storage.

            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.Peeking;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };
            var bufferLogger = new BufferSink();
            p.Logger = new LoggerConfiguration().Enrich.FromLogContext()
                                                       .WriteTo.Sink(bufferLogger)
                                                       .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                                                       .CreateLogger();

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
            var bufferLog = bufferLogger.ToString();
            StringAssert.Contains(bufferLog, "Destination does not have enough space and there is no event handler for ProcessError", $"BufferLog: {bufferLog}");
        }

        [TestMethod]
        public async Task ProcessArchiveTest_OutOfStorage_Fixed()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };

            // Setup extractor to raise ExtractProgress event
            mocks.MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>()))
                               .Returns(new DPExtractionReport())
                               .Raises(x => x.ExtractProgress += null, arc, new DPExtractProgressArgs(100, arc, null));
            mocks.MockArchive.Setup(x => x.ExtractContents(It.IsAny<DPExtractSettings>()))
                             .Returns(expectedReport)
                             .Callback(() => mocks.Extractor.Extract(new DPExtractSettings())); // Used to call ExtractionProgress event
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>())).Returns(new DPExtractionReport());
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });

            // Fake out of storage
            processorMocks.FakeDriveInfo.AvailableFreeSpace = 0; // Requirement #1 for triggering out of storage.
            mocks.Archive.TrueArchiveSize = 500; // Requirement #2 for triggering out of storage.

            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;
            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedExtractSettings = new DPExtractSettings() {
                TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                Archive = arc,
                FilesToExtract = new HashSet<IDPFile>(),
                OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
            };
            var expectedProgressArgs = new DPExtractProgressArgs(100, arc, null);

            // Setup DSX file
            var dsxFile = new Mock<FakeDPDSXFile>("a.dsx", null!, mocks.Archive, true) { CallBase = true };
            var dsxFileInfo = Mock.Get(mocks.FakeFileSystem.CreateFileInfo("a.dsx"));
            dsxFile.Object.FileInfo = dsxFileInfo.Object;
            dsxFileInfo.Setup(x => x.Exists).Returns(true);
            dsxFileInfo.Setup(x => x.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                        .Callback((out Stream s, out Exception? e) => {
                                s = new MemoryStream();
                                e = null;
                        }).Returns(true);
            
            var expectedTempExtractSettings = expectedExtractSettings;
            var expectedProcessorErrorArgs = new DPProcessorErrorArgs(null, "Destination does not have enough space.") { Continuable = true };
            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.PreparingExtraction | ProcessorState.Peeking | ProcessorState.Extracting | ProcessorState.Analyzing;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertExtractionProgress(p, processorTask, expectedProgressArgs),
                DPProcessorTestHelpers.AssertProcessorError(p, processorTask, expectedProcessorErrorArgs),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, true, expectedReport, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            p.ProcessError += (_, __) => processorMocks.FakeDriveInfo.AvailableFreeSpace = long.MaxValue;

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
            p.ArchiveCancellationSource.Cancel(); // Cancel afterward to test if CancellationToken is set properly.
            Assert.IsTrue(mocks.Extractor.CancellationToken.IsCancellationRequested, "Cancellation token was not set properly for extractor.");
            mocks.MockArchive.Verify(x => x.PeekContents(It.IsAny<string>()), Times.Once(), "Processor did not call PeekContents");
            DPProcessorTestHelpers.VerifyExtractContentsCalled(false, mocks.MockArchive, expectedExtractSettings, Times.Once());
            DPProcessorTestHelpers.VerifyExtractContentsCalled(true, mocks.MockArchive, expectedTempExtractSettings, Times.Once());
            mocks.MockArchive.VerifySet(x => x.Type = It.IsAny<ArchiveType>(), Times.Once(), "Processor did not set Archive.Type");
            processorMocks.MockTagProvider.Verify(x => x.GetTags(arc, DefaultProcessSettings), Times.Once(), "Processor did not call GetTags on the TagProvider with process settings");
            processorMocks.MockFakeTempDirectoryInfo.Verify(x => x.Delete(true), Times.Never(), "Processor did not attempt to delete temp directory.");
        }

        [TestMethod]
        public async Task ProcessArchiveTest_TempOutOfStorage()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };
        
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });
            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;

            // Fake out of storage
            processorMocks.MockFakeDriveInfo.SetupSequence(x => x.AvailableFreeSpace).Returns(9999999999999).Returns(0); // Requirement #1 for triggering out of storage.
            mocks.MockArchive.SetupSequence(x => x.TrueArchiveSize).Returns(0).Returns(ulong.MaxValue); // Requirement #2 for triggering out of storage.

            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(It.IsAny<string>()))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.Peeking | ProcessorState.PreparingExtraction;
            var expectedErrorArgs = new DPProcessorErrorArgs(null, "Temp location does not have enough space") { Continuable = true };
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertProcessorError(p, processorTask, expectedErrorArgs),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            p.ProcessError += (_, __) => p.CancelProcessing();

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
        }

        [TestMethod]
        public async Task ProcessArchiveTest_TempOutOfStorage_Fixed()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };

            // Setup extractor to raise ExtractProgress event
            mocks.MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>()))
                               .Returns(new DPExtractionReport())
                               .Raises(x => x.ExtractProgress += null, arc, new DPExtractProgressArgs(100, arc, null));
            mocks.MockArchive.Setup(x => x.ExtractContents(It.IsAny<DPExtractSettings>()))
                             .Returns(expectedReport)
                             .Callback(() => mocks.Extractor.Extract(new DPExtractSettings())); // Used to call ExtractionProgress event
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>())).Returns(new DPExtractionReport());
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });

            // Fake out of storage
            processorMocks.MockFakeDriveInfo.SetupSequence(x => x.AvailableFreeSpace).Returns(9999999999999).Returns(0).Returns(0).Returns(0); // Requirement #1 for triggering out of storage.
            mocks.Archive.TrueArchiveSize = 50;

            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;
            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(It.IsAny<string>()))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedExtractSettings = new DPExtractSettings() {
                TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                Archive = arc,
                FilesToExtract = new HashSet<IDPFile>(),
                OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
            };
            var expectedProgressArgs = new DPExtractProgressArgs(100, arc, null);

            // Setup DSX file
            var dsxFile = new Mock<FakeDPDSXFile>("a.dsx", null!, mocks.Archive, true) { CallBase = true };
            var dsxFileInfo = Mock.Get(mocks.FakeFileSystem.CreateFileInfo("a.dsx"));
            dsxFile.Object.FileInfo = dsxFileInfo.Object;
            dsxFileInfo.Setup(x => x.Exists).Returns(true);
            dsxFileInfo.Setup(x => x.TryAndFixOpenRead(out It.Ref<Stream>.IsAny!, out It.Ref<Exception>.IsAny!))
                        .Callback((out Stream s, out Exception? e) => {
                                s = new MemoryStream();
                                e = null;
                        }).Returns(true);
            
            var expectedTempExtractSettings = expectedExtractSettings;
            var expectedProcessorErrorArgs = new DPProcessorErrorArgs(null, "Temp location does not have enough space") { Continuable = true };
            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.PreparingExtraction | ProcessorState.Peeking | ProcessorState.Extracting | ProcessorState.Analyzing;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertExtractionProgress(p, processorTask, expectedProgressArgs),
                DPProcessorTestHelpers.AssertProcessorError(p, processorTask, expectedProcessorErrorArgs),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, true, expectedReport, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            p.ProcessError += (_, __) => processorMocks.MockFakeDriveInfo.SetupSequence(x => x.AvailableFreeSpace).Returns(9999999999999);

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
            p.ArchiveCancellationSource.Cancel(); // Cancel afterward to test if CancellationToken is set properly.
            Assert.IsTrue(mocks.Extractor.CancellationToken.IsCancellationRequested, "Cancellation token was not set properly for extractor.");
            mocks.MockArchive.Verify(x => x.PeekContents(It.IsAny<string>()), Times.Once(), "Processor did not call PeekContents");
            DPProcessorTestHelpers.VerifyExtractContentsCalled(false, mocks.MockArchive, expectedExtractSettings, Times.Once());
            DPProcessorTestHelpers.VerifyExtractContentsCalled(true, mocks.MockArchive, expectedTempExtractSettings, Times.Once());
            mocks.MockArchive.VerifySet(x => x.Type = It.IsAny<ArchiveType>(), Times.Once(), "Processor did not set Archive.Type");
            processorMocks.MockTagProvider.Verify(x => x.GetTags(arc, DefaultProcessSettings), Times.Once(), "Processor did not call GetTags on the TagProvider with process settings");
            processorMocks.MockFakeTempDirectoryInfo.Verify(x => x.Delete(true), Times.Once(), "Processor did not attempt to delete temp directory.");
        }

        [TestMethod]
        public async Task ProcessArchiveTest_TempOutOfStorage_NoEventHandler()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));

            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                    .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };
        
            // Setup mock archive to not call base.
            mocks.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });
            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;

            // Fake out of storage
            processorMocks.MockFakeDriveInfo.SetupSequence(x => x.AvailableFreeSpace).Returns(9999999999999).Returns(0).Returns(0); // Requirement #1 for triggering out of storage.
            mocks.Archive.TrueArchiveSize = 50; // Requirement #2 for triggering out of storage.

            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks.MockFakeFileSystem.Setup(x => x.CreateDriveInfo(It.IsAny<string>()))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.Peeking | ProcessorState.PreparingExtraction;
            var expectedErrorArgs = new DPProcessorErrorArgs(null, "Temp location does not have enough space") { Continuable = true };
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };
            var bufferLogger = new BufferSink();
            p.Logger = new LoggerConfiguration().Enrich.FromLogContext()
                                                       .WriteTo.Sink(bufferLogger)
                                                       .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                                                       .CreateLogger();

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
            StringAssert.Contains(bufferLogger.ToString(), "Temp location does not have enough space and there is no event handler for ProcessError");
        }

        [TestMethod]
        public async Task ProcessArchiveOrderTest()
        {
            // Create multiple archives
            var archives = new List<FakeDPArchive>();
            var mocks = new List<DPProcessorTestHelpers.ArchiveMocks>();
            for (var i = 0; i < 5; i++)
            {
                var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var arcMocks);
                arc.Path = $"Z:/{(char)('a' + i)}.rar";
                arcMocks.MockFakeDPFileInfo.Setup(x => x.Path).Returns(arc.Path);
                archives.Add(arc);
                mocks.Add(arcMocks);
            }

            // Setup chain
            archives[0].Subarchives.AddRange(new[] { archives[1], archives[2] });
            archives[1].Subarchives.Add(archives[3]);
            archives[2].Subarchives.Add(archives[4]);

            var po = new DPProcessorTestHelpers.ProcessorOptions() 
            { 
                Archive = archives[0], 
                FileSystem = mocks[0].FakeFileSystem, 
                Settings = DefaultProcessSettings 
            };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks[0].FakeDPFileInfo.Path, DefaultProcessSettings));

            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };

            // Setup mock archives
            foreach (var mock in mocks)
            {   
                // Setup extractor to raise ExtractProgress event
                mock.MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>()))
                                .Returns(new DPExtractionReport())
                                .Raises(x => x.ExtractProgress += null, mock.Archive, new DPExtractProgressArgs(100, mock.Archive, null));
                
                mock.MockArchive.Setup(x => x.ExtractContents(It.IsAny<DPExtractSettings>()))
                                .Returns(expectedReport)
                                .Callback(() => mock.Extractor.Extract(new DPExtractSettings()));
                
                mock.MockArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>())).Returns(new DPExtractionReport());
                mock.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });
            }

            // Setup Processor mocks.
            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                        .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            processorMocks.FakeDriveInfo.AvailableFreeSpace = long.MaxValue; // Satisfy DestinationHasEnoughSpace
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false; // Do not call base, otherwise, will run issues with Create()

            // Setup file system to return specific temp directory info
            // NOTE: Processor uses mocks[0].MockFakeFileSystem.
            mocks[0].MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks[0].MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.PreparingExtraction | ProcessorState.Peeking | ProcessorState.Extracting | ProcessorState.Analyzing;
            var expectedOrder = new Queue<IDPArchive>(new[] { archives[0], archives[2], archives[4], archives[1], archives[3] });
            var assertTasks = new List<Task>
            {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, archives.ToArray()),
                DPProcessorTestHelpers.AssertArchiveProcessOrder(p, processorTask, expectedOrder),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, true, expectedReport, archives.ToArray()),
            };

            foreach (var arc in archives)
            {
                assertTasks.Add(DPProcessorTestHelpers.AssertState(p, archives[0], expectedProcessorStates, processorTask));
                assertTasks.Add(DPProcessorTestHelpers.AssertAnyExtractionProgress(p, processorTask, archives.Select(x => new DPExtractProgressArgs(100, x, null)).ToArray()));
            }

            assertTasks.Add(DPProcessorTestHelpers.AssertFinished(p, processorTask));

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();

            // Verify the order of processing
            for (byte i = 0; i < archives.Count; i++)
            {
                var arc = archives[i];
                var expectedExtractSettings = new DPExtractSettings() {
                    TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                    Archive = arc,
                    FilesToExtract = new HashSet<IDPFile>(),
                    OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
                };
                mocks[i].MockArchive.Verify(x => x.PeekContents(It.IsAny<string>()), Times.Once(), $"Processor did not call PeekContents for archive {i}");
                DPProcessorTestHelpers.VerifyExtractContentsCalled(false, mocks[i].MockArchive, expectedExtractSettings, Times.Once());
                DPProcessorTestHelpers.VerifyExtractContentsCalled(true, mocks[i].MockArchive, expectedExtractSettings, Times.Once());
                mocks[i].MockArchive.VerifySet(x => x.Type = It.IsAny<ArchiveType>(), Times.Once(), $"Processor did not set Archive.Type for archive {i}");
                processorMocks.MockTagProvider.Verify(x => x.GetTags(arc, DefaultProcessSettings), Times.Once(), $"Processor did not call GetTags on the TagProvider with process settings for archive {i}");
            }

            processorMocks.MockFakeTempDirectoryInfo.Verify(x => x.Delete(true), Times.Never(), "Processor attempted to delete temp directory.");
        }

        [TestMethod]
        public async Task ProcessArchiveTest_ArchiveExtractorIsNull()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = DefaultProcessSettings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings));
            arc.Extractor = null;

            // Do not call base for FakeTempDirectoryInfo otherwise will run into issues with Create().
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false;
            // Setup file system to return specific temp directory info.
            mocks.MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);

            var expectedErrorArgs = new DPProcessorErrorArgs(null, "Unable to process archive. Potentially not an archive or archive is corrupted.");
            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.Peeking;
            var assertTasks = new Task[] {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, arc),
                DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, arc),
                DPProcessorTestHelpers.AssertProcessorError(p, processorTask, expectedErrorArgs),
                DPProcessorTestHelpers.AssertFinished(p, processorTask),
            };

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();
        }

        [TestMethod]
        public void TempLocationTest()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var settings = new DPProcessSettings("T:/", "D:/", InstallOptions.Automatic);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = settings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);

            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, settings));
            p.ArchiveEnter += (p, e) =>
            {
                p.CancelProcessing();
                processorTask.AddAssertion(() => Assert.AreEqual(Path.Combine("T:/", @"DazProductInstaller\"), p.TempLocation));
            };

            processorTask.RunSynchronously();

            processorTask.Assert();
        }

        [TestMethod]
        public void DestinationPathTest()
        {
            var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
            var settings = new DPProcessSettings("T:/", "D:/", InstallOptions.Automatic);
            var po = new DPProcessorTestHelpers.ProcessorOptions() { Archive = arc, FileSystem = mocks.FakeFileSystem, Settings = settings };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);

            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks.FakeDPFileInfo.Path, settings));
            p.ArchiveEnter += (p, e) =>
            {
                p.CancelProcessing();
                processorTask.AddAssertion(() => Assert.AreEqual(settings.DestinationPath, p.DestinationPath));
            };

            processorTask.RunSynchronously();

            processorTask.Assert();
        }

        [TestMethod]
        public void DefaultDependenciesTest() {
            var p = new DPProcessor();
            Assert.AreSame(p.ParentArchiveFactory, DPParentArchiveFactory.Instance);
            Assert.AreSame(p.DestinationDeterminer, DPDestinationDeterminer.Singleton);
            Assert.AreSame(p.TagProvider, DPTagProvider.Singleton);
            Assert.IsInstanceOfType(p.FileSystem, typeof(DPFileSystem));
            Assert.IsInstanceOfType(p.Logger, typeof(Serilog.Core.Logger));
        }

        [TestMethod]
        public async Task ProcessArchiveTest_ErrorneousSubarchive()
        {
            // Create multiple archives
            var archives = new List<FakeDPArchive>();
            var mocks = new List<DPProcessorTestHelpers.ArchiveMocks>();
            for (var i = 0; i < 3; i++)
            {
                var arc = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var arcMocks);
                arcMocks.MockFakeDPFileInfo.Setup(x => x.Path).Returns(() => arc.Path);
                archives.Add(arc);
                mocks.Add(arcMocks);
            }

            // Setup chain
            archives[0].Subarchives.AddRange(new[] { archives[1], archives[2] });
            archives[0].Path = "Z:/root.rar";
            archives[1].Path = "Z:/error.rar";
            archives[2].Path = "Z:/good.rar";

            var po = new DPProcessorTestHelpers.ProcessorOptions() 
            { 
                Archive = archives[0], 
                FileSystem = mocks[0].FakeFileSystem, 
                Settings = DefaultProcessSettings 
            };
            var p = DPProcessorTestHelpers.SetupProcessor(po, out var processorMocks);
            var processorTask = new DPProcessorTestHelpers.AssertableTask(() => p.ProcessArchive(mocks[0].FakeDPFileInfo.Path, DefaultProcessSettings));

            var expectedReport = new DPExtractionReport() { ExtractedFiles = new List<IDPFile>() { new FakeDPFile() } };

            // Setup mock archives
            foreach (var mock in mocks)
            {   
                // Setup extractor to raise ExtractProgress event
                mock.MockExtractor.Setup(x => x.Extract(It.IsAny<DPExtractSettings>()))
                                .Returns(new DPExtractionReport())
                                .Raises(x => x.ExtractProgress += null, mock.Archive, new DPExtractProgressArgs(100, mock.Archive, null));
                
                mock.MockArchive.Setup(x => x.ExtractContents(It.IsAny<DPExtractSettings>()))
                                .Returns(expectedReport)
                                .Callback(() => mock.Extractor.Extract(new DPExtractSettings()));
                
                mock.MockArchive.Setup(x => x.ExtractContentsToTemp(It.IsAny<DPExtractSettings>())).Returns(new DPExtractionReport());
                if (mock.Archive.Path == "Z:/error.rar")
                    mock.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Throws(new Exception("Errorneous archive"));
                else
                    mock.MockArchive.Setup(x => x.PeekContents(It.IsAny<string>())).Callback(() => { });
            }

            // Setup Processor mocks.
            processorMocks.MockDestinationDeterminer.Setup(x => x.DetermineDestinations(It.IsAny<IDPArchive>(), It.IsAny<DPProcessSettings>()))
                                                        .Returns(new HashSet<IDPFile>()); // Used to set DPExtractSettings.FilesToExtract
            processorMocks.FakeDriveInfo.AvailableFreeSpace = long.MaxValue; // Satisfy DestinationHasEnoughSpace
            processorMocks.MockFakeTempDirectoryInfo.CallBase = false; // Do not call base, otherwise, will run issues with Create()

            // Setup file system to return specific temp directory info
            // NOTE: Processor uses mocks[0].MockFakeFileSystem.
            mocks[0].MockFakeFileSystem.Setup(x => x.CreateDirectoryInfo(Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\")))
                                    .Returns(processorMocks.FakeTempDirectoryInfo);
            mocks[0].MockFakeFileSystem.Setup(x => x.CreateDriveInfo(DefaultProcessSettings.DestinationPath))
                                    .Returns(processorMocks.FakeDriveInfo);

            var expectedProcessorStates = ProcessorState.Starting | ProcessorState.PreparingExtraction | ProcessorState.Peeking | ProcessorState.Extracting | ProcessorState.Analyzing;
            var expectedSuccessArchives = new[] { archives[0], archives[2] };
            var assertTasks = new List<Task>
            {
                DPProcessorTestHelpers.AssertArchiveEnter(p, processorTask, archives.ToArray()),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, true, expectedReport, expectedSuccessArchives),
                DPProcessorTestHelpers.AssertAnyArchiveExit(p, processorTask, false, null, archives[1]),
            };

            foreach (var arc in archives)
            {
                if (arc.Path.Contains("Z:/error.rar")) continue;
                assertTasks.Add(DPProcessorTestHelpers.AssertState(p, arc, expectedProcessorStates, processorTask));
                assertTasks.Add(DPProcessorTestHelpers.AssertAnyExtractionProgress(p, processorTask, expectedSuccessArchives.Select(x => new DPExtractProgressArgs(100, x, null)).ToArray()));
            }

            assertTasks.Add(DPProcessorTestHelpers.AssertFinished(p, processorTask));

            processorTask.RunSynchronously();
            await Task.WhenAll(assertTasks);

            processorTask.Assert();

            // Verify the order of processing
            for (byte i = 0; i < archives.Count; i++)
            {
                if (i == 1) continue; // skip error archive
                var arc = archives[i];
                var expectedExtractSettings = new DPExtractSettings() {
                    TempPath = Path.Combine(DefaultProcessSettings.TempPath, @"DazProductInstaller\"),
                    Archive = arc,
                    FilesToExtract = new HashSet<IDPFile>(),
                    OverwriteFiles = DefaultProcessSettings.OverwriteFiles,
                };
                mocks[i].MockArchive.Verify(x => x.PeekContents(It.IsAny<string>()), Times.Once(), $"Processor did not call PeekContents for archive {i}");
                DPProcessorTestHelpers.VerifyExtractContentsCalled(false, mocks[i].MockArchive, expectedExtractSettings, Times.Once());
                DPProcessorTestHelpers.VerifyExtractContentsCalled(true, mocks[i].MockArchive, expectedExtractSettings, Times.Once());
                mocks[i].MockArchive.VerifySet(x => x.Type = It.IsAny<ArchiveType>(), Times.Once(), $"Processor did not set Archive.Type for archive {i}");
                processorMocks.MockTagProvider.Verify(x => x.GetTags(arc, DefaultProcessSettings), Times.Once(), $"Processor did not call GetTags on the TagProvider with process settings for archive {i}");
            }

            processorMocks.MockFakeTempDirectoryInfo.Verify(x => x.Delete(true), Times.Never(), "Processor attempted to delete temp directory.");
        }

        // [TestMethod]
        // public void StateChangedTest()
        // {
        //     var a = DPProcessorTestHelpers.NewMockedArchive(DPProcessorTestHelpers.DefaultMockOptions, out var mocks);
        //     var p = DPProcessorTestHelpers.SetupProcessor(a, mocks.FakeFileSystem, in mocks, out var processorMocks);
        //     var settings = DPProcessorTestHelpers.CreateExtractSettings(DefaultContents, a);



        //     p.ProcessArchive(mocks.FakeDPFileInfo.Path, DefaultProcessSettings);
        // }
    }
}