using System.Text;

namespace SynchronizationPrimitives;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            Console.WriteLine("\nПримитивы синхронизации\n");

            Console.WriteLine("1. SemaphoreSlim");
            Console.WriteLine("2. Producer-Consumer");
            Console.WriteLine("3. Mutex");
            Console.WriteLine("4. Interlocked");
            Console.WriteLine("0. Выход");

            Console.Write("\n Выберите эксперимент: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    {
                        var demo = new SemaphoreDemo();
                        await demo.RunAsync();
                        break;
                    }

                case "2":
                    {
                        using var demo = new ProducerConsumerDemo();
                        await demo.RunAsync();
                        break;
                    }

                case "3":
                    {
                        MutexDemo.Run();
                        break;
                    }

                case "4":
                    {
                        new InterlockedDemo().Run();
                        break;
                    }

                case "0":
                    {
                        return;
                    }

                default:
                    {
                        Console.WriteLine("Неизвестная команда.");
                        break;
                    }
            }
        }
    }
}