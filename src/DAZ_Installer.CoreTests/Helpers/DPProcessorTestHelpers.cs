using DAZ_Installer.Core.Extraction;
using DAZ_Installer.CoreTests.Extraction;
using DAZ_Installer.Core.Tests.Fakes;
using System.Linq.Expressions;
using DAZ_Installer.IO;
using DAZ_Installer.IO.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Serilog;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Dynamic;

namespace DAZ_Installer.Core.Tests
{
    [Obsolete("For testing purposes only.")]
    internal class DPProcessorTestHelpers
    {
        const string DEFAULT_ARCHIVE_PATH = "Z:/test.rar";
        public static MockOptions DefaultMockOptions => new();

        /// <summary>
        /// Used to assert the <see cref="DPProcessor"/> events.
        /// </summary>
        /// <remarks>
        /// This is used to collect assertions for events that are made during the process of the <see cref="DPProcessor"/>.
        /// </remarks>
        public interface IProcessorAssertor
        {
            /// <summary>
            /// Raises any exceptions that were thrown with <see cref="AddAssertion(Action)"/>
            /// </summary>
            void Assert();
            /// <summary>
            /// Asserts and holds on to the exception if it is thrown.
            /// </summary>
            /// <param name="assertion"></param>
            void AddAssertion(Action assertion);
        }

        /// <summary>
        /// Used to assert the <see cref="DPProcessor"/> events.
        /// </summary>
        /// <remarks>
        /// This is used to collect assertions for events that are made during the process of the <see cref="DPProcessor"/>.
        /// </remarks>
        public class EventProcessorAssertor : IProcessorAssertor {
            /// <summary>
            /// The first exception that was thrown during the process of the <see cref="DPProcessor"/>.
            /// </summary>
            /// <remarks>
            /// This can only be added by <see cref="AddAssertion(Action)"/>
            /// </remarks>
            public Exception? exception;
            /// <summary>
            /// Assert now which can throw an exception if an assertion fails.
            /// </summary>
            /// <exception cref="AggregateException">If an assertion fails.</exception> 
            public void Assert() {
                if (exception is not null) throw new AggregateException(exception);
            }
            /// <summary>
            /// Asserts and if an assertion fails, no more assertions will be added and <see cref="Assert"/> will throw an exception.
            /// </summary>
            /// <param name="assertion">An assertion statement such as <see cref="Assert.AreEqual{T}(T, T)"/>.</param>
            public void AddAssertion(Action assertion) {
                if (exception is not null) return;
                try {
                    assertion();
                } catch (Exception e) {
                    exception = e;
                }
            }

            public EventProcessorAssertor() {}
        }

        /// <summary>
        /// A processor assertor that can be used to assert the <see cref="DPProcessor"/> events.
        /// </summary>
        /// <remarks>
        /// This only throws at <see cref="Assert"/> if all assertions fail.
        /// </remarks> 
        public class EventOrProcessAssertor : IProcessorAssertor {
            private List<Exception> exceptions = new();
            private byte calls = 0;
            /// <summary>
            /// Assert throws if all assertions fail. If one assertion does not fail, then it does not throw.
            /// </summary>
            /// <exception cref="AggregateException">The exception of all failed assertions if all assertions fail.</exception>
            public void Assert() {
                if (exceptions.Count == calls) throw new AggregateException(exceptions);
            }
            /// <inheritdoc/>
            public void AddAssertion(Action assertion) {
                calls++;
                try {
                    assertion();
                } catch (Exception e) {
                    exceptions.Add(e);
                }
            }
        }

