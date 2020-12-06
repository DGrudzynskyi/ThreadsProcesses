using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Queues
{
    class Program
    {
        static void Main(string[] args)
        {
            var queue = new FIFOQueue(10000);

            // uncomment below to observe concurrency issues even while all methods of FIFOQueue using locks inside
            //for (int i = 0; i < 10000; i++) {
            //    queue.Put(i);
            //}
            //ThreadPool.QueueUserWorkItem((o) => DequeueWhileExists(queue));
            //ThreadPool.QueueUserWorkItem((o) => DequeueWhileExists(queue));


            ThreadPool.QueueUserWorkItem((o) => PutThenPick(queue));
            ThreadPool.QueueUserWorkItem((o) => PutThenPick(queue));

            Thread.Sleep(2000);

            Console.ReadLine();
        }

        static void PutThenPick(FIFOQueue queue)
        {
            for (int i = 0; i < 10000; i++)
            {
                queue.Put(i);
                queue.Pick();
            }
            Console.WriteLine("done");
        }

        static void DequeueWhileExists(FIFOQueue queue) {
            while (true)
            {
                if (queue.Count() > 0)
                {
                    queue.Pick();
                }
            }
        }
    }

    interface IQueue {
        int Pick();

        void Put(int putIt);
    }

    class FIFOQueue : IQueue
    {
        private int[] holder;
        private int length;

        public FIFOQueue(int capacity) {
            holder = new int[capacity];
        }

        public int Count()
        {
            return this.length;
        }

        public int Pick()
        {
            lock (holder)
            {
                var retIt = holder[length - 1];
                length--;
                return retIt;
            }
        }

        public void Put(int putIt)
        {
            lock (holder)
            {
                if (length == holder.Length) {
                    return;
                }

                for (int i = length - 1; i >= 0; i--) {
                    holder[i + 1] = holder[i];
                }
                holder[0] = putIt;
                length++;
            }
        }
    }
}
