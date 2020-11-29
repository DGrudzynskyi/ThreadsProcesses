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
            var queue = new LIFOQueue(100);

            ThreadPool.QueueUserWorkItem((o) =>
            {
                for (int i = 0; i < 10000; i++) {
                    queue.Put(i.ToString());
                    queue.Pick();
                }
                Console.WriteLine("1");
            });

            ThreadPool.QueueUserWorkItem((o) =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    queue.Put(i.ToString());
                    queue.Pick();
                }
                Console.WriteLine("2");
            });

            Console.ReadLine();
        }
    }

    interface IQueue {
        string Pick();

        void Put(string putIt);
    }

    class LIFOQueue : IQueue
    {
        private string[] holder;
        private int length;

        public LIFOQueue(int capacity) {
            holder = new string[capacity];
        }

        public string Pick()
        {
                var retIt = holder[length];
                length--;
                return retIt;
            
        }

        public void Put(string putIt)
        {
                holder[length] = putIt;
                length++;
           
        }
    }
}