        /// <summary>
        /// A task that can be used to assert the <see cref="EventProcessorAssertor"/> object.
        /// </summary>
        /// <remarks>
        /// This is used to collect the assertions that are made during the process of the <see cref="DPProcessor"/>.
        /// Helper functions use this to finalize event handler tests.
        /// </remarks>
        public sealed class AssertableTask : IAsyncResult, IDisposable {
            private readonly IProcessorAssertor assertor;
            private readonly Task task;
            /// <summary>
            /// A task that can be used to assert the <see cref="EventProcessorAssertor"/> object.
            /// </summary>
            /// <param name="action">The action that will be run.</param>
            /// <param name="assertor">The assertor to use, if any. Otherwise, <see cref="EventProcessorAssertor"/> is used.</param>
            public AssertableTask(Action action, IProcessorAssertor? assertor = null) {
                this.assertor = assertor ?? new EventProcessorAssertor();
                task = new Task(action);
            }
            /// <summary>
            /// <inheritdoc cref="AssertableTask(Action, IProcessorAssertor)"/>
            /// </summary>
            /// <param name="action">The action that will be run with a assertable as a parameter.</param>
            /// <param name="assertor">The assertor to use, if any. Otherwise, <see cref="EventProcessorAssertor"/> is used.</param>
            public AssertableTask(Action<AssertableTask> action, IProcessorAssertor? assertor = null)
            {
                this.assertor = assertor ?? new EventProcessorAssertor();
                task = new Task(() => action(this));
            }
            /// <inheritdoc cref="IProcessorAssertor.Assert"/>
            public void Assert() => assertor.Assert();
            /// <inheritdoc cref="IProcessorAssertor.AddAssertion(Action)"/>
            public void AddAssertion(Action assertion) => assertor.AddAssertion(assertion);
            /// <inheritdoc cref="Task.Dispose()"/>
            public void Dispose() => task.Dispose();
            /// <inheritdoc cref="Task.AsyncState"/>
            public object? AsyncState => task.AsyncState;
            /// <inheritdoc cref="Task.AsyncWaitHandle"/>
            public WaitHandle AsyncWaitHandle => ((IAsyncResult)task).AsyncWaitHandle;
            /// <inheritdoc cref="Task.CompletedSynchronously"/>
            public bool CompletedSynchronously => ((IAsyncResult)task).CompletedSynchronously;
            /// <inheritdoc cref="Task.IsCompleted"/>
            public bool IsCompleted => ((IAsyncResult)task).IsCompleted;
            /// <inheritdoc cref="Task.RunSynchronously()"/>
            public void RunSynchronously() => task.RunSynchronously();
            /// <inheritdoc cref="Task.Wait()"/>
            public void Wait() => task.Wait();
            /// <inheritdoc cref="Task.GetAwaiter()"/>
            public TaskAwaiter GetAwaiter() => task.GetAwaiter();
        }

        /// <summary>
        /// A collection of Mock settings used for <see cref="DPProcessorTestHelpers"/> helper methods.
        /// </summary>
        /// <seealso cref="ArchiveMocks"/>
        public struct MockOptions
        {
            /// <summary>
            /// Determines whether to partially mock the <see cref="FakeFileInfo"/> object.
            /// </summary>
            public bool partialFileInfo = true;
            /// <summary>
            /// Determines whether to partially mock the <see cref="IRAR"/> object.
            /// </summary>
            public bool partialRAR = true;
            /// <summary>
            /// Determines whether to partially mock the <see cref="FakeDPFileInfo"/> object.
            /// </summary>
            public bool partialDPFileInfo = true;
            /// <summary>
            /// Determines whether to partially mock each <see cref="IZipArchiveEntry"/> object.
            /// </summary>
            public bool partialZipArchiveEntry = true;
            /// <summary>
            /// Determines whether to partially mock the <see cref="FakeFileSystem"/> object.
            /// </summary>
            public bool partialFakeFileSystem = true;
            /// <summary>
            /// Determines whether to partially mock the <see cref="FakeDPArchive"/> object.
            /// </summary>
            public bool partialFakeDPArchive = true;
            public MockOptions() { }
        }

        public struct ProcessorOptions
        {
            /// <summary>
            /// The settings that will be used to determine, for instance, a temp directory by <see cref="AbstractFileSystem"/>.
            /// </summary>
            public DPProcessSettings Settings;
            /// <summary>
            /// The archive to return on the start of the process.
            /// </summary>
            public IDPArchive Archive;
            /// <summary>
            /// The file system that the <see cref="DPProcessor"/> will use.
            /// </summary>
            public FakeFileSystem FileSystem;
        }

