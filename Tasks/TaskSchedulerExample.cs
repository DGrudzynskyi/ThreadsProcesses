using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace Tasks
{
    public class TaskSchedulerExample
    {
        /// <summary>
        /// выполнение тасков кастомным планировщиком (TaskScheduler)
        /// </summary>
        public static void RunExample()
        {
            // кастомный планировщик тасков, выполняет все задачи не-более-чем-в-2 потоках
            LimitedConcurrencyLevelTaskScheduler lcts = new LimitedConcurrencyLevelTaskScheduler(2);
            List<Task> tasks = new List<Task>();

            // фабрика, которая все таски будет стартовать с кастомным планировщиком
            TaskFactory factory = new TaskFactory(lcts);

            //ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);

            // Use our factory to run a set of tasks.
            Object lockObj = new Object();
            int outputItem = 0;

            // старт 4 тасков, каждый имеет 30 итераций, на каждой итерации задержка 50мс
            for (int tCtr = 0; tCtr <= 4; tCtr++)
            {
                int iteration = tCtr;
                var t = factory.StartNew(async () =>
                {
                    for (int i = 0; i < 30; i++)
                    {
                        lock (lockObj)
                        {
                            Console.Write("{0} in task t-{1} on thread {2}   ",
                                          i, iteration, Thread.CurrentThread.ManagedThreadId);
                            outputItem++;
                            if (outputItem % 3 == 0)
                                Console.WriteLine();
                        }

                        await Task.Delay(50); // обратите внимание на изменение времени выполнения программы если заменить на Task.Delay(50).Wait()
                    }
                }).Unwrap();
                tasks.Add(t);
            }

            // старт ещё 4 тасков, каждый имеет 40 итераций, на каждой итерации задержка 50мс
            for (int tCtr = 0; tCtr <= 4; tCtr++)
            {
                int iteration = tCtr;
                var t1 = factory.StartNew(async () =>
                {
                    //return Task.Run(async () =>
                    //{
                        for (int outer = 0; outer <= 8; outer++)
                        {
                            for (int i = 50; i < 55; i++)
                            {
                                lock (lockObj)
                                {
                                    Console.Write("{0} in task t1-{1} on thread {2}   ",
                                                  i, iteration, Thread.CurrentThread.ManagedThreadId);
                                    outputItem++;
                                    if (outputItem % 3 == 0)
                                        Console.WriteLine();
                            }
                            await Task.Delay(50); // обратите внимание на изменение времени выполнения программы если заменить на Task.Delay(50).Wait()
                        }
                        }
                    //});
                }).Unwrap();
                tasks.Add(t1);
            }

            Stopwatch mywatch = new Stopwatch();
            mywatch.Start();

            // Wait for the tasks to complete before displaying a completion message.
            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("\n\nSuccessful completed in: " + mywatch.ElapsedMilliseconds + "ms");
        }
    }

    // Provides a task scheduler that ensures a maximum concurrency level while
    // running on top of the thread pool.
    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        // Indicates whether the current thread is processing work items.
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        // The list of tasks to be executed
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        // The maximum concurrency level allowed by this scheduler.
        private readonly int _maxDegreeOfParallelism;

        // Indicates whether the scheduler is currently processing work items.
        private int _delegatesQueuedOrRunning = 0;

        // Creates a new instance with the specified degree of parallelism.
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        // Queues a task to the scheduler.
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        // Inform the ThreadPool that there's work to be executed for this scheduler.
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
            // Note that the current thread is now processing work items.
            // This is necessary to enable inlining of tasks into this thread.
            _currentThreadIsProcessingItems = true;
                try
                {
                // Process all available items in the queue.
                while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                        // When there are no more items to be processed,
                        // note that we're done processing, and get out.
                        if (_tasks.Count == 0)
                            {
                                //--_delegatesQueuedOrRunning;
                                //break;
                                continue;
                            }

                        // Get the next item from the queue
                        item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                    // Execute the task we pulled out of the queue
                    base.TryExecuteTask(item);
                    }
                }
            // We're done processing items on the current thread
            finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        // Attempts to execute the specified task on the current thread.
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued)
                // Try to run the task.
                if (TryDequeue(task))
                    return base.TryExecuteTask(task);
                else
                    return false;
            else
                return base.TryExecuteTask(task);
        }

        // Attempt to remove a previously scheduled task from the scheduler.
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // Gets the maximum concurrency level supported by this scheduler.
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        // Gets an enumerable of the tasks currently scheduled on this scheduler.
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }
}