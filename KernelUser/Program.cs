// code taken from https://devblogs.microsoft.com/oldnewthing/20170623-00/?p=96455
// for educatinoal purposes, thanks

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

class Program
{
    static int ITERATIONS = 1000;
    static CountdownEvent done = new CountdownEvent(ITERATIONS);
    static DateTime startTime = DateTime.Now;
    static TimeSpan totalLatency = TimeSpan.FromSeconds(0);
    static SynchronizedCollection<string> messages =
     new SynchronizedCollection<string>();

    static void Log(int id, DateTime queueTime, string action)
    {
        var now = DateTime.Now;
        var timestamp = now - startTime;
        var latency = now - queueTime;
        var msg = string.Format("{0}: {1} {2,3}, latency = {3}",
          timestamp, action, id, latency);
        messages.Add(msg);
        System.Console.WriteLine(msg);
    }

    static void OnTaskStart(int id, DateTime queueTime)
    {
        var latency = DateTime.Now - queueTime;
        lock (done) totalLatency += latency;
        Log(id, queueTime, "Starting");
    }

    static void OnTaskEnd(int id, DateTime queueTime)
    {
        Log(id, queueTime, "Finished");
        done.Signal();
    }

    public static void V1()
    {
        ThreadPool.SetMaxThreads(10, 10);
        for (int i = 0; i < ITERATIONS; i++)
        {
            var queueTime = DateTime.Now;
            int id = i;
            ThreadPool.QueueUserWorkItem((o) => {
                OnTaskStart(id, queueTime);
                Thread.Sleep(500);
                OnTaskEnd(id, queueTime);
            });
            //Thread.Sleep(10);
        }
    }

    public static void V2()
    {
        ThreadPool.SetMinThreads(1000, 1);
        ThreadPool.SetMaxThreads(1000, 1);
        for (int i = 0; i < ITERATIONS; i++)
        {
            var queueTime = DateTime.Now;
            int id = i;
            ThreadPool.QueueUserWorkItem((o) => {
                OnTaskStart(id, queueTime);
                Thread.Sleep(500);
                OnTaskEnd(id, queueTime);
            });
            //Thread.Sleep(10);
        }
    }

    public static void V3()
    {
        ThreadPool.SetMaxThreads(1, 1);
        for (int i = 0; i < ITERATIONS; i++)
        {
            var queueTime = DateTime.Now;
            int id = i;
            ThreadPool.QueueUserWorkItem(async (o) => {
                OnTaskStart(id, queueTime);
                await Task.Delay(500);
                OnTaskEnd(id, queueTime);
            });
            //Thread.Sleep(10);
        }
    }

    public static void Main(string[] args)
    {
        var commandKey = Console.ReadLine().ToString();
        switch (commandKey)
        {
            case "threads": V1(); break;
            case "manyThreads": V2(); break;
            case "tasks": V3(); break;
        }
        done.Wait();
        foreach (var message in messages)
        {
            System.Console.WriteLine(message);
        }
        System.Console.WriteLine(
         "Average latency = {0}",
         TimeSpan.FromMilliseconds(totalLatency.TotalMilliseconds / ITERATIONS));
        Console.ReadLine();
    }
}