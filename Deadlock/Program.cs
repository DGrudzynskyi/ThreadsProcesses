using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Deadlock
{
    class Program
    {

        static void Main(string[] args)
        {
            var account1 = new Account("oleg");
            var account2 = new Account("sasha");


            var thread1 = new Thread(() => {
                Program.makeALotOfTransfers(account1, account2);
            });
            var thread2 = new Thread(() => {
                Program.makeALotOfTransfers(account2, account1);
            });


            thread1.Start();
            thread2.Start();

            var allThreadsAreDone = false;
            while (!allThreadsAreDone)
            {
                allThreadsAreDone = thread1.ThreadState == System.Threading.ThreadState.Stopped &&
                    thread2.ThreadState == System.Threading.ThreadState.Stopped;
            }

            Console.Write(account1.getStatus());
            Console.Write(account2.getStatus());

            Console.ReadLine();
        }

        public static void makeALotOfTransfers(Account acc1, Account acc2) {
            for (int i = 0; i < 100; i++)
            {
                acc1.transfer(acc2, 100);
            }
        }
    }

    class Account
    {
        private double balance;
        private string name;

        public Account(string name) {
            this.name = name;
        }

        public void withdraw(double amount)
        {
            balance -= amount;
        }

        public void deposit(double amount)
        {
            balance += amount;
        }

        public string getName() {
            return name;
        }
        public string getStatus()
        {
            return String.Format("user {0} has {1} in bank", this.name, this.balance);
        }

        public void transfer(Account to, double amount)
        {
            lock (this)
            {
                lock (to)
                {
                    this.withdraw(amount);
                    to.deposit(amount);
                    Console.WriteLine(String.Format("transferred {0} from {1} to {2}", amount, this.getName(), to.getName()));
                }
                var t = false;
            }
            var s = false;
        }

    }
}