        /// <summary>
        /// A struct that contains all the mocked dependencies for the <see cref="IDPArchive"/>
        /// for <see cref="NewMockedArchive(MockOptions, out ArchiveMocks)"/>
        /// </summary>
        /// <seealso cref="MockOptions"/>
        public struct ArchiveMocks
        {
            /// <summary>
            /// The mocked Extractor that <see cref="FakeDPArchive"/> will use.
            /// </summary>
            public Mock<DPAbstractExtractor> MockExtractor;
            /// <summary>
            /// The mocked <see cref="MockFakeDPFileInfo"/> that <see cref="FakeDPArchive"/> will use.
            /// </summary>
            public Mock<FakeDPFileInfo> MockFakeDPFileInfo;
            /// <summary>
            /// The mocked <see cref="MockFakeFileInfo"/> that <see cref="ArchiveMocks.MockFakeDPFileInfo"/> will use.
            /// </summary>
            public Mock<FakeFileInfo> MockFakeFileInfo;
            /// <summary>
            /// The fake file system that <see cref="ArchiveMocks.MockFakeDPFileInfo"/> will use.
            /// </summary>
            public Mock<FakeFileSystem> MockFakeFileSystem;
            /// <summary>
            /// The mocked <see cref="FakeDPFolderFactory"/> that <see cref="FakeDPArchive"/> will use.
            /// </summary>
            public Mock<FakeDPFolderFactory> MockFolderFactory;
            /// <summary>
            /// The mocked <see cref="FakeDPFileFactory"/> that <see cref="FakeDPArchive"/> will use.
            /// </summary>
            public Mock<FakeDPFileFactory> MockFileFactory;
            /// <summary>
            /// The mocked <see cref="FakeDPArchive"/> that will be used for testing.
            /// </summary>
            public Mock<FakeDPArchive> MockArchive;

            public readonly DPAbstractExtractor Extractor => MockExtractor.Object;
            public readonly FakeDPFileInfo FakeDPFileInfo => MockFakeDPFileInfo.Object;
            public readonly FakeFileInfo FakeFileInfo => MockFakeFileInfo.Object;
            public readonly FakeFileSystem FakeFileSystem => MockFakeFileSystem.Object;
            public readonly FakeDPFolderFactory FolderFactory => MockFolderFactory.Object;
            public readonly FakeDPFileFactory FileFactory => MockFileFactory.Object;
            public readonly FakeDPArchive Archive => MockArchive.Object;
        }

        /// <summary>
        /// A struct that contains all the mocked dependencies for the <see cref="DPProcessor"/>
        /// </summary>
        public struct ProcessorMocks
        {
            /// <summary>
            /// The mocked <see cref="AbstractDestinationDeterminer"/> that <see cref="DPProcessor"/> will use.
            /// </summary>
            public Mock<AbstractDestinationDeterminer> MockDestinationDeterminer;
            /// <summary>
            /// The mocked <see cref="AbstractTagProvider"/> that <see cref="DPProcessor"/> will use.
            /// </summary>
            public Mock<AbstractTagProvider> MockTagProvider;
            /// <summary>
            /// The mocked <see cref="FakeFileSystem"/> that <see cref="DPProcessor"/> will use.
            /// </summary>
            /// <remarks><see cref="DPProcessor"/> uses this object to "create" the temp directory and clear temp.</remarks>
            public Mock<FakeDPDirectoryInfo> MockFakeTempDirectoryInfo;
            /// <summary>
            /// The mocked <see cref="FakeDPDriveInfo"/> that <see cref="DPProcessor"/> will use.
            /// </summary>
            public Mock<FakeDPDriveInfo> MockFakeDriveInfo;
            /// <summary>
            /// The mocked <see cref="MetadataReader"/> that <see cref="DPProcessor"/> will use.
            /// </summary>
            public Mock<IDPMetadataReader> MockMetadataReader;
            
            public readonly AbstractDestinationDeterminer DestinationDeterminer => MockDestinationDeterminer.Object;
            public readonly AbstractTagProvider TagProvider => MockTagProvider.Object;
            public readonly FakeDPDirectoryInfo FakeTempDirectoryInfo => MockFakeTempDirectoryInfo.Object;
            public readonly FakeDPDriveInfo FakeDriveInfo => MockFakeDriveInfo.Object;
            public readonly IDPMetadataReader MetadataReader => MockMetadataReader.Object;
        }

