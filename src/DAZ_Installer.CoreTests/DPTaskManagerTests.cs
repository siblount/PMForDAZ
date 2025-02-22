using DAZ_Installer.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog;
using MSTestLogger = Microsoft.VisualStudio.TestTools.UnitTesting.Logging.Logger;
using Moq;
using DAZ_Installer.Common;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DAZ_Installer.Core.Tests
{
    [TestClass]
    public class DPTaskManagerTests
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

        [TestMethod]
        public async Task AddToQueueTest_Action()
        {
            var mockAction = new Mock<Action>();

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction.Object);

            mockAction.Verify(a => a(), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueueTest_QueueAction()
        {
            var mockAction = new Mock<DPTaskManager.QueueAction>();

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction.Object);

            mockAction.Verify(a => a(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueueTest_ActionDoesntStartInParallel()
        {
            var taskManager = new DPTaskManager();
            using var waitSlim = new ManualResetEventSlim(false);

            var t = taskManager.AddToQueue(() => waitSlim.Wait());
            var t2 = taskManager.AddToQueue(() => waitSlim.Wait());
            Thread.Sleep(0);

            if (t2.Status is not (TaskStatus.WaitingForActivation or TaskStatus.WaitingToRun))
            {
                Assert.Fail($"Expected either WaitingForAction or WaitingToRun, got: {t2.Status}");
            }

            waitSlim.Set();

            await Task.WhenAll(t, t2);
        }

        [TestMethod]
        public async Task AddToQueueTest_ActionContinues()
        {
            var mockAction1 = new Mock<Action>();
            var mockAction2 = new Mock<Action>();
            var mockAction3 = new Mock<Action>();

            var sequence = new MockSequence();

            mockAction1.InSequence(sequence).Setup(a => a());
            mockAction2.InSequence(sequence).Setup(a => a());
            mockAction3.InSequence(sequence).Setup(a => a());

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction1.Object);
            await taskManager.AddToQueue(mockAction2.Object);
            await taskManager.AddToQueue(mockAction3.Object);

            mockAction1.Verify(a => a(), Times.Once);
            mockAction2.Verify(a => a(), Times.Once);
            mockAction3.Verify(a => a(), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueueTest_MaintainsOrderAcrossThreads()
        {
            var taskManager = new DPTaskManager();
            var numberOfTasks = 8 * Environment.ProcessorCount;
            var orderOfExecution = new List<int>(numberOfTasks);
            var innerTasks = new List<Task>(numberOfTasks);
            var lockObj = new object();

            // Create multiple threads to add tasks to the queue
            await TaskUtils.ExecuteInParallel(numberOfTasks, (byte)Environment.ProcessorCount, (_) => {
                lock (lockObj)
                    innerTasks.Add(taskManager.AddToQueue(() =>
                    {
                        var index = orderOfExecution.Count;
                        orderOfExecution.Add(index);
                    }));
            });

            await Task.WhenAll(innerTasks);

            // Verify the order of execution
            for (int i = 0; i < numberOfTasks; i++)
            {
                Assert.AreEqual(i, orderOfExecution[i], $"Task at index {i} was executed out of order.");
            }
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionTest()
        {
            var mockAction = new Mock<DPTaskManager.QueueAction>();

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction.Object);

            mockAction.Verify(a => a(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueueTest_QueueActionDoesntStartInParallel()
        {
            var taskManager = new DPTaskManager();
            using var waitSlim = new ManualResetEventSlim(false);

            var t = taskManager.AddToQueue((_) => waitSlim.Wait());
            var t2 = taskManager.AddToQueue((_) => waitSlim.Wait());
            Thread.Sleep(0);

            if (t2.Status is not (TaskStatus.WaitingForActivation or TaskStatus.WaitingToRun))
            {
                Assert.Fail($"Expected either WaitingForAction or WaitingToRun, got: {t2.Status}");
            }

            waitSlim.Set();

            await Task.WhenAll(t, t2);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionContinuesTest()
        {
            var mockAction1 = new Mock<DPTaskManager.QueueAction>();
            var mockAction2 = new Mock<DPTaskManager.QueueAction>();
            var mockAction3 = new Mock<DPTaskManager.QueueAction>();

            var sequence = new MockSequence();

            mockAction1.InSequence(sequence).Setup(a => a(It.IsAny<CancellationToken>()));
            mockAction2.InSequence(sequence).Setup(a => a(It.IsAny<CancellationToken>()));
            mockAction3.InSequence(sequence).Setup(a => a(It.IsAny<CancellationToken>()));

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction1.Object);
            await taskManager.AddToQueue(mockAction2.Object);
            await taskManager.AddToQueue(mockAction3.Object);

            mockAction1.Verify(a => a(It.IsAny<CancellationToken>()), Times.Once);
            mockAction2.Verify(a => a(It.IsAny<CancellationToken>()), Times.Once);
            mockAction3.Verify(a => a(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1Test()
        {
            var mockAction = new Mock<DPTaskManager.QueueAction<int>>();

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction.Object, 1);

            mockAction.Verify(a => a(1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1ContinuesTest()
        {
            var mockAction1 = new Mock<DPTaskManager.QueueAction<int>>();
            var mockAction2 = new Mock<DPTaskManager.QueueAction<int>>();
            var mockAction3 = new Mock<DPTaskManager.QueueAction<int>>();

            var sequence = new MockSequence();

            mockAction1.InSequence(sequence).Setup(a => a(1, It.IsAny<CancellationToken>()));
            mockAction2.InSequence(sequence).Setup(a => a(2, It.IsAny<CancellationToken>()));
            mockAction3.InSequence(sequence).Setup(a => a(3, It.IsAny<CancellationToken>()));

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction1.Object, 1);
            await taskManager.AddToQueue(mockAction2.Object, 2);
            await taskManager.AddToQueue(mockAction3.Object, 3);

            mockAction1.Verify(a => a(1, It.IsAny<CancellationToken>()), Times.Once);
            mockAction2.Verify(a => a(2, It.IsAny<CancellationToken>()), Times.Once);
            mockAction3.Verify(a => a(3, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1T2Test()
        {
            var mockAction = new Mock<DPTaskManager.QueueAction<int, string>>();

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction.Object, 1, "test");

            mockAction.Verify(a => a(1, "test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1T2ContinuesTest()
        {
            var mockAction1 = new Mock<DPTaskManager.QueueAction<int, string>>();
            var mockAction2 = new Mock<DPTaskManager.QueueAction<int, string>>();
            var mockAction3 = new Mock<DPTaskManager.QueueAction<int, string>>();

            var sequence = new MockSequence();

            mockAction1.InSequence(sequence).Setup(a => a(1, "test", It.IsAny<CancellationToken>()));
            mockAction2.InSequence(sequence).Setup(a => a(2, "test2", It.IsAny<CancellationToken>()));
            mockAction3.InSequence(sequence).Setup(a => a(3, "test3", It.IsAny<CancellationToken>()));

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction1.Object, 1, "test");
            await taskManager.AddToQueue(mockAction2.Object, 2, "test2");
            await taskManager.AddToQueue(mockAction3.Object, 3, "test3");

            mockAction1.Verify(a => a(1, "test", It.IsAny<CancellationToken>()), Times.Once);
            mockAction2.Verify(a => a(2, "test2", It.IsAny<CancellationToken>()), Times.Once);
            mockAction3.Verify(a => a(3, "test3", It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1T2T3Test()
        {
            var mockAction = new Mock<DPTaskManager.QueueAction<int, string, bool>>();

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction.Object, 1, "test", true);

            mockAction.Verify(a => a(1, "test", true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1T2T3ContinuesTest()
        {
            var mockAction1 = new Mock<DPTaskManager.QueueAction<int, string, bool>>();
            var mockAction2 = new Mock<DPTaskManager.QueueAction<int, string, bool>>();
            var mockAction3 = new Mock<DPTaskManager.QueueAction<int, string, bool>>();

            var sequence = new MockSequence();

            mockAction1.InSequence(sequence).Setup(a => a(1, "test", true, It.IsAny<CancellationToken>()));
            mockAction2.InSequence(sequence).Setup(a => a(2, "test2", false, It.IsAny<CancellationToken>()));
            mockAction3.InSequence(sequence).Setup(a => a(3, "test3", true, It.IsAny<CancellationToken>()));

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction1.Object, 1, "test", true);
            await taskManager.AddToQueue(mockAction2.Object, 2, "test2", false);
            await taskManager.AddToQueue(mockAction3.Object, 3, "test3", true);

            mockAction1.Verify(a => a(1, "test", true, It.IsAny<CancellationToken>()), Times.Once);
            mockAction2.Verify(a => a(2, "test2", false, It.IsAny<CancellationToken>()), Times.Once);
            mockAction3.Verify(a => a(3, "test3", true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1T2T3T4Test()
        {
            var mockAction = new Mock<DPTaskManager.QueueAction<int, string, bool, double>>();

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction.Object, 1, "test", true, 3.14);

            mockAction.Verify(a => a(1, "test", true, 3.14, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1T2T3T4ContinuesTest()
        {
            var mockAction1 = new Mock<DPTaskManager.QueueAction<int, string, bool, double>>();
            var mockAction2 = new Mock<DPTaskManager.QueueAction<int, string, bool, double>>();
            var mockAction3 = new Mock<DPTaskManager.QueueAction<int, string, bool, double>>();

            var sequence = new MockSequence();

            mockAction1.InSequence(sequence).Setup(a => a(1, "test", true, 3.14, It.IsAny<CancellationToken>()));
            mockAction2.InSequence(sequence).Setup(a => a(2, "test2", false, 2.71, It.IsAny<CancellationToken>()));
            mockAction3.InSequence(sequence).Setup(a => a(3, "test3", true, 1.41, It.IsAny<CancellationToken>()));

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction1.Object, 1, "test", true, 3.14);
            await taskManager.AddToQueue(mockAction2.Object, 2, "test2", false, 2.71);
            await taskManager.AddToQueue(mockAction3.Object, 3, "test3", true, 1.41);

            mockAction1.Verify(a => a(1, "test", true, 3.14, It.IsAny<CancellationToken>()), Times.Once);
            mockAction2.Verify(a => a(2, "test2", false, 2.71, It.IsAny<CancellationToken>()), Times.Once);
            mockAction3.Verify(a => a(3, "test3", true, 1.41, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1T2T3T4T5Test()
        {
            var mockAction = new Mock<DPTaskManager.QueueAction<int, string, bool, double, char>>();

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction.Object, 1, "test", true, 3.14, 'a');

            mockAction.Verify(a => a(1, "test", true, 3.14, 'a', It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddToQueue_QueueActionT1T2T3T4T5ContinuesTest()
        {
            var mockAction1 = new Mock<DPTaskManager.QueueAction<int, string, bool, double, char>>();
            var mockAction2 = new Mock<DPTaskManager.QueueAction<int, string, bool, double, char>>();
            var mockAction3 = new Mock<DPTaskManager.QueueAction<int, string, bool, double, char>>();

            var sequence = new MockSequence();

            mockAction1.InSequence(sequence).Setup(a => a(1, "test", true, 3.14, 'a', It.IsAny<CancellationToken>()));
            mockAction2.InSequence(sequence).Setup(a => a(2, "test2", false, 2.71, 'b', It.IsAny<CancellationToken>()));
            mockAction3.InSequence(sequence).Setup(a => a(3, "test3", true, 1.41, 'c', It.IsAny<CancellationToken>()));

            var taskManager = new DPTaskManager();
            await taskManager.AddToQueue(mockAction1.Object, 1, "test", true, 3.14, 'a');
            await taskManager.AddToQueue(mockAction2.Object, 2, "test2", false, 2.71, 'b');
            await taskManager.AddToQueue(mockAction3.Object, 3, "test3", true, 1.41, 'c');

            mockAction1.Verify(a => a(1, "test", true, 3.14, 'a', It.IsAny<CancellationToken>()), Times.Once);
            mockAction2.Verify(a => a(2, "test2", false, 2.71, 'b', It.IsAny<CancellationToken>()), Times.Once);
            mockAction3.Verify(a => a(3, "test3", true, 1.41, 'c', It.IsAny<CancellationToken>()), Times.Once);
        }
        [TestMethod]
        public void StopTest()
        {
            var mockAction1 = new Mock<Action>();
            var mockAction2 = new Mock<Action>();
            var mockAction3 = new Mock<Action>();

            var taskManager = new DPTaskManager();

            // Add a delay to mockAction1 to ensure we have time to call Stop
            mockAction1.Setup(a => a()).Callback(() => Thread.Sleep(9999999));

            taskManager.AddToQueue(mockAction1.Object);
            taskManager.AddToQueue(mockAction2.Object);
            var task3 = taskManager.AddToQueue(mockAction3.Object);

            taskManager.Stop();

            mockAction1.Verify(a => a(), Times.Once);
            mockAction2.Verify(a => a(), Times.Never);
            mockAction3.Verify(a => a(), Times.Never);
            Assert.IsTrue(task3.IsCanceled);
        }

        [TestMethod]
        public void StopAndWaitTest()
        {
            var mockAction1 = new Mock<Action>();
            var mockAction2 = new Mock<Action>();
            var mockAction3 = new Mock<Action>();

            var taskManager = new DPTaskManager();

            // Add a delay to mockAction1 to ensure we have time to call Stop
            mockAction1.Setup(a => a()).Callback(() =>
            {
                Thread.Sleep(9999999);
            });

            taskManager.AddToQueue(mockAction1.Object);
            taskManager.AddToQueue(mockAction2.Object);
            taskManager.AddToQueue(mockAction3.Object);

            // Wait for all tasks to complete or be cancelled
            taskManager.StopAndWait();

            mockAction1.Verify(a => a(), Times.Once);
            mockAction2.Verify(a => a(), Times.Never);
            mockAction3.Verify(a => a(), Times.Never);
        }
    }
}
