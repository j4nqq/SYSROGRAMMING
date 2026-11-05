namespace SynchronizationPrimitives;

public class MutexDemo
{
    public static void Run()
    {
        Console.WriteLine(" === MUTEX Демо ===");

        using Mutex mutex = new(
            initiallyOwned: false,
            name: "Local\\SynchronizationPrimitivesDemo"
        );

        bool acquired = mutex.WaitOne(0);

        if (!acquired)
        {
            Console.WriteLine(
                "Другой экземпляр приложения уже работает."
            );
            return;
        }

        try
        {
            Console.WriteLine(
                "Блокировка получена. Ресурс занят."
            );
            Console.WriteLine(
                "Нажмите Enter для освобождения блокировки."
            );

            Console.ReadLine();
        }
        finally
        {
            mutex.ReleaseMutex();
            Console.WriteLine("Блокировка освобождена.");
        }
    }
}
