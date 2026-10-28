using System;
using System.Diagnostics;
using System.Threading;

public record Order(int Id, decimal Amount);

public class Program
{
    private static void ProcessOrder(Order order)
    {
        Console.WriteLine(
            $"Заказ {order.Id}: поток " +
            $"{Environment.CurrentManagedThreadId}"
        );

        Thread.Sleep(100);

        decimal total = order.Amount * 1.05m;

        Console.WriteLine(
            $"Заказ {order.Id} завершён. Сумма: {total:F2}"
        );
    }

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Order[] orders =
        {
            new(1, 100),
            new(2, 200),
            new(3, 300),
            new(4, 400),
            new(5, 500),
            new(6, 600),
            new(7, 700),
            new(8, 800)
        };

        Console.WriteLine("Последовательная обработка");
        Stopwatch sw = Stopwatch.StartNew();

        foreach (Order order in orders)
        {
            ProcessOrder(order);
        }

        sw.Stop();
        TimeSpan sequentialTime = sw.Elapsed;

        Console.WriteLine();
        Console.WriteLine("Многопоточная обработка");
        Thread[] threads = new Thread[orders.Length];

        sw.Restart();

        for (int i = 0; i < orders.Length; i++)
        {
            Order current = orders[i];

            threads[i] = new Thread(() =>
            {
                ProcessOrder(current);
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

        sw.Stop();
        TimeSpan parallelTime = sw.Elapsed;

        Console.WriteLine();
        Console.WriteLine($"Последовательно: {sequentialTime}");
        Console.WriteLine($"Многопоточно:    {parallelTime}");
    }
}