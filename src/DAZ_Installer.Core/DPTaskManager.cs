// This code is licensed under the Keep It Free License V1.
// You may find a full copy of this license at root project directory\LICENSE

using System;
using System.Threading;
using System.Threading.Tasks;

namespace DAZ_Installer.DP
{
    // Notes about Tasks, TaskFactory:
    // (1) Token still works even after being disposed.
    // (2) Source still semi-works after dispose call. Token is "disposed".
    // (3) Tasks will continue to run unless you explicitly use the token and current task scheduler.
    internal struct DPTaskManager
    {
        internal delegate void QueueAction(CancellationToken token);
        internal delegate void QueueAction<in T>(T arg1, CancellationToken token);
        internal delegate void QueueAction<in T1, in T2>(T1 arg1, T2 arg2, CancellationToken token);
        internal delegate void QueueAction<in T1, in T2, in T3>(T1 arg1, T2 arg2, T3 arg3, CancellationToken token);
        internal delegate void QueueAction<in T1, in T2, in T3, in T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, CancellationToken token);
        internal delegate void QueueAction<in T1, in T2, in T3, in T4, in T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, CancellationToken token);



        private CancellationTokenSource _source;
        private TaskFactory _taskFactory;
        private CancellationToken _token;
        private volatile Task lastTask;
        // (3) Tasks will continue with continueWith() chain unless this is passed in.
        private const TaskContinuationOptions _continuationOptions = TaskContinuationOptions.NotOnCanceled;
        
        // According to documentation, Scheduler may (and usually is) null, if null use Current property.
        // Check each time by using property instead of raw field.
        private TaskScheduler _scheduler {
            get => _taskFactory.Scheduler ?? TaskScheduler.Current;
        }

        public DPTaskManager()
        {
            _source = new CancellationTokenSource();
            _token = _source.Token;
            _taskFactory = new TaskFactory(_token);
            lastTask = null;
        }
    
        private void Reset()
        {
            _source.Dispose();
            _source = new CancellationTokenSource();
            _token = _source.Token;
            _taskFactory = new TaskFactory(_token);
            lastTask = null;
        }

        public void Stop()
        {
            _source.Cancel();
            Reset();
        }

        public void StopAndWait()
        {
            _source.Cancel();
            lastTask?.Wait();
            Reset();
        }
        #region Queue methods

        public Task AddToQueue(Action action)
        {
            CancellationToken t = _token;
            Task task;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(action, _token);
            } else
            {
                task = lastTask = lastTask.ContinueWith((_) => action(), t, _continuationOptions, _scheduler);
            }
            return task;
        }
        public Task AddToQueue(QueueAction action)
        { 
            CancellationToken t = _token;
            Task task;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => action(t));
            } else
            {
                task = lastTask = lastTask.ContinueWith((_) => action(t), t,_continuationOptions, _scheduler);
            }
            return task;
        }

        public Task AddToQueue<T>(QueueAction<T> action, T arg)
        {
            CancellationToken t = _token;
            Task task;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => action(arg, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => action(arg, t), t, _continuationOptions, _scheduler);
            }
            return task;
        }


        public Task AddToQueue<T1, T2>(QueueAction<T1, T2> action, T1 arg1, T2 arg2)
        {
            CancellationToken t = _token;
            Task task;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => action(arg1, arg2, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, t), t, _continuationOptions, _scheduler);
            }
            return task;
        }

        public Task AddToQueue<T1, T2, T3>(QueueAction<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            CancellationToken t = _token;
            Task task = lastTask;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => action(arg1, arg2, arg3, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, arg3, t),
                                                t, _continuationOptions, _scheduler);
            }
            return task;
        }

        public Task AddToQueue<T1, T2, T3, T4>(QueueAction<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            CancellationToken t = _token;
            Task task = lastTask;

            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => action(arg1, arg2, arg3, arg4, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, arg3, arg4, t),
                                                    t, _continuationOptions, _scheduler);
            }
            return task;
        }

        public Task AddToQueue<T1, T2, T3, T4, T5>(QueueAction<T1, T2, T3, T4, T5> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            CancellationToken t = _token;
            Task task = lastTask;

            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => action(arg1, arg2, arg3, arg4, arg5, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, arg3, arg4, arg5, t),
                                                    t, _continuationOptions, _scheduler);
            }
            return task;
        }

        public Task AddToQueue<ReturnType>(Func<CancellationToken, ReturnType> func)
        {
            CancellationToken t = _token;
            Task task = lastTask;

            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => func(t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => func(t),
                                                    t, _continuationOptions, _scheduler);
            }
            return task;
        }
        public Task AddToQueue<ReturnType, T1>(Func<T1, CancellationToken, ReturnType> func, T1 arg1)
        {
            CancellationToken t = _token;
            Task task = lastTask;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => func(arg1, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => func(arg1, t),
                                                    t, _continuationOptions, _scheduler);
            }
            return task;
        }
        public Task AddToQueue<ReturnType, T1, T2>(Func<T1, T2, CancellationToken, ReturnType> func, T1 arg1, T2 arg2)
        {
            CancellationToken t = _token;
            Task task = lastTask;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => func(arg1, arg2, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => func(arg1, arg2, t),
                                                    t, _continuationOptions, _scheduler);
            }
            return task;
        }
        public Task AddToQueue<ReturnType, T1, T2, T3>(Func<T1, T2, T3, CancellationToken, ReturnType> func, 
                                                        T1 arg1, T2 arg2, T3 arg3)
        {
            CancellationToken t = _token;
            Task task = lastTask;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => func(arg1, arg2, arg3, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => func(arg1, arg2, arg3, t),
                                                    t, _continuationOptions, _scheduler);
            }
            return task;
        }
        public Task AddToQueue<ReturnType, T1, T2, T3, T4>(Func<T1, T2, T3, T4, CancellationToken, ReturnType> func,
                                                       T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            CancellationToken t = _token;
            Task task = lastTask;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => func(arg1, arg2, arg3, arg4, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => func(arg1, arg2, arg3, arg4, t),
                                                    t, _continuationOptions, _scheduler);
            }
            return task;
        }
        public Task AddToQueue<ReturnType, T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, CancellationToken, ReturnType> func,
                                                       T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            CancellationToken t = _token;
            Task task = lastTask;
            if (lastTask == null)
            {
                task = lastTask = Task.Factory.StartNew(() => func(arg1, arg2, arg3, arg4, arg5, t));
            }
            else
            {
                task = lastTask = lastTask.ContinueWith((_) => func(arg1, arg2, arg3, arg4, arg5, t),
                                                    t, _continuationOptions, _scheduler);
            }
            return task;
        }
        #endregion



    }
}
