using ConsoleApp11.Services;

namespace ConsoleApp11;

internal class Program
{
    private static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        string dataDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data");

        Directory.CreateDirectory(dataDirectory);

        string filePath = Path.Combine(
            dataDirectory,
            "sample.txt");

        File.WriteAllText(
            filePath,
            "Hello from C#!\nResource management example.");

        Console.WriteLine("=== RESOURCE MANAGEMENT LAB ===");
        Console.WriteLine();

        Console.WriteLine("1. Чтение без using");
        Console.WriteLine(FileReaderWithoutUsing.Read(filePath));

        Console.WriteLine();
        Console.WriteLine("2. Чтение с using");
        Console.WriteLine(FileReaderWithUsing.Read(filePath));

        Console.WriteLine();
        Console.WriteLine("3. Исключение внутри using");

        try
        {
            FileReaderWithUsing.ReadFileWithError(filePath);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Перехвачена ошибка: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("4. Сравнение using и отсутствия using");

        // В первом варианте файл закрывается только когда GC когда-нибудь доберётся
        // до объекта и вызовет финализатор, а когда именно - неизвестно.
        // Во втором using сам вызывает Dispose() при выходе из метода, даже если было исключение,
        // так что момент закрытия известен и от GC не зависит.
        // Повторное открытие файла тут ничего не доказывает, потому что блокировки зависят от ОС,
        // главное доказательство это явный вызов Dispose().

        Console.WriteLine();
        Console.WriteLine("5. Поколения GC");
        GcGenerationsDemo.Run();

        Console.WriteLine();
        Console.WriteLine("Работа программы завершена.");
    }
}
