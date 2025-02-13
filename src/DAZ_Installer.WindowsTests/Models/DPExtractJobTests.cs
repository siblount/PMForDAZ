using DAZ_Installer.UI;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Microsoft.VisualBasic.FileIO;
using Moq;
using Serilog;
using DAZ_Installer.Core;
using DAZ_Installer.IO;
using DAZ_Installer.Database;
using DAZ_Installer.Windows.Tests;
using DAZ_Installer.Core.Tests.Fakes;
using System.Windows.Forms;
using DAZ_Installer.Core.Extraction;
using DAZ_Installer.IO.Fakes;

namespace DAZ_Installer.Windows.DP.Tests
{
    [TestClass]
    [Obsolete]
    public class DPExtractJobTests
    {
        public Mock<IExtractView> MockExtractView { get; set; } = null!;
        public Mock<IProgressCombo> MockProgressCombo { get; set; } = null!;
        public Mock<IDPProcessor> MockProcessor { get; set; } = null!;
        public Mock<FakeFileSystem> MockFileSystem { get; set; } = null!;
        public Mock<IMessageBoxProvider> MockMessageBoxProvider { get; set; } = null!;
        public Mock<IDPRecordManager> MockRecordManager { get; set; } = null!;
        public Mock<IDPDatabase> MockDatabase { get; set; } = null!;
        public IExtractView ExtractView => MockExtractView.Object;
        public IProgressCombo ProgressCombo => MockProgressCombo.Object;
        public IDPProcessor Processor => MockProcessor.Object;
        public FakeFileSystem FileSystem => MockFileSystem.Object;
        public IMessageBoxProvider MessageBox => MockMessageBoxProvider.Object;
        public IDPRecordManager RecordManager => MockRecordManager.Object;
        public IDPDatabase Database => MockDatabase.Object;

        [ClassInitialize]
        public static void ClassInitialize(TestContext _)
        {
            Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
                        .WriteTo.Sink(new MSTestLoggerSink(SerilogLoggerConstants.LoggerTemplate, MSTestLogger.LogMessage))
                        .MinimumLevel.Information()
                        .CreateLogger();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            MockExtractView = new Mock<IExtractView>();
            MockProgressCombo = new Mock<IProgressCombo>();
            MockProcessor = new Mock<IDPProcessor>();
            MockFileSystem = new Mock<FakeFileSystem>() { CallBase = true };
            MockMessageBoxProvider = new Mock<IMessageBoxProvider>();
            MockRecordManager = new Mock<IDPRecordManager>();
            MockDatabase = new Mock<IDPDatabase>();
        }

        private DPExtractJob NewExtractJob(IEnumerable<string> files) {
            return new DPExtractJob(files, ExtractView, ProgressCombo, Database, RecordManager) 
            {
                Processor = Processor,
                FileSystem = FileSystem,
                MessageBoxProvider = MessageBox,
            };
        }

        private Mock<FakeDPArchive> NewFakeDPArchive(string path, bool parent)
        {
            var arc = new Mock<FakeDPArchive>(path, null!, null!) { CallBase = true };
            if (parent)
            {
                var fi = new Mock<FakeFileInfo>(path) { CallBase = true };
                var dfi = new Mock<FakeDPFileInfo>(fi.Object, FileSystem, null!) { CallBase = true };
                arc.Object.FileInfo = dfi.Object;
            }
            return arc;
        }

        [TestMethod]
        public void DPExtractJobTest()
        {
            string[] files = ["U:/foo", "U:/bar"];

            var job = NewExtractJob(files);

            CollectionAssert.AreEquivalent(files, job.InitialFilesToProcess);
            Assert.IsNull(job.TaskJob);
        }

        [TestMethod]
        public async Task DoJobTest_RunsImmediately()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var processingCompleted = new TaskCompletionSource<bool>();
            MockProgressCombo.Setup(x => x.StartProgress()).Callback(() => {
                processingCompleted.SetResult(true);
                Thread.Sleep(50);
            });

            var task = job.DoJob();

            var started = await Task.WhenAny(processingCompleted.Task, Task.Delay(500));
            Assert.AreEqual(started, processingCompleted.Task);
            Assert.AreEqual(TaskStatus.Running, task.Status);
        }

        [TestMethod]
        public async Task DoJobTest_WaitsOne()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var job2 = NewExtractJob(files);
            using var waitSlim = new ManualResetEventSlim(false);