        /// <summary>
        /// Creates a new <see cref="DPArchive"/> with mocked dependencies.
        /// </summary>
        /// <remarks>
        /// Completely sets up the dependencies for the <see cref="DPArchive"/> and returns the mocked dependencies in an <see cref="ArchiveMocks"/> struct. 
        /// Partial mocks will allow you to keep the original behavior of the mocked object while still allowing you to override certain methods.
        /// If you do not wish to override any methods (or mock everything), make sure you change <paramref name="options"/> to False for partial fields.
        /// <br/>
        /// The extractor is setup to return a <see cref="DPExtractionReport"/> with the files that were extracted.
        /// </remarks>
        /// <param name="options">The mock options to use for setting up the archive.</param>
        /// <param name="mocks">An out parameter that will contain all the mocked dependencies.</param>
        /// <returns>A new <see cref="DPArchive"/> with mocked dependencies.</returns>
        public static FakeDPArchive NewMockedArchive(MockOptions options, out ArchiveMocks mocks)
        {
            mocks = new ArchiveMocks();
            mocks.MockFakeFileSystem = new Mock<FakeFileSystem>() { CallBase = options.partialFakeFileSystem };
            mocks.MockFakeFileInfo = new Mock<FakeFileInfo>(DEFAULT_ARCHIVE_PATH) { CallBase = options.partialFileInfo };
            mocks.MockFakeDPFileInfo = new Mock<FakeDPFileInfo>(mocks.FakeFileInfo, mocks.FakeFileSystem, null!) { CallBase = options.partialDPFileInfo };
            mocks.MockExtractor = new Mock<DPAbstractExtractor>();
            mocks.MockFileFactory = new Mock<FakeDPFileFactory>() { CallBase = true };
            mocks.MockFolderFactory = new Mock<FakeDPFolderFactory>() { CallBase = true };
            mocks.MockArchive = new Mock<FakeDPArchive>(DEFAULT_ARCHIVE_PATH, null!, null!) { CallBase = options.partialFakeDPArchive };
            mocks.Archive.Extractor = mocks.Extractor;
            mocks.Archive.FileFactory = mocks.FileFactory;
            mocks.Archive.FolderFactory = mocks.FolderFactory;
            mocks.Archive.FileInfo = mocks.FakeDPFileInfo;
            return mocks.Archive;
        }

        /// <summary>
        /// Initializes the <see cref="DPProcessor"/> by fully setting up its depenencies and creating the entities in the archive.
        /// </summary>
        /// <remarks>
        /// <paramref name="destDerm"/> returns a mock destination determiner that is setup to simply 
        /// return the contents of the <paramref name="arc"/>. <paramref name="arc"/> is also used for setting up 
        /// entities with <see cref="DefaultContents"/>
        /// <br/>
        /// Also note that <paramref name="tagProvider"/> returns a mock tag provider that is not setup.
        /// </remarks>
        /// <param name="opts">The options to use for setting up the processor.</param>
        /// <returns>A <see cref="DPProcessor"/> that is ready to be tested.</returns>
        public static DPProcessor SetupProcessor(ProcessorOptions opts, out ProcessorMocks mocks)
        {
            mocks = new() {
                MockDestinationDeterminer = new Mock<AbstractDestinationDeterminer>(),
                MockTagProvider = new Mock<AbstractTagProvider>(),
                MockFakeTempDirectoryInfo = new Mock<FakeDPDirectoryInfo>(opts.Settings.TempPath, opts.FileSystem, null!) { CallBase = true },
                MockFakeDriveInfo = new Mock<FakeDPDriveInfo>(opts.FileSystem, opts.Settings.DestinationPath) { CallBase = true },
                MockMetadataReader = new Mock<IDPMetadataReader>()
            };
            var paf = new Mock<IDPParentArchiveFactory>();
            paf.Setup(x => x.CreateNewParentArchive(It.IsAny<IDPFileInfo>())).Returns(opts.Archive);
            var p = new DPProcessor()
            {
                Logger = Log.Logger.ForContext<DPProcessor>(),
                FileSystem = opts.FileSystem,
                DestinationDeterminer = mocks.DestinationDeterminer,
                TagProvider = mocks.TagProvider,
                ParentArchiveFactory = paf.Object,
                MetadataReader = mocks.MetadataReader,
            };
            return p;
        }

