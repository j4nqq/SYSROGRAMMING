using ConsoleApp11.Services;

namespace ConsoleApp11;

internal class Program
{
    private static async Task Main()
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

        string testPath = Path.Combine(
            dataDirectory,
            "test.txt");

        File.Delete(testPath);

        Console.WriteLine();
        Console.WriteLine("6. FileStorage: запись и чтение");

        using (var storage = new FileStorage(testPath))
        {
            storage.Write("Hello");
            Console.WriteLine(storage.Read());
        }

        Console.WriteLine();
        Console.WriteLine("7. Использование после Dispose");

        var disposedStorage = new FileStorage(testPath);
        disposedStorage.Dispose();

        try
        {
            disposedStorage.Read();
        }
        catch (ObjectDisposedException ex)
        {
            Console.WriteLine($"Read: {ex.GetType().Name}");
        }

        try
        {
            disposedStorage.Write("text");
        }
        catch (ObjectDisposedException ex)
        {
            Console.WriteLine($"Write: {ex.GetType().Name}");
        }

        Console.WriteLine();
        Console.WriteLine("8. Повторный Dispose");

        var storage3 = new FileStorage(testPath);

        storage3.Write("Hello");

        storage3.Dispose();
        storage3.Dispose();
        storage3.Dispose();

        Console.WriteLine("Исключений нет.");

        Console.WriteLine();
        Console.WriteLine("9. Владение ресурсом и leaveOpen");

        using (var stream = new FileStream(
            testPath,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite))
        {
            Console.WriteLine(StreamConsumer.ReadAll(stream));
            Console.WriteLine($"Поток всё ещё открыт: {stream.CanRead}");
        }

        Console.WriteLine();
        Console.WriteLine("10. IAsyncDisposable");

        string asyncPath = Path.Combine(
            dataDirectory,
            "async.txt");

        File.Delete(asyncPath);

        await using (var asyncStorage = new AsyncFileStorage(asyncPath))
        {
            await asyncStorage.WriteAsync("Асинхронная запись.");
            Console.WriteLine(await asyncStorage.ReadAsync());
        }

        Console.WriteLine();
        Console.WriteLine("Работа программы завершена.");
    }
}
