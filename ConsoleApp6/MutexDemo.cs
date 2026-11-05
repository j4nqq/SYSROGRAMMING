namespace SynchronizationPrimitives;

public class MutexDemo
{
    private const string MutexName = "Global\\SynchronizationPrimitivesMutex";

    public void Run()
    {
        Console.WriteLine("=== MUTEX DEMO ===");

        using ManualResetEventSlim ownerReady = new(false);
        using ManualResetEventSlim ownerCanFinish = new(false);

        Thread owner = new(() =>
        {
            using Mutex mutex = new(false, MutexName);
            mutex.WaitOne();

            try
            {
                Console.WriteLine("Первый участник: захватил Mutex");
                ownerReady.Set();
                ownerCanFinish.Wait();
            }
            finally
            {
                mutex.ReleaseMutex();
                Console.WriteLine("Первый участник: освободил Mutex");
            }
        });

        owner.Start();
        ownerReady.Wait();

        using Mutex mutex = new(false, MutexName);

        TryEnter(mutex, "Второй участник (пока занято)");

        ownerCanFinish.Set();
        owner.Join();

        TryEnter(mutex, "Второй участник (после освобождения)");
    }

    private static void TryEnter(Mutex mutex, string name)
    {
        bool acquired = mutex.WaitOne(0);

        if (!acquired)
        {
            Console.WriteLine($"{name}: ресурс занят");
            return;
        }

        try
        {
            Console.WriteLine($"{name}: получил доступ к ресурсу");
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }
}