            MockProgressCombo.Setup(x => x.StartProgress()).Callback(() => {
                waitSlim.Wait();
                throw new Exception("done");
            });

            var task = job.DoJob();
            var task2 = job.DoJob();

            if (task2.Status is not (TaskStatus.WaitingForActivation or TaskStatus.WaitingToRun))
            {
                Assert.Fail($"Expected either WaitingForAction or WaitingToRun, got: {task2.Status}");
            }
            waitSlim.Set();

            await Task.WhenAll(task, task2);
        }

        [TestMethod]
        public async Task DoJobTest_Cancels()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var job2 = NewExtractJob(files);
            using var waitSlim = new ManualResetEventSlim(false);

            MockProgressCombo.Setup(x => x.StartProgress()).Callback(() => {
                waitSlim.Wait();
                throw new Exception("done");
            });

            var task = job.DoJob();
            var task2 = job.DoJob();

            if (task2.Status is not (TaskStatus.WaitingForActivation or TaskStatus.WaitingToRun))
            {
                Assert.Fail($"Expected either WaitingForAction or WaitingToRun, got: {task2.Status}");
            }
            waitSlim.Set();

            await Task.WhenAll(task, task2);
        }

        [TestMethod]
        public async Task DoJobTest_ProcessArchivesAsync()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var expectedSettings = new DPProcessSettings
            {
                ContentFolders = DPSettings.CurrentSettingsObject.CommonContentFolderNames,
                ContentRedirectFolders = DPSettings.CurrentSettingsObject.FolderRedirects,
                DestinationPath = DPSettings.CurrentSettingsObject.DestinationPath,
                TempPath = DPSettings.CurrentSettingsObject.TempDir,
                InstallOption = DPSettings.CurrentSettingsObject.HandleInstallation,
                OverwriteFiles = DPSettings.CurrentSettingsObject.OverwriteFiles == SettingOptions.Yes ||
                                DPSettings.CurrentSettingsObject.OverwriteFiles == SettingOptions.Prompt,
                ForceFileToDest = [],
            };
            IDPArchive? processorCurrentArchive = null;
            ProcessorState processorState = ProcessorState.Idle;
            MockProcessor.SetupGet(x => x.State).Returns(() => processorState);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(() => processorCurrentArchive);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                var mockArchive = NewFakeDPArchive(path, true);
                processorCurrentArchive = mockArchive.Object;
                MockProcessor.Raise(x => x.ArchiveEnter += null, Processor, new DPArchiveEnterArgs(mockArchive.Object));
                ReadOnlySpan<ProcessorState> states = [ProcessorState.Starting, ProcessorState.Peeking, ProcessorState.Analyzing, ProcessorState.PreparingExtraction, ProcessorState.Extracting];
                foreach (var state in states) {
                    processorState = state;
                    MockProcessor.Raise(x => x.StateChanged += null);
                }
                var report = new DPExtractionReport() { ErroredFiles = [], ExtractedFiles = [], Settings = new DPExtractSettings() };
                MockProcessor.Raise(x => x.ExtractProgress += null, Processor, new DPExtractProgressArgs(100, mockArchive.Object, null));
                MockProcessor.Raise(x => x.MoveProgress += null, Processor, new DPExtractProgressArgs(100, mockArchive.Object, null));
                MockProcessor.Raise(x => x.ArchiveExit += null, Processor, new DPArchiveExitArgs(mockArchive.Object, report, true));
            });
            MockProgressCombo.SetupGet(x => x.Token).Returns(CancellationToken.None);

            await job.DoJob();

            MockProgressCombo.Verify(x => x.StartProgress(), Times.Once());
            MockProgressCombo.Verify(x => x.EndProgress(), Times.Once());
            MockProgressCombo.Verify(x => x.SetProgress(50), Times.Once());
            MockProgressCombo.Verify(x => x.SetProgress(100), Times.Once());
            MockProgressCombo.Verify(x => x.SetText("Finished processing archives"));
            MockProgressCombo.Verify(x => x.SetText("Processing archive 1/2: foo...(0%)"));
            MockProgressCombo.Verify(x => x.SetText("Processing archive 2/2: bar...(50%)"));
            MockExtractView.Verify(x => x.AddToQueue(job), Times.Once());
            MockExtractView.Verify(x => x.OnCreatingRecords(It.IsAny<IDPArchive>()), Times.Exactly(2));
            MockExtractView.Verify(x => x.OnProcessorStateUpdate(Processor), Times.Exactly(5*2)); // Called 5 times per archive.
            MockExtractView.Verify(x => x.OnExtractionProgressUpdate(Processor, It.IsAny<DPExtractProgressArgs>()), Times.Exactly(2)); // Called once per archive
            MockExtractView.Verify(x => x.OnMoveProgressUpdate(Processor, It.IsAny<DPExtractProgressArgs>()), Times.Exactly(2)); // Called once per archive
            MockProcessor.Verify(x => x.ProcessArchive("U:/foo", It.Is(expectedSettings, DPProcessSettingsComparer.Instance)));
            MockProcessor.Verify(x => x.ProcessArchive("U:/bar", It.Is(expectedSettings, DPProcessSettingsComparer.Instance)));
            MockRecordManager.Verify(x => x.CreateAndAddRecord(It.IsAny<DPExtractionReport>(), It.IsAny<DPSettings>()));

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Pending, DPArchiveStatus.Processing, DPArchiveStatus.Completed);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.Pending, DPArchiveStatus.Processing, DPArchiveStatus.Completed);

            MockProcessor.VerifyRemove(x => x.ArchiveEnter -= It.IsAny<DPProcessorEventHandler<DPArchiveEnterArgs>>());
            MockProcessor.VerifyRemove(x => x.ArchiveExit -= It.IsAny<DPProcessorEventHandler<DPArchiveExitArgs>>());
            MockProcessor.VerifyRemove(x => x.ProcessError -= It.IsAny<DPProcessorEventHandler<DPProcessorErrorArgs>>());
            MockProcessor.VerifyRemove(x => x.ExtractProgress -= It.IsAny<DPProcessorEventHandler<DPExtractProgressArgs>>());
            MockProcessor.VerifyRemove(x => x.MoveProgress -= It.IsAny<DPProcessorEventHandler<DPExtractProgressArgs>>());
            MockProcessor.VerifyRemove(x => x.StateChanged -= It.IsAny<Action>());
        }

        [TestMethod]
        public async Task DoJobTest_CancelledBeforeProcessingSetsCancelStatus()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var expectedSettings = new DPProcessSettings
            {
                ContentFolders = DPSettings.CurrentSettingsObject.CommonContentFolderNames,
                ContentRedirectFolders = DPSettings.CurrentSettingsObject.FolderRedirects,
                DestinationPath = DPSettings.CurrentSettingsObject.DestinationPath,
                TempPath = DPSettings.CurrentSettingsObject.TempDir,
                InstallOption = DPSettings.CurrentSettingsObject.HandleInstallation,
                OverwriteFiles = DPSettings.CurrentSettingsObject.OverwriteFiles == SettingOptions.Yes ||
                                DPSettings.CurrentSettingsObject.OverwriteFiles == SettingOptions.Prompt,
                ForceFileToDest = [],
            };
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockExtractView.Setup(x => x.AddToQueue(It.IsAny<DPExtractJob>()))
                           .Callback(cts.Cancel);

            await job.DoJob();

            MockProgressCombo.Verify(x => x.StartProgress(), Times.Once());
            MockProgressCombo.Verify(x => x.EndProgress(), Times.Once());
            MockExtractView.Verify(x => x.AddToQueue(It.IsAny<DPExtractJob>()), Times.Once());
            MockProgressCombo.Verify(x => x.SetProgress(It.Is<int>(x => x != 100)), Times.Never());
            MockProgressCombo.Verify(x => x.SetProgress(100), Times.Once());
            MockProcessor.Verify(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>()), Times.Never());

            MockProcessor.VerifyRemove(x => x.ArchiveEnter -= It.IsAny<DPProcessorEventHandler<DPArchiveEnterArgs>>());
            MockProcessor.VerifyRemove(x => x.ArchiveExit -= It.IsAny<DPProcessorEventHandler<DPArchiveExitArgs>>());
            MockProcessor.VerifyRemove(x => x.ProcessError -= It.IsAny<DPProcessorEventHandler<DPProcessorErrorArgs>>());
            MockProcessor.VerifyRemove(x => x.ExtractProgress -= It.IsAny<DPProcessorEventHandler<DPExtractProgressArgs>>());
            MockProcessor.VerifyRemove(x => x.MoveProgress -= It.IsAny<DPProcessorEventHandler<DPExtractProgressArgs>>());
            MockProcessor.VerifyRemove(x => x.StateChanged -= It.IsAny<Action>());
        }

        [TestMethod]
        public async Task StateChanged_SetsToProcessing()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(NewFakeDPArchive("U:/foo", true).Object);
            MockProcessor.SetupGet(x => x.State).Returns(ProcessorState.PreparingExtraction);
            MockProcessor.Setup(x => x.ProcessArchive("U:/foo", It.IsAny<DPProcessSettings>())).Callback(() =>
            {
                MockProcessor.Raise(x => x.StateChanged += null);
            });

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Processing);
            MockExtractView.Verify(x => x.OnProcessorStateUpdate(Processor), Times.Once());
        }

        [TestMethod]
        public async Task StateChanged_UpdatesExtractView()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(NewFakeDPArchive("U:/foo", true).Object);
            MockProcessor.SetupGet(x => x.State).Returns(ProcessorState.Idle);
            MockProcessor.Setup(x => x.ProcessArchive("U:/foo", It.IsAny<DPProcessSettings>())).Callback(() =>
            {
                MockProcessor.Raise(x => x.StateChanged += null);
            });

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Pending);
            MockExtractView.Verify(x => x.OnExtractJobStatusUpdate(job, It.IsAny<DPArchiveInfo>()), Times.Once());
            MockExtractView.Verify(x => x.OnProcessorStateUpdate(Processor), Times.Once());
        }

        [TestMethod]
        public async Task StateChanged_CancelsCurrentArchive()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            IDPArchive? processorCurrentArchive = null!;

            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(() => processorCurrentArchive);
            MockProcessor.SetupGet(x => x.State).Returns(ProcessorState.PreparingExtraction);
            MockProcessor.Setup(x => x.ProcessArchive("U:/foo", It.IsAny<DPProcessSettings>())).Callback(() =>
            {
                job.SkipArchive("U:/foo");
                processorCurrentArchive = NewFakeDPArchive("U:/foo", true).Object;
                MockProcessor.Raise(x => x.StateChanged += null);
            });

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Processing, DPArchiveStatus.CancellationRequested);
            MockExtractView.Verify(x => x.OnProcessorStateUpdate(Processor), Times.Once());
            MockProcessor.Verify(x => x.CancelCurrentArchive(), Times.Once());
        }

        [TestMethod]
        public void StateChanged_CurrentArchiveNull()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(null as IDPArchive);

            MockExtractView.Verify(x => x.OnProcessorStateUpdate(Processor), Times.Never());
        }

        [TestMethod]
        public void StateChanged_FileInfoNull()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            var arc = NewFakeDPArchive("U:/foo", true).Object;
            arc.FileInfo = null;
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(arc);
            MockProcessor.SetupGet(x => x.State).Returns(ProcessorState.PreparingExtraction);

            MockExtractView.Verify(x => x.OnProcessorStateUpdate(Processor), Times.Never());
        }

        [TestMethod]
        public async Task ArchiveEnter_UpdatesStatus()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                var mockArchive = NewFakeDPArchive(path, true);
                MockProcessor.Raise(x => x.ArchiveEnter += null, new DPArchiveEnterArgs(mockArchive.Object));
            });
            MockDatabase.Setup(x => x.ContainsArchive(It.IsAny<string>(), null)).ReturnsAsync(false);

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Pending);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.Pending);
        }

        [TestMethod]
        public async Task ArchiveEnter_AddsNestedArchive()
        {
            string[] files = ["U:/foo"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                var mockArchive = NewFakeDPArchive("U:/foo", true);
                var nestedArchive = NewFakeDPArchive("bar", true);
                MockProcessor.Raise(x => x.ArchiveEnter += null, Processor, new DPArchiveEnterArgs(mockArchive.Object));
                MockProcessor.Raise(x => x.ArchiveEnter += null, Processor, new DPArchiveEnterArgs(nestedArchive.Object));
            });
            MockDatabase.Setup(x => x.ContainsArchive(It.IsAny<string>(), null)).ReturnsAsync(false);

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Pending);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "bar", DPArchiveStatus.Pending);
        }

        [TestMethod]
        public async Task ArchiveEnter_ExistsButContinue()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                var mockArchive = NewFakeDPArchive(path, true);
                MockProcessor.Raise(x => x.ArchiveEnter += null, Processor, new DPArchiveEnterArgs(mockArchive.Object));
            });
            MockMessageBoxProvider.Setup(x => x.Show(It.IsAny<string>(), It.IsAny<string>())).Returns(DialogResult.Yes);
            MockDatabase.Setup(x => x.ContainsArchive(It.IsAny<string>(), null)).ReturnsAsync(true);

            await job.DoJob();

            MockMessageBoxProvider.Verify();
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Pending);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.Pending);
        }

        [TestMethod]
        public async Task ArchiveEnter_ExistsButCancelJob()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                var mockArchive = NewFakeDPArchive(path, true);
                MockProcessor.Raise(x => x.ArchiveEnter += null, Processor, new DPArchiveEnterArgs(mockArchive.Object));
            });
            MockMessageBoxProvider.Setup(x => x.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question)).Returns(DialogResult.Cancel);
            MockDatabase.Setup(x => x.ContainsArchive(It.IsAny<string>(), null)).ReturnsAsync(null as bool?);

            await job.DoJob();

            MockMessageBoxProvider.Verify();
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Pending, DPArchiveStatus.CancellationRequested);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.CancellationRequested);
            MockProcessor.Verify(x => x.CancelProcessing(), Times.Once());
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ArchiveExit_UpdatesStatus_Completed_DeleteSource(bool delete)
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                job.UserSettings!.PermDeleteSource = SettingOptions.Yes;
                job.UserSettings!.DeleteAction = delete ? RecycleOption.DeletePermanently : RecycleOption.SendToRecycleBin;
                var mockArchive = NewFakeDPArchive(path, true);
                var report = new DPExtractionReport() { ErroredFiles = [], ExtractedFiles = [], Settings = new DPExtractSettings() };
                MockProcessor.Raise(x => x.ArchiveExit += null, Processor, new DPArchiveExitArgs(mockArchive.Object, report, true));
            });
            var fi = FileSystem.CreateFileInfo("doesnt-matter");
            MockFileSystem.Setup(x => x.CreateFileInfo(It.IsAny<string>())).Returns(fi);

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Completed);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.Completed);
            MockExtractView.Verify(x => x.OnCreatingRecords(It.IsAny<IDPArchive>()), Times.Exactly(2));

            if (delete) {
                Mock.Get(fi).Verify(x => x.TryAndFixDelete(out It.Ref<Exception>.IsAny!), Times.Exactly(2));
            } else {
                Mock.Get(fi).Verify(x => x.TryAndFixSendToRecycleBin(out It.Ref<Exception>.IsAny!), Times.Exactly(2));
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ArchiveExit_UpdatesStatus_Completed_DeleteSource_Prompt(bool delete)
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                var mockArchive = NewFakeDPArchive(path, true);
                var report = new DPExtractionReport() { ErroredFiles = [], ExtractedFiles = [], Settings = new DPExtractSettings() };
                job.UserSettings!.PermDeleteSource = SettingOptions.Prompt;
                job.UserSettings!.DeleteAction = delete ? RecycleOption.DeletePermanently : RecycleOption.SendToRecycleBin;
                MockProcessor.Raise(x => x.ArchiveExit += null, Processor, new DPArchiveExitArgs(mockArchive.Object, report, true));
            });
            var fi = FileSystem.CreateFileInfo("doesnt-matter");
            MockFileSystem.Setup(x => x.CreateFileInfo(It.IsAny<string>())).Returns(fi);
            MockMessageBoxProvider.SetReturnsDefault(DialogResult.Yes);

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Completed);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.Completed);
            MockExtractView.Verify(x => x.OnCreatingRecords(It.IsAny<IDPArchive>()), Times.Exactly(2));

            if (delete) {
                Mock.Get(fi).Verify(x => x.TryAndFixDelete(out It.Ref<Exception>.IsAny!), Times.Exactly(2));
            } else {
                Mock.Get(fi).Verify(x => x.TryAndFixSendToRecycleBin(out It.Ref<Exception>.IsAny!), Times.Exactly(2));
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task ArchiveExit_UpdatesStatus_Completed_DeleteSource_Prompt_NullFileInfo(bool delete)
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                var mockArchive = NewFakeDPArchive(path, true);
                var report = new DPExtractionReport() { ErroredFiles = [], ExtractedFiles = [], Settings = new DPExtractSettings() };
                job.UserSettings!.PermDeleteSource = SettingOptions.Yes;
                job.UserSettings!.DeleteAction = delete ? RecycleOption.DeletePermanently : RecycleOption.SendToRecycleBin;
                MockProcessor.Raise(x => x.ArchiveExit += null, Processor, new DPArchiveExitArgs(mockArchive.Object, report, true));
            });
            var fi = FileSystem.CreateFileInfo("doesnt-matter");
            MockFileSystem.Setup(x => x.CreateFileInfo(It.IsAny<string>())).Returns(fi);
            MockMessageBoxProvider.SetReturnsDefault(DialogResult.Yes);
            MockExtractView.Setup(x => x.OnCreatingRecords(It.IsAny<IDPArchive>())).Callback((IDPArchive x) => {
                x.FileInfo = null;
            });

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Completed);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.Completed);
            MockExtractView.Verify(x => x.OnCreatingRecords(It.IsAny<IDPArchive>()), Times.Exactly(2));

            if (delete) {
                Mock.Get(fi).Verify(x => x.TryAndFixDelete(out It.Ref<Exception>.IsAny!), Times.Never());
            } else {
                Mock.Get(fi).Verify(x => x.TryAndFixSendToRecycleBin(out It.Ref<Exception>.IsAny!), Times.Never());
            }
        }

        [TestMethod]
        public async Task ArchiveExit_UpdatesStatus_Completed_DeleteSource_NullFileInfo()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                var mockArchive = NewFakeDPArchive(path, true);
                var report = new DPExtractionReport() { ErroredFiles = [], ExtractedFiles = [], Settings = new DPExtractSettings() };
                job.UserSettings!.PermDeleteSource = SettingOptions.Yes;
                MockProcessor.Raise(x => x.ArchiveExit += null, Processor, new DPArchiveExitArgs(mockArchive.Object, report, true));
            });
            var fi = FileSystem.CreateFileInfo("doesnt-matter");
            MockFileSystem.Setup(x => x.CreateFileInfo(It.IsAny<string>())).Returns(fi);
            MockMessageBoxProvider.SetReturnsDefault(DialogResult.Yes);
            MockExtractView.Setup(x => x.OnCreatingRecords(It.IsAny<IDPArchive>())).Callback((IDPArchive x) => {
                x.FileInfo = null;
            });

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Completed);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.Completed);
            MockExtractView.Verify(x => x.OnCreatingRecords(It.IsAny<IDPArchive>()), Times.Exactly(2));
            Mock.Get(fi).Verify(x => x.TryAndFixDelete(out It.Ref<Exception>.IsAny!), Times.Never());
            Mock.Get(fi).Verify(x => x.TryAndFixSendToRecycleBin(out It.Ref<Exception>.IsAny!), Times.Never());
        }

        [TestMethod]
        public async Task ProcessorErrorOnArchiveUpdatesStatus()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var cts = new CancellationTokenSource();
            MockProgressCombo.SetupGet(x => x.Token).Returns(cts.Token);
            var archiveMock1 = new Mock<FakeDPArchive>("U:/foo") { CallBase = true };
            var archiveMock2 = new Mock<FakeDPArchive>("U:/foo") { CallBase = true };
            IDPArchive? processorCurrentArchive = null;
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(() => processorCurrentArchive);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings _) => {
                var mockArchive = NewFakeDPArchive(path, true);
                processorCurrentArchive = mockArchive.Object; // Setup Current Processor before raising the event.
                MockProcessor.Raise(x => x.ArchiveEnter += null, new DPArchiveEnterArgs(processorCurrentArchive));
                if (path == "U:/foo") {
                    MockProcessor.Raise(x => x.ProcessError += null, new DPProcessorErrorArgs(null, "booga"));
                }
            });

            await job.DoJob();

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.Pending);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.Pending);
            MockExtractView.Verify(x => x.OnExtractJobStatusUpdate(It.IsAny<DPExtractJob>(), It.Is<DPArchiveInfo>(x => x.FilePath == "U:/foo" && x.Errors.Count == 1)), Times.Once());
        }

        [TestMethod]
        public void CancelJobTest()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            MockProcessor.Setup(x => x.CancelProcessing()).Callback(() => {
                MockProcessor.Raise(x => x.ProcessError += null, Processor, new DPProcessorErrorArgs(null, null)); // Used to check status
            });

            job.CancelJob();

            MockProcessor.Verify(x => x.CancelProcessing(), Times.Once());
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.CancellationRequested);
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.CancellationRequested);
        }

        [TestMethod]
        public void CancelCurrentArchiveTest()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            MockProcessor.Setup(x => x.CancelProcessing()).Callback(() => {
                MockProcessor.Raise(x => x.ProcessError += null, Processor, new DPProcessorErrorArgs(null, null)); // Used to check status
            });
            var currentArc = NewFakeDPArchive("U:/foo", true);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(currentArc.Object);

            job.CancelCurrentArchive();

            MockProcessor.Verify(x => x.CancelCurrentArchive(), Times.Once());
            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/foo", DPArchiveStatus.CancellationRequested);
        }

        [TestMethod]
        public void CancelCurrentArchiveTest_NullCurrentArchive()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            MockProcessor.Setup(x => x.CancelProcessing()).Callback(() => {
                MockProcessor.Raise(x => x.ProcessError += null, Processor, new DPProcessorErrorArgs(null, null)); // Used to check status
            });
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(null as IDPArchive);

            job.CancelCurrentArchive();

            MockProcessor.Verify(x => x.CancelCurrentArchive(), Times.Never());
            MockExtractView.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void CancelCurrentArchiveTest_NullFileInfo()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            MockProcessor.Setup(x => x.CancelProcessing()).Callback(() => {
                MockProcessor.Raise(x => x.ProcessError += null, Processor, new DPProcessorErrorArgs(null, null)); // Used to check status
            });
            var currentArc = NewFakeDPArchive("U:/foo", true);
            currentArc.Object.FileInfo = null;
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(currentArc.Object);

            job.CancelCurrentArchive();

            MockProcessor.Verify(x => x.CancelCurrentArchive(), Times.Once());
            MockExtractView.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void SkipArchiveTest_PendingNotCurrentArchive()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var currentArc = NewFakeDPArchive("U:/bar", true);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(currentArc.Object);

            job.SkipArchive("U:/bar");

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.CancellationPending);
            MockProcessor.Verify(x => x.CancelCurrentArchive(), Times.Once());
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(currentArc.Object);
        }

        [TestMethod]
        public void SkipArchiveTest_PendingCurrentArchive()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var currentArc = NewFakeDPArchive("U:/bar", true);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(currentArc.Object);

            job.SkipArchive("U:/bar");

            DPExtractJobTestHelpers.AssertStatusUpdates(MockExtractView, "U:/bar", DPArchiveStatus.CancellationPending, DPArchiveStatus.CancellationRequested);
            MockProcessor.Verify(x => x.CancelCurrentArchive(), Times.Once());
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(currentArc.Object);
        }

        [TestMethod]
        public void SkipArchiveTest_DoesntExist()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var currentArc = NewFakeDPArchive("U:/bar", true);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(currentArc.Object);

            Assert.ThrowsException<ArgumentException>(() => job.SkipArchive("nonexistant"));
        }

        [TestMethod]
        public void SkipArchiveTest_JobCancelled()
        {
            string[] files = ["U:/foo", "U:/bar"];
            var job = NewExtractJob(files);
            var currentArc = NewFakeDPArchive("U:/bar", true);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(currentArc.Object);
            job.CancelJob();

            job.SkipArchive("U:/bar");

            MockProcessor.Verify(x => x.CancelCurrentArchive(), Times.Never());
        }

        [TestMethod]
        public void SkipArchiveTest_ArchiveAlreadyCancelled()
        {
            string[] files = ["U:/bar"];
            var job = NewExtractJob(files);
            var currentArc = NewFakeDPArchive("U:/bar", true);
            MockProcessor.SetupGet(x => x.CurrentArchive).Returns(currentArc.Object);
            MockProgressCombo.SetupGet(x => x.Token).Returns(CancellationToken.None);
            MockProcessor.Setup(x => x.ProcessArchive(It.IsAny<string>(), It.IsAny<DPProcessSettings>())).Callback((string path, DPProcessSettings __) => {
                var mockArchive = NewFakeDPArchive(path, true);
                var report = new DPExtractionReport() { ErroredFiles = [], ExtractedFiles = [], Settings = new DPExtractSettings() };
                // Raise the event to set CancellationRequested -> Cancelled.
                // Set processed to false to make life easier.
                MockProcessor.Raise(x => x.ArchiveExit += null, Processor, new DPArchiveExitArgs(mockArchive.Object, report, false));
            });
            job.CancelCurrentArchive(); // Get the archive to set as CancellationRequested.
            job.SkipArchive("U:/bar");
            
            MockProcessor.Verify(x => x.CancelCurrentArchive(), Times.Once());
        }
    }
}