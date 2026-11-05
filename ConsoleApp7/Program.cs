using System.Text;

namespace DeadlockLab;

internal class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            Console.WriteLine("\n === Deadlock Lab === \n");

            Console.WriteLine("1. Deadlock (Barrier)");
            Console.WriteLine("2. Одинаковый порядок захвата");
            Console.WriteLine("3. Deadlock без Barrier");
            Console.WriteLine("4. Deadlock на трёх ресурсах");
            Console.WriteLine("0. Выход");

            Console.Write("\n Выберите эксперимент: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                {
                    DeadlockDemo.Run();
                    break;
                }

                case "2":
                {
                    DeadlockDemo.Run(sameOrder: true, useBarrier: false);
                    break;
                }

                case "3":
                {
                    DeadlockDemo.Run(useBarrier: false);
                    break;
                }

                case "4":
                {
                    DeadlockDemo.RunThreeResources();
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