        public static async Task AssertState(DPProcessor processor, IDPArchive archive, ProcessorState states, AssertableTask processorTask)
        {

            void func()
            {
                if (processor.CurrentArchive != archive) return;
                if (processor.State != ProcessorState.Idle && (processor.State & states) == ProcessorState.Idle)
                    processorTask.AddAssertion(() => Assert.Fail("Got unexpected state: " + processor.State.ToString()));
                // if the state is in the states, remove it
                states &= ~processor.State;
            }
            void stopOnEnd()
            {
                if (processor.CurrentArchive != archive) return;
                if (processor.State == ProcessorState.Idle)
                {
                    processor.StateChanged -= func;
                    processor.StateChanged -= stopOnEnd;
                }
            }
            processor.ArchiveExit += async (_, e) =>
            {
                if (e.Archive == archive)
                {
                    processor.StateChanged -= func;
                    processor.StateChanged -= stopOnEnd;
                }
            };
            processor.StateChanged += func;
            // Wait for the processor to finish.
            await processorTask;
            processor.StateChanged -= func;
            // Now that the processor is finished, assert that all states were asserted.
            processorTask.AddAssertion(() => Assert.AreEqual(ProcessorState.Idle, states, "Not all states were asserted"));
        }
        public static async Task AssertAnyState(DPProcessor processor, IDPArchive archive, ProcessorState states, AssertableTask processorTask)
        {
            var statesList = new List<ProcessorState>();
            processor.ArchiveEnter += async (_, e) =>
            {
                if (e.Archive != archive) return;
                statesList.Add(states);
            };

            void func()
            {
                if (processor.CurrentArchive != archive) return;
                statesList[^1] &= ~processor.State;
            }
            processor.StateChanged += func;
            // Wait for the processor to finish.
            await processorTask;
            processor.StateChanged -= func;
            // Now that the processor is finished, assert that all states were asserted.
            if (statesList.All(x => x != ProcessorState.Idle))
                processorTask.AddAssertion(() => Assert.Fail("No state found where all states were asserted"));
        }

        public static async Task AssertArchiveEnter(DPProcessor processor, AssertableTask processorTask, params IDPArchive[] expectedArchives)
        {
            var expectedArchivesList = expectedArchives.ToList();
            var called = false;
            processor.ArchiveEnter += async (p, e) =>
            {
                // processorTask.AddAssertion(() => Assert.Fail("because i can"));
                processorTask.AddAssertion(() => Assert.AreEqual(processor, p));
                processorTask.AddAssertion(() => CollectionAssert.Contains(expectedArchives, e.Archive, "Archive not found in expected archives: " + e.Archive.FileName));
                expectedArchivesList.Remove(e.Archive);
                called = true;
            };

            // Wait for the processor to finish.
            await processorTask;

            // Check that we were called as intended.
            if (!called) processorTask.AddAssertion(() => Assert.Fail("ArchiveEnter was not called"));
            processorTask.AddAssertion(() => Assert.AreEqual(0, expectedArchivesList.Count, "Not all expected archives were entered: " + string.Join(", ", expectedArchivesList.Select(x => x.FileName)))); 
        }

        public static async Task AssertAnyArchiveExit(DPProcessor processor, AssertableTask processorTask, bool success, DPExtractionReport? report, params IDPArchive[] archives)
        {
            var exitEvents = new List<DPArchiveExitArgs>();
            async Task func(IDPProcessor p, DPArchiveExitArgs e)
            {
                exitEvents.Add(e);
            }
            processor.ArchiveExit += func;

            await processorTask;

            processor.ArchiveExit -= func;

            processorTask.AddAssertion(() =>
            {
                if (exitEvents.Count == 0) Assert.Fail("ArchiveExit was not called");
                var matchingExit = exitEvents.FirstOrDefault(e => archives.Contains(e.Archive) && e.Processed == success && e.Report == report);
                if (matchingExit == null) 
                    Assert.Fail($"No matching ArchiveExit event found. Expected: success={success}, report={report}, archives=[{string.Join(", ", archives.Select(a => a.FileName))}]");
            });
        }

        public static async Task AssertFinished(DPProcessor processor, AssertableTask processorTask)
        {
            var called = false;
            processor.Finished += () => called = true;

            await processorTask;
            Assert.IsTrue(called, "Finished was not called");
        }

