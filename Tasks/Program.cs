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
    class Program
    {
        static void Main(string[] args)
        {
            var mywatch = new Stopwatch();
            mywatch.Start();
            var fibb = Fibbon(0, 1, 1000000);
            mywatch.Stop();
            Console.WriteLine(fibb);
            //ContinueWithCancellatino();
            //AwaitExample();
            //AwaitAllExample();
            //ContinueWithExample();
            //ContinueWithAllExample();
            Console.WriteLine("Time consumed is : " + mywatch.ElapsedMilliseconds.ToString());
            Console.ReadLine();
        }

        public static long Fibbon(long i, long k, long n) {
            var prevValue = i;
            var curValue = k;
            var summ = 0;

            for (; n >= 0; n--) {
                var nextValue = prevValue + curValue;
                prevValue = curValue;
                curValue = nextValue;
            };
            return curValue;
        }


        public static void ContinueWithCancellatino()
        {
            var cancellatinoTokenSource = new CancellationTokenSource();
            cancellatinoTokenSource.Token.Register(() => Console.WriteLine("Here cancellation is requested"));
            for (int i = 0; i < 10; i++)
            {
                GenerateRandomNumber(cancellatinoTokenSource.Token).ContinueWith((x, index) =>
                {
                    // подставить i вместо индекса
                    Console.WriteLine(x.Status);
                    if (x.Status != TaskStatus.Canceled)
                    {
                        Console.WriteLine("result for task {0} = {1}", index, x.Result);
                    }
                }, i
                );
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
                //try
                //{
                var taask = GenerateRandomNumber().ContinueWith((Task<int> x, object index) => //
                    {
                        Console.WriteLine(x.Status);
                        // подставить i вместо индекса
                        Console.WriteLine("result for task {0} = {1}", index, x.Result);
                    }, i
                    );

                //}
                //catch (Exception e) {
                    // catchIt
                //}
            };

            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");
            Thread.Sleep(100);
            Console.WriteLine("wait 100 ms in main thread");

            Console.ReadLine();
        }

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

            // throw new InvalidOperationException();

            // возвращаемое значение автоматически врапится в Task
            return number;
        }

        public static async Task<int> GenerateRandomNumber(CancellationToken cancellationToken)
        {
            var number = new Random().Next(1, 100);

            await Task.Delay(number);

            cancellationToken.ThrowIfCancellationRequested();

            // возвращаемое значение автоматически врапится в Task
            return number;
        }
    }
}
