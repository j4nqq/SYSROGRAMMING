namespace DeadlockLab;

public class DeadlockDemo
{
    private const int JoinTimeoutMs = 5000;

    public static void Run(bool sameOrder = false, bool useBarrier = true)
    {
        Console.WriteLine("=== DEADLOCK DEMO ===");

        object lockA = new();
        object lockB = new();

        using Barrier barrier = new(2);

        Thread threadA = new(() =>
        {
            Console.WriteLine("Поток A: ожидает ресурс A");

            lock (lockA)
            {
                Console.WriteLine("Поток A: захватил ресурс A");

                if (useBarrier)
                    barrier.SignalAndWait();

                Console.WriteLine("Поток A: ожидает ресурс B");

                lock (lockB)
                {
                    Console.WriteLine("Поток A: получил оба ресурса");
                }
            }
        });

        object firstForB = sameOrder ? lockA : lockB;
        object secondForB = sameOrder ? lockB : lockA;
        string firstNameForB = sameOrder ? "A" : "B";
        string secondNameForB = sameOrder ? "B" : "A";

        Thread threadB = new(() =>
        {
            Console.WriteLine($"Поток B: ожидает ресурс {firstNameForB}");

            lock (firstForB)
            {
                Console.WriteLine($"Поток B: захватил ресурс {firstNameForB}");

                if (useBarrier)
                    barrier.SignalAndWait();

                Console.WriteLine($"Поток B: ожидает ресурс {secondNameForB}");

                lock (secondForB)
                {
                    Console.WriteLine("Поток B: получил оба ресурса");
                }
            }
        });

        threadA.IsBackground = true;
        threadB.IsBackground = true;

        threadA.Start();
        threadB.Start();

        bool completedA = threadA.Join(JoinTimeoutMs);
        bool completedB = threadB.Join(JoinTimeoutMs);

        Console.WriteLine();

        if (!completedA || !completedB)
        {
            Console.WriteLine("DEADLOCK ОБНАРУЖЕН!");
            Console.WriteLine($"Потоки не завершились за {JoinTimeoutMs / 1000} секунд.");
        }
        else
        {
            Console.WriteLine("Оба потока завершились.");
        }
    }

    public static void RunThreeResources()
    {
        Console.WriteLine("=== DEADLOCK DEMO (3 ресурса) ===");

        object lockA = new();
        object lockB = new();
        object lockC = new();

        using Barrier barrier = new(3);

        Thread thread1 = CreateThread("1", lockA, "A", lockB, "B", barrier);
        Thread thread2 = CreateThread("2", lockB, "B", lockC, "C", barrier);
        Thread thread3 = CreateThread("3", lockC, "C", lockA, "A", barrier);

        thread1.Start();
        thread2.Start();
        thread3.Start();

        bool completed1 = thread1.Join(JoinTimeoutMs);
        bool completed2 = thread2.Join(JoinTimeoutMs);
        bool completed3 = thread3.Join(JoinTimeoutMs);

        Console.WriteLine();

        if (!completed1 || !completed2 || !completed3)
        {
            Console.WriteLine("DEADLOCK ОБНАРУЖЕН!");
            Console.WriteLine($"Потоки не завершились за {JoinTimeoutMs / 1000} секунд.");
        }
        else
        {
            Console.WriteLine("Все потоки завершились.");
        }
    }

    private static Thread CreateThread(
        string name,
        object first,
        string firstName,
        object second,
        string secondName,
        Barrier barrier)
    {
        Thread thread = new(() =>
        {
            Console.WriteLine($"Поток {name}: ожидает ресурс {firstName}");

            lock (first)
            {
                Console.WriteLine($"Поток {name}: захватил ресурс {firstName}");

                barrier.SignalAndWait();

                Console.WriteLine($"Поток {name}: ожидает ресурс {secondName}");

                lock (second)
                {
                    Console.WriteLine($"Поток {name}: получил оба ресурса");
                }
            }
        });

        thread.IsBackground = true;

        return thread;
    }
}