        public static async Task AssertExtractionProgress(DPProcessor processor, AssertableTask processorTask, DPExtractProgressArgs expected) {
            var called = false;
            processor.ExtractProgress += async (p, e) =>
            {
                processorTask.AddAssertion(() => Assert.AreEqual(processor, p));
                processorTask.AddAssertion(() => Assert.AreEqual(expected.ExtractionPercentage, e.ExtractionPercentage));
                processorTask.AddAssertion(() => Assert.AreEqual(expected.Archive, e.Archive));
                processorTask.AddAssertion(() => Assert.AreEqual(expected.File, e.File));
                called = true;
            };

            // Wait for the processor to finish.
            await processorTask;
            if (!called) processorTask.AddAssertion(() => Assert.Fail("ExtractProgress was not called"));
        }

        public static async Task AssertAnyExtractionProgress(DPProcessor processor, AssertableTask processorTask, params DPExtractProgressArgs[] expectedArgs)
        {
            var progressEvents = new List<DPExtractProgressArgs>();
            async Task func(IDPProcessor p, DPExtractProgressArgs e)
            {
                progressEvents.Add(e);
            }
            processor.ExtractProgress += func;

            await processorTask;

            processor.ExtractProgress -= func;

            processorTask.AddAssertion(() =>
            {
                if (progressEvents.Count == 0) Assert.Fail("ExtractProgress was not called");
                var matchingProgress = progressEvents.FirstOrDefault(e => 
                    expectedArgs.Any(expected => 
                        e.ExtractionPercentage == expected.ExtractionPercentage &&
                        e.Archive == expected.Archive &&
                        e.File == expected.File
                    )
                );
                if (matchingProgress == null)
                    Assert.Fail($"No matching ExtractProgress event found. Expected one of: [{string.Join(", ", expectedArgs.Select(a => $"{{Archive: {a.Archive.FileName}, File: {a.File?.Path}, Percentage: {a.ExtractionPercentage}}}"))}]");
            });
        }
        public static async Task AssertArchiveProcessOrder(DPProcessor processor, AssertableTask processorTask, Queue<IDPArchive> expectedArchiveOrder)
        {
            var expectedArchiveOrderForExiting = new Queue<IDPArchive>(expectedArchiveOrder);
            async Task func(IDPProcessor p, DPArchiveEnterArgs e)
            {
                if (expectedArchiveOrder.Count == 0) return;
                var expected = expectedArchiveOrder.Dequeue();
                processorTask.AddAssertion(() => Assert.AreEqual(expected, e.Archive, $"Expected {expected.Path}, got {e.Archive.Path}"));
            }
            async Task func2(IDPProcessor p, DPArchiveExitArgs e)
            {
                if (expectedArchiveOrderForExiting.Count == 0) return;
                var expected = expectedArchiveOrderForExiting.Dequeue();
                processorTask.AddAssertion(() => Assert.AreEqual(expected, e.Archive, $"Expected {expected.Path}, got {e.Archive.Path}"));
            }
            processor.ArchiveEnter += func;
            processor.ArchiveExit += func2;
            await processorTask;
            processor.ArchiveEnter -= func;
            processor.ArchiveExit -= func2;

        }
        
        public static async Task AssertProcessorError(DPProcessor processor, AssertableTask processorTask, DPProcessorErrorArgs expected)
        {
            var called = false;
            processor.ProcessError += async (p, e) =>
            {
                processorTask.AddAssertion(() => Assert.AreEqual(processor, p));
                processorTask.AddAssertion(() => Assert.AreEqual(expected.Explaination, e.Explaination));
                processorTask.AddAssertion(() => Assert.AreEqual(expected.Ex, e.Ex));
                processorTask.AddAssertion(() => Assert.AreEqual(expected.Continuable, e.Continuable));
                called = true;
            };

            // Wait for the processor to finish.
            await processorTask;
            if (!called) processorTask.AddAssertion(() => Assert.Fail("Error was not called"));
        }

