using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ThreadProcessApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //CallChildProcessSeparateWindow();
            CallChildProcessSameWindow();
        }

        static void CallChildProcessSeparateWindow() {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "SortItemsApp.exe";
                process.Start();

                // отпускаем какую то работу делаться в потоке, который является частью текущего процесса
                DoSomeWorkInAnotherThread();

                // ждём пока закончится дочерний процесс
                process.WaitForExit();
                Console.WriteLine("Child process finished");

                // ждём пока пользователь нажмёт любую кнопку и закрываем приложение
                Console.ReadKey();
            }
        }

        static void CallChildProcessSameWindow()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "SortItemsApp.exe";
                // запрещаем вызов консольки для запускаемового приложения
                process.StartInfo.UseShellExecute = false;
                // говорим что хотим перенаправить вывод сорт айтемс процесса в стрим, который мы контролируем
                process.StartInfo.RedirectStandardOutput = true;
                // раз консольку мы не запускаем - передаём какие то аргументы, 
                // т.е. теперь наш вызов равен вызову в консоли "SortItemsApp.exe a b x c"
                process.StartInfo.Arguments = "a b x c";
                process.Start();

                // отпускаем какую то работу делаться в потоке, который является частью текущего процесса
                DoSomeWorkInAnotherThread();

                // ждём пока закончится дочерний процесс
                
                // указываем стрим, в который писать дочернему процессу
                StreamReader reader = process.StandardOutput;
                process.WaitForExit();

                string output = reader.ReadToEnd();
                Console.Write(output);

                Console.WriteLine("Child process finished");
                Console.ReadKey();
            }
        }

        static void DoSomeWorkInAnotherThread() {
            new Thread(() =>
            {
                // будет закрыта как только закончит выполняться основной поток.
                // без этого флажка консолька бы висела до упора т.к. мы этот поток никогда не будет завершен
                Thread.CurrentThread.IsBackground = true;

                while (true)
                {
                    /* run your code here */
                    Thread.Sleep(1000);
                    Console.WriteLine("I am doing some work while main thread is locked....");
                }
            }).Start();
        }
    }
}
