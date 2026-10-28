using System;
using System.Threading;

public class Program
{
    private static int completedOrders = 0;
    private static int safeCompletedOrders = 0;

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        const int threadCount = 8;
        const int incrementsPerThread = 100_000;
        int expected = threadCount * incrementsPerThread;

        Console.WriteLine("=== Обычный инкремент ===");
        Run(threadCount, incrementsPerThread, () => completedOrders++);
        Console.WriteLine($"Ожидалось: {expected}");
        Console.WriteLine($"Получено: {completedOrders}");

        Console.WriteLine();
        Console.WriteLine("=== Interlocked.Increment ===");
        Run(threadCount, incrementsPerThread,
            () => Interlocked.Increment(ref safeCompletedOrders));
        Console.WriteLine($"Ожидалось: {expected}");
        Console.WriteLine($"Получено: {safeCompletedOrders}");
    }

    private static void Run(int threadCount, int incrementsPerThread, Action action)
    {
        Thread[] threads = new Thread[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < incrementsPerThread; j++)
                {
                    action();
                }
            });
        }

        foreach (Thread thread in threads)
        {
            thread.Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }
    }
}