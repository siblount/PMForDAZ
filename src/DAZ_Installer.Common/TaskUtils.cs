using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAZ_Installer.Common
{
    /// <summary>
    /// A class containing utility methods for tasks.
    /// </summary>
    public static class TaskUtils
    {
        /// <summary>
        /// Executes the tasks in parallel on the thread pool <paramref name="n"/> times executing <paramref name="action"/> each time.
        /// Tasks will be chunked into <paramref name="chunkSize"/> sized chunks.
        /// </summary>
        /// <param name="n">The amount of times to execute the action sequentially.</param>
        /// <param name="chunkSize">The size of each chunk.</param>
        /// <param name="action">The action to perform.</param>
        public static Task ExecuteInParallel(int n, byte chunkSize, Action<int> action)
        {
            var arr = Enumerable.Range(0, n).ToArray();
            var chunks = arr.Chunk(chunkSize);
            var tasks = chunks.Select(chunk => Task.Run(() =>
            {
                foreach (var i in chunk)
                {
                    action(i);
                }
            }));

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Executes the task sequentially on the thread pool <paramref name="n"/> times executing <paramref name="action"/> each time.
        /// </summary>
        /// <param name="n">The amount of times to execute the action sequentially.</param>
        /// <param name="action">The action to perform.</param>
        /// <returns>A list of tasks.</returns>
        public static List<Task> ExecuteTasksSequentially(uint n, Action action)
        {
            List<Task> tasks = new((int)n);
            Task? lastTask = null;
            for (uint j = 0; j < n; j++)
            {
                uint i = j;
                if (lastTask is not null) tasks.Add(lastTask = lastTask.ContinueWith(_ =>
                {
                    Console.WriteLine(i);
                    action();
                }));
                else tasks.Add(lastTask = Task.Factory.StartNew(() =>
                {
                    Console.WriteLine(i);
                    action();
                }));
            }
            return tasks;
        }
    }
}
