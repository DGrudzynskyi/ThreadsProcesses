using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManualThreadLocking
{
    class Program
    {
        public static readonly Object commanExecutinoLockingObject = new { };
        public static string commandText = "укеі";

        static void Main(string[] args)
        {
            new Thread(ProcessCommand).Start();
            Thread.Sleep(1000);
            new Thread(ReadUserCommand).Start();
        }

        static void ReadUserCommand() {
            lock (commanExecutinoLockingObject)
            {
                Console.WriteLine("please enter your command");
                commandText = Console.ReadLine();
            }

            Console.ReadKey();
            ReadUserCommand();
        }

        static void ProcessCommand() {
            while (true) {
                if (!String.IsNullOrEmpty(commandText))
                {
                    lock (commanExecutinoLockingObject)
                    {
                        if (commandText == "exit")
                        {
                            Process.GetCurrentProcess().Kill();
                        }

                    
                        Console.WriteLine("processing command: " + commandText);
                    }
                }
                Thread.Sleep(1000);
            }
        }
    }
}
