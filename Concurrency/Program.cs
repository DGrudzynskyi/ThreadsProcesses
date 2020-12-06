using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrency
{
    class Program
    {
        public static Object theLocker = new object();

        static void Main(string[] args)
        {
            int theNumber = 0;
            ThreadStart action = () =>
            {
                for (int i = 0; i < 100000; i++)
                {
                    //lock (theLocker)
                    //{
                        theNumber = theNumber + 1;
                    //}
                    /*
                    Monitor.Enter(theLocker);
                    try
                    {
                        theNumber = theNumber + 1;
                    }
                    finally
                    {
                        Monitor.Exit(theLocker);
                    }*/
                }
            };

            var thread1 = new Thread(action);
            var thread2 = new Thread(action);
            var thread3 = new Thread(action);
            var thread4 = new Thread(action);

            thread1.Start();
            thread2.Start();
            thread3.Start();
            thread4.Start();

            var allThreadsAreDone = false;
            while (!allThreadsAreDone) {
                Console.WriteLine(theNumber);
                allThreadsAreDone = thread1.ThreadState == System.Threading.ThreadState.Stopped &&
                    thread2.ThreadState == System.Threading.ThreadState.Stopped &&
                    thread3.ThreadState == System.Threading.ThreadState.Stopped &&
                    thread4.ThreadState == System.Threading.ThreadState.Stopped;
            }

            Console.WriteLine("result:" + theNumber);
            Console.ReadLine();
        }
    }
}
