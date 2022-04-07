using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DAZ_Installer.DP
{
    internal struct DPTaskManager
    {
        private CancellationTokenSource _source;
        private TaskFactory _taskFactory;
        private CancellationToken _token;
        private Task lastTask;

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
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(action);
            } else
            {
                lastTask = lastTask.ContinueWith((t) => action());
            }
        }

        public void AddToQueue<T>(Action<T> action, T arg)
        {
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => action(arg));
            }
            else
            {
                lastTask = lastTask.ContinueWith((t) => action(arg));
            }
        }

        public void AddToQueue<T>(Action<T[]> action, params T[] args)
        {
            if (lastTask == null)
            {
                lastTask = Task.Factory.StartNew(() => action(args));
            }
            else
            {
                lastTask = lastTask.ContinueWith((t) => action(args));
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
