using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tasks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //TaskRun();
            //ContinueWithCancellation();
            //AwaitExample();
            //AwaitAllExample();
            //ContinueWithExample();
            //ContinueWithAllExample();
            TaskSchedulerExample.RunExample();
            Console.ReadLine();
        }

        /// <summary>
        /// простейшее создание тасков через Task.Run или конструктор
        /// </summary>
        /// <returns></returns>
        public static async Task<int> TaskRun() {
            Console.WriteLine("Application thread ID: {0}",
                            Thread.CurrentThread.ManagedThreadId);

            // так можно. Создаётся и сразу же запускается
            //var t = Task.Run(() =>
            //{
            //    Console.WriteLine("Task thread ID: {0}",
            //       Thread.CurrentThread.ManagedThreadId);
            //});


            // и так тоже можно. в єтом случае после создания нужно явно запустить
            var t = new Task<int>(() =>
            {
                Console.WriteLine("Task thread ID: {0}",
                   Thread.CurrentThread.ManagedThreadId);
                return 2;
            });
            t.Start();
            var res = await t;

            // блокирующее ожидание, может быть использовано вместо await с блокировкой вызывающего потока
            // t.Wait();

            return res;
        }

        /// <summary>
        /// cancellation token отменяется(cancell) из главного потока через 100мс. Часть запланированных задач тоже будет отменена
        /// </summary>
        public static void ContinueWithCancellation()
        {
            var cancellatinoTokenSource = new CancellationTokenSource();

            cancellatinoTokenSource.Token.Register(() => Console.WriteLine("Here cancellation is requested"));
            for (int i = 0; i < 10; i++)
            {
                GenerateRandomNumber(cancellatinoTokenSource.Token).ContinueWith((x, index) =>
                {
                    Console.WriteLine(x.Status);
                    if (x.Status != TaskStatus.Canceled)
                    {
                        Console.WriteLine("result for task {0} = {1}", index, x.Result);
                    }
                    // если CancellationToken передаётся в ContinueWith аргументом, и в момент когда GenerateRandomNumber закончен, кенслейшн токен уже отменён
                    // - континьюейшн таск не будет выполнен вообще.
                    // если CancellationToken НЕ передаётся в ContinueWith аргументом, и в момент когда GenerateRandomNumber закончен, кенслейшн токен уже отменён
                    // - континьюейшн такс будет выполнен, понять что он был отменён можно из поля task.Status
                }, i);// , cancellatinoTokenSource.Token);
            };

            Thread.Sleep(50);
            Console.WriteLine("wait 100 ms in main thread");
            cancellatinoTokenSource.Cancel();
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");

            Console.ReadLine();
        }

        public static void ContinueWithExample() {
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var taask = GenerateRandomNumber().ContinueWith((Task<int> x, object index) => //
                    {
                        Console.WriteLine(x.Status);
                        // подставить i вместо индекса
                        Console.WriteLine("result for task {0} = {1}", index, x.Result);
                        // throw new InvalidOperationException();
                    }, i);

                    // заметьте что taask.Exception сожержит не InvalidOperationException а AggregateException
                    var checkAggregateException = taask.Exception;
                }
                catch (Exception e) {
                    // заметьте что try catch не отрабатывает для тасков т.к. они не выполнены в текущем потоке (что лечится использовнием async await)
                    var t = e;
                }
            };

            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");

            Console.ReadLine();
        }

        /// <summary>
        /// пример ожидания всех тасков и выполнения следующего кода только после того как все таски выполнены, ожидания распаралеливаются
        /// </summary>
        public static void ContinueWithAllExample()
        {
            var tasks = new List<Task<int>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(GenerateRandomNumber());
            };

            Task.Factory.ContinueWhenAll(tasks.ToArray(), (completedTasks) =>
            {
                foreach (var task in completedTasks) {
                    Console.WriteLine("result for task = {0}", task.Result);
                }
            });

            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");

            Console.ReadLine();
        }

        /// <summary>
        /// пример ожидания каждого из тасков последовательно, ожидания НЕ распаралеливаются
        /// </summary>
        public static async Task AwaitExample()
        {
            Stopwatch mywatch = new Stopwatch();
            mywatch.Start();
            for (int i = 0; i < 10; i++)
            {
                var result = await GenerateRandomNumber();
                Console.WriteLine("result for task {0} = {1}", i, result);
            };
            mywatch.Stop();
            Console.WriteLine("Time consumed is : " + mywatch.ElapsedMilliseconds.ToString());

            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");

            Console.ReadLine();
        }

        /// <summary>
        /// пример ожидания всех тасков и выполнения следующего кода только после того как все таски выполнены, ожидания распаралеливаются
        /// </summary>
        public static async Task AwaitAllExample()
        {

            Stopwatch mywatch = new Stopwatch();
            mywatch.Start();
            var tasks = new List<Task<int>>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(GenerateRandomNumber());
            };

            var taskResults = await Task.WhenAll(tasks);

            foreach(var result in taskResults)
            {
                Console.WriteLine("result for task: {0}", result);
            }
            mywatch.Stop();
            Console.WriteLine("Time consumed is : " + mywatch.ElapsedMilliseconds.ToString());

            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");

            Console.ReadLine();
        }


        public static async Task<int> GenerateRandomNumber() {
            var number = new Random().Next(1, 100);

            await Task.Delay(number);
            //throw new InvalidOperationException();

            // возвращаемое значение автоматически врапится в Task
            return number;
        }

        // можно так - идентично предыдущему GenerateRandomNumber но без использования синтаксиса async/await
        //public static Task<int> GenerateRandomNumberNonAsync()
        //{
        //    var number = new Random().Next(1, 100);

        //    return Task.Delay(number).ContinueWith((prevTask) => number);
        //}

        public static async Task<int> GenerateRandomNumber(CancellationToken cancellationToken)
        {
            var number = new Random().Next(1, 100);

            await Task.Delay(number);

            // стандартный паттерн при использовании CancellationToken - перед выполнением работы проверять не отменёнё ли токен. 
            // Если отменён - кидать специальный ексепшн OperationCanceledException вместо выполнения всех последующих действий.
            cancellationToken.ThrowIfCancellationRequested();

            // тут может быть какая то ещё работа

            // возвращаемое значение автоматически врапится в Task
            return number;
        }
    }
}