        public static async Task AssertAnyProcessorError(DPProcessor processor, AssertableTask processorTask, params DPProcessorErrorArgs[] expected)
        {
            //var progressEvents = new List<DPExtractProgressArgs>();
            //void func(DPProcessor p, DPExtractProgressArgs e)
            //{
            //    progressEvents.Add(e);
            //}
            //processor.ExtractProgress += func;

            //await processorTask;

            //processor.ExtractProgress -= func;

            //processorTask.AddAssertion(() =>
            //{
            //    if (progressEvents.Count == 0) Assert.Fail("ExtractProgress was not called");
            //    var matchingProgress = progressEvents.FirstOrDefault(e =>
            //        expectedArgs.Any(expected =>
            //            e.ExtractionPercentage == expected.ExtractionPercentage &&
            //            e.Archive == expected.Archive &&
            //            e.File == expected.File
            //        )
            //    );
            //    if (matchingProgress == null)
            //        Assert.Fail($"No matching ExtractProgress event found. Expected one of: [{string.Join(", ", expectedArgs.Select(a => $"{{Archive: {a.Archive.FileName}, File: {a.File?.Path}, Percentage: {a.ExtractionPercentage}}}"))}]");
            //});
            var processorErrorEvents = new List<DPProcessorErrorArgs>(2);
            async Task func(IDPProcessor p, DPProcessorErrorArgs e)
            {
                processorErrorEvents.Add(e);
            }
            processor.ProcessError += func;
            await processorTask;
            processor.ProcessError -= func;

            processorTask.AddAssertion(() =>
            {
                if (processorErrorEvents.Count == 0) Assert.Fail("ProcessError was not called");
                var matchingError = processorErrorEvents.FirstOrDefault(e =>
                    expected.Any(expected =>
                        e.Explaination == expected.Explaination &&
                        e.Ex == expected.Ex &&
                        e.Continuable == expected.Continuable
                    )
                );
                if (matchingError == null)
                    Assert.Fail($"No matching ProcessError event found. Expected one of: [{string.Join(", ", expected.Select(a => $"{{Explaination: {a.Explaination}, Ex: {a.Ex}, Continuable: {a.Continuable}}}"))}]");
            });
        }

        /// <summary>
        /// Create a new <see cref="DPExtractionReport"/> with the given settings and files.
        /// </summary>
        /// <param name="settings">The settings to use</param>
        /// <param name="failedFiles">Expected failed file(names).</param>
        /// <param name="successFiles">Expected successful file(names).</param>
        /// <returns>A configured <see cref="DPExtractionReport"/> with dummy files.</returns>
        public static DPExtractionReport CreateExtractionReport(DPExtractSettings settings, IEnumerable<string> failedFiles, IEnumerable<string>? successFiles)
        {
            var fh = failedFiles.ToHashSet();
            var sh = successFiles?.ToHashSet() ?? new HashSet<string>();
            var extractedFiles = successFiles?.ToList() ?? settings.Archive.Contents.Values.Where(m => !fh.Contains(m.Path)).Select(x => x.Path);
            if (fh.Intersect(sh).Count() > 1) Assert.Inconclusive("Failed and Success files intersect");
            return new DPExtractionReport()
            {
                Settings = settings,
                ErroredFiles = failedFiles.ToDictionary(m => (IDPFile) CreateDummyFile(m), _ => string.Empty),
                ExtractedFiles = successFiles?.Select(x => (IDPFile) CreateDummyFile(x)).ToList() ?? new List<IDPFile>()
            };
        }

        /// <summary>
        /// Creates a <see cref="DPFile"/> with null dependencies.
        /// </summary>
        /// <param name="path">The path to set for this file.</param>
        /// <returns>A file with null <see cref="DPArchive"/>, <see cref="DPFolder"/>, <see cref="IDPFileInfo"/>, and <see cref="ILogger"/>.</returns>
        public static FakeDPFile CreateDummyFile(string path) => new(path, null);
        /// <summary>
        /// Creates a <see cref="DPFolder"/> with null dependencies.
        /// </summary>
        /// <param name="path">The path to set for this folder.</param>
        /// <returns>A folder with null <see cref="DPArchive"/> and parent <see cref="DPFolder"/></returns>
        public static DPFolder CreateDummyFolder(string path) => new(path, null, null!);
        /// <summary>
        /// Asserts the <see cref="DPExtractionReport"/> objects are equal.
        /// </summary>
        /// <param name="want">The expected report</param>
        /// <param name="got">The result report from an operation</param>
        public static void AssertReport(DPExtractionReport want, DPExtractionReport got)
        {
            var a = want.ErroredFiles.Keys.Select(x => x.Path).ToArray();
            var b = got.ErroredFiles.Keys.Select(x => x.Path).ToArray();
            CollectionAssert.AreEqual(a, b, "Errored file paths are not equal");
            a = want.ExtractedFiles.Select(x => x.Path).ToArray();
            b = got.ExtractedFiles.Select(x => x.Path).ToArray();
            CollectionAssert.AreEqual(a, b, "Extracted file paths are not equal");

            // Settings
            Assert.AreSame(want.Settings.Archive, got.Settings.Archive, "Reports' archive are not the same");
            CollectionAssert.AreEqual(want.Settings.FilesToExtract.Select(x => x.Path).ToArray(), got.Settings.FilesToExtract.Select(x => x.Path).ToArray(), "Reports' files to extract are not the same");
            Assert.AreEqual(want.Settings.OverwriteFiles, got.Settings.OverwriteFiles, "Reports' overwrite files are not the same");
        }

        private static bool CompareExtractSettings(DPExtractSettings want, DPExtractSettings got)
        {
            try
            {
                Assert.AreEqual(want.Archive, got.Archive);
                Assert.AreEqual(want.TempPath, got.TempPath);
                Assert.AreEqual(want.OverwriteFiles, got.OverwriteFiles);
                Assert.AreEqual(want.FilesToExtract.Count, got.FilesToExtract.Count);
                CollectionAssert.AreEqual(want.FilesToExtract.Select(x => x.Path).ToArray(), got.FilesToExtract.Select(x => x.Path).ToArray());
            } catch (Exception e)
            {
                Log.Error(e, "Extract settings are not the same");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Verify that <see cref="DPArchive.ExtractContents(DPExtractSettings)"/> or <see cref="DPArchive.ExtractContentsToTemp(DPExtractSettings)"/>.
        /// </summary>
        /// <param name="toTemp">Whether to verify <see cref="DPArchive.ExtractContentsToTemp(DPExtractSettings)"/> or <see cref="DPArchive.ExtractContents(DPExtractSettings)"/>.</param>
        /// <param name="arc">The archive to verify.</param>
        /// <param name="expected">The expected settings to verify.</param>
        /// <param name="times">The number of times to verify.</param> 
        public static void VerifyExtractContentsCalled(bool toTemp, Mock<FakeDPArchive> arc, DPExtractSettings expected, Times times)
        {
            var methodName = toTemp ? nameof(FakeDPArchive.ExtractContentsToTemp) : nameof(FakeDPArchive.ExtractContents);

            if (toTemp)
            {
                arc.Verify(x => x.ExtractContentsToTemp(It.Is<DPExtractSettings>(got => CompareExtractSettings(expected, got))), times, 
                    $"Processor did not call {methodName} with expected settings.");
            }
            else
            {
                arc.Verify(x => x.ExtractContents(It.Is<DPExtractSettings>(got => CompareExtractSettings(expected, got))), times, 
                    $"Processor did not call {methodName} with expected settings.");
            }
        }
        public static void AssertExtractSettings(DPExtractSettings want, DPExtractSettings got) {
            Assert.AreSame(want.Archive, got.Archive, "Archive is not the same");
            CollectionAssert.AreEqual(want.FilesToExtract.Select(x => x.Path).ToArray(), got.FilesToExtract.Select(x => x.Path).ToArray(), "Files to extract are not the same");
            Assert.AreEqual(want.OverwriteFiles, got.OverwriteFiles, "Overwrite files are not the same");
            Assert.AreEqual(want.TempPath, got.TempPath, "Temp path is not the same");
        }
        /// <summary>
        /// Creates a new <see cref="DPExtractSettings"/> with the given paths and archive.
        /// </summary>
        /// <param name="paths">The paths of the archive. Paths can be empty or null, it will be filtered out.</param>
        /// <param name="arc">The archive to extract.</param>
        /// <returns>A set-up <see cref="DPExtractSettings"/> object.</returns>
        public static DPExtractSettings CreateExtractSettings(IEnumerable<string> paths, IDPArchive arc) => 
            new("A:/", paths.Where(x => !string.IsNullOrEmpty(Path.GetFileName(x))).Select(x => CreateDummyFile(x)), archive: arc);

    }
}
