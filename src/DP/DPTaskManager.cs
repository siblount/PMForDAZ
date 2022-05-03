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


        private CancellationTokenSource _source;
        private TaskFactory _taskFactory;
        private CancellationToken _token;
        private Task lastTask;
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
        #region Queue methods

        public void AddToQueue(Action action)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(action, _token);
            } else
            {
                lastTask = lastTask.ContinueWith((_) => action(), t, _continuationOptions, _scheduler);
            }
        }
        public void AddToQueue(QueueAction action)
        { 
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => action(t));
            } else
            {
                lastTask = lastTask.ContinueWith((_) => action(t), t,_continuationOptions, _scheduler);
            }
        }

        public void AddToQueue<T>(QueueAction<T> action, T arg)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => action(arg, t));
            }
            else
            {
                lastTask = lastTask.ContinueWith((_) => action(arg, t), t, _continuationOptions, _scheduler);
            }
        }


        public void AddToQueue<T1, T2>(QueueAction<T1, T2> action, T1 arg1, T2 arg2)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => action(arg1, arg2, t));
            }
            else
            {
                lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, t), t, _continuationOptions, _scheduler);
            }
        }

        public void AddToQueue<T1, T2, T3>(QueueAction<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => action(arg1, arg2, arg3, t));
            }
            else
            {
                lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, arg3, t),
                                                t, _continuationOptions, _scheduler);
            }
        }

        public void AddToQueue<T1, T2, T3, T4>(QueueAction<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => action(arg1, arg2, arg3, arg4, t));
            }
            else
            {
                lastTask = lastTask.ContinueWith((_) => action(arg1, arg2, arg3, arg4, t),
                                                    t, _continuationOptions, _scheduler);
            }
        }

        public void AddToQueue<ReturnType>(Func<CancellationToken, ReturnType> func)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => func(t));
            }
            else
            {
                lastTask = lastTask.ContinueWith((_) => func(t),
                                                    t, _continuationOptions, _scheduler);
            }
        }
        public void AddToQueue<ReturnType, T1>(Func<T1, CancellationToken, ReturnType> func, T1 arg1)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => func(arg1, t));
            }
            else
            {
                lastTask = lastTask.ContinueWith((_) => func(arg1, t),
                                                    t, _continuationOptions, _scheduler);
            }
        }
        public void AddToQueue<ReturnType, T1, T2>(Func<T1, T2, CancellationToken, ReturnType> func, T1 arg1, T2 arg2)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => func(arg1, arg2, t));
            }
            else
            {
                lastTask = lastTask.ContinueWith((_) => func(arg1, arg2, t),
                                                    t, _continuationOptions, _scheduler);
            }
        }
        public void AddToQueue<ReturnType, T1, T2, T3>(Func<T1, T2, T3, CancellationToken, ReturnType> func, 
                                                        T1 arg1, T2 arg2, T3 arg3)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => func(arg1, arg2, arg3, t));
            }
            else
            {
                lastTask = lastTask.ContinueWith((_) => func(arg1, arg2, arg3, t),
                                                    t, _continuationOptions, _scheduler);
            }
        }
        public void AddToQueue<ReturnType, T1, T2, T3, T4>(Func<T1, T2, T3, T4, CancellationToken, ReturnType> func,
                                                       T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            CancellationToken t = _token;
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => func(arg1, arg2, arg3, arg4, t));
            }
            else
            {
                lastTask = lastTask.ContinueWith((_) => func(arg1, arg2, arg3, arg4, t),
                                                    t, _continuationOptions, _scheduler);
            }
        }
        #endregion

        public void Stop()
        {
            _source.Cancel();
            Reset();
        }

    }
}
