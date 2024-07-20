// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

namespace DAZ_Installer.Core
{
    // Notes about Tasks, TaskFactory:
    // (1) Token still works even after being disposed.
    // (2) Source still semi-works after dispose call. Token is "disposed".
    // (3) Tasks will continue to run unless you explicitly use the token and current task scheduler.
    public struct DPTaskManager
    {
        public delegate void QueueAction(CancellationToken token);
        public delegate void QueueAction<in T>(T arg1, CancellationToken token);
        public delegate void QueueAction<in T1, in T2>(T1 arg1, T2 arg2, CancellationToken token);
        public delegate void QueueAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3, CancellationToken token);
        public delegate void QueueAction<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, CancellationToken token);
        public delegate void QueueAction<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, CancellationToken token);

        private CancellationTokenSource _source;
        private CancellationToken _token;
        private volatile Task? lastTask;
        private readonly object lockObj = new();
        // (3) Tasks will continue with continueWith() chain unless this is passed in.
        private const TaskContinuationOptions _continuationOptions = TaskContinuationOptions.NotOnCanceled;

        public DPTaskManager()
        {
            _source = new CancellationTokenSource();
            _token = _source.Token;
            lastTask = null;
        }

        private void Reset()
        {
            lock (lockObj)
            {
                _source.Dispose();
                _source = new CancellationTokenSource();
                _token = _source.Token;
                lastTask = null;
            }
        }

        public void Stop()
        {
            lock (lockObj)
            {
                _source.Cancel();
                Reset();
            }
        }

        public void StopAndWait()
        {
            lock (lockObj)
            {
                _source.Cancel();
                try
                {
                    lastTask?.Wait();
                } catch { }
                Reset();
            }
        }
        #region Queue methods

        public Task AddToQueue(Action action)
        {
            CancellationToken t = _token;
            Task task;
            lock (lockObj)
            {
                if (lastTask == null)
                {
                    task = lastTask = Task.Factory.StartNew(action, _token);
                }
                else
                {
                    task = lastTask = lastTask.ContinueWith((_) => action(), t, _continuationOptions, TaskScheduler.Current);
                }
            }
            return task;
        }
        public Task AddToQueue(QueueAction action)
        {
            CancellationToken t = _token;
            Task task;
            lock (lockObj)
            {
                if (lastTask == null)
                {
                    task = lastTask = Task.Factory.StartNew(() => action(t));
                }
                else
                {
                    task = lastTask = lastTask.ContinueWith((_) => action(t), t, _continuationOptions, TaskScheduler.Current);
                }
            }
            return task;
        }

        public Task AddToQueue<T>(QueueAction<T> action, T arg)
        {
            CancellationToken t = _token;
            Task task;
            lock (lockObj)
            {
                if (lastTask == null)
                {
                    task = lastTask = Task.Factory.StartNew(() => action(arg, t));
                }
                else
                {
                    task = lastTask = lastTask.ContinueWith((_) => action(arg, t), t, _continuationOptions, TaskScheduler.Current);
                }
            }
            return task;
        }


        public Task AddToQueue<T1, T2>(QueueAction<T1, T2> action, T1 arg1, T2 arg2)
        {
            CancellationToken t = _token;
            Task task;
            lock (lockObj)
            {
                if (lastTask == null)
                {
                    task = lastTask = Task.Factory.StartNew(() => action(arg1, arg2, t));
                }
                else
                {
                    task = lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, t), t, _continuationOptions, TaskScheduler.Current);
                }
            }
            return task;
        }

        public Task AddToQueue<T1, T2, T3>(QueueAction<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            CancellationToken t = _token;
            Task task = lastTask;
            lock (lockObj)
            {
                if (lastTask == null)
                {
                    task = lastTask = Task.Factory.StartNew(() => action(arg1, arg2, arg3, t));
                }
                else
                {
                    task = lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, arg3, t),
                                                    t, _continuationOptions, TaskScheduler.Current);
                }
            }
            return task;
        }

        public Task AddToQueue<T1, T2, T3, T4>(QueueAction<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            CancellationToken t = _token;
            Task task = lastTask;
            lock (lockObj)
            {
                if (lastTask == null)
                {
                    task = lastTask = Task.Factory.StartNew(() => action(arg1, arg2, arg3, arg4, t));
                }
                else
                {
                    task = lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, arg3, arg4, t),
                                                        t, _continuationOptions, TaskScheduler.Current);
                }
            }
            return task;
        }

        public Task AddToQueue<T1, T2, T3, T4, T5>(QueueAction<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            CancellationToken t = _token;
            Task task = lastTask;

            lock (lockObj)
            {
                if (lastTask == null)
                {
                    task = lastTask = Task.Factory.StartNew(() => action(arg1, arg2, arg3, arg4, arg5, t));
                }
                else
                {
                    task = lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, arg3, arg4, arg5, t),
                                                        t, _continuationOptions, TaskScheduler.Current);
                } 
            }
            return task;
        }
        #endregion



    }
}
