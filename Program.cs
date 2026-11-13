using ConsoleApp2.Models;
using ConsoleApp2.Services;
using System.Collections.Concurrent;

namespace ConsoleApp2;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Зачётный проект 03: Мониторинг системных журналов");
        Console.WriteLine(new string('=', 64));

        var service = new ProjectService();
        await service.RunAsync();

        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для завершения.");
        Console.ReadLine();
    }
}

public sealed class ProjectService
{
    public async Task RunAsync()
    {
        // TODO: перенесите предметную логику в отдельные методы и классы.
        // TODO: не размещайте весь проект в Main.
        // TODO 1: Читать журнал и выделять INFO, WARNING и ERROR.

        string dataDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data");
        Directory.CreateDirectory(dataDirectory);

        string logFilePath = Path.Combine(
            dataDirectory,
            "system.log");

        if (!File.Exists(logFilePath))
        {
            Console.WriteLine($"Файл журнала не найден: {logFilePath}");
            Console.WriteLine("Создайте файл с записями в формате: [дата] [УРОВЕНЬ] сообщение");
            await Task.CompletedTask;
            return;
        }

        Console.WriteLine("TODO 1");
        Console.WriteLine();
        Console.WriteLine($"Файл журнала: {logFilePath}");
        Console.WriteLine();

        int infoCount = 0;
        int warningCount = 0;
        int errorCount = 0;
        int unknownCount = 0;
        int skippedCount = 0;

        foreach (string line in File.ReadLines(logFilePath))
        {
            LogEntry? entry = LogParser.TryParse(line);

            if (entry is null)
            {
                skippedCount++;
                continue;
            }

            switch (entry.Level)
            {
                case LogLevel.Info:
                    infoCount++;
                    break;
                case LogLevel.Warning:
                    warningCount++;
                    break;
                case LogLevel.Error:
                    errorCount++;
                    break;
                case LogLevel.Unknown:
                    unknownCount++;
                    break;
            }

            Console.WriteLine(
                $"[{entry.Timestamp:HH:mm:ss}] " +
                $"[{entry.Level,-7}] " +
                $"{entry.Message}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Итоги ---");
        Console.WriteLine($"INFO:    {infoCount}");
        Console.WriteLine($"WARNING: {warningCount}");
        Console.WriteLine($"ERROR:   {errorCount}");
        Console.WriteLine($"UNKNOWN: {unknownCount}");
        Console.WriteLine($"Пропущено строк: {skippedCount}");
        Console.WriteLine();

        // TODO 2: Подписаться на изменения файла через FileSystemWatcher.

        //Console.WriteLine("TODO 2");
        //Console.WriteLine();

        //using var watcher = new LogFileWatcher(logFilePath);
        //watcher.FileChanged += (_, path) =>
        //    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Файл изменён: {path}");
        //watcher.WatcherError += (_, ex) =>
        //    Console.WriteLine($"Ошибка наблюдателя: {ex.Message}");
        //watcher.Start();

        //Console.WriteLine("Наблюдение запущено. Допишите строку в журнал и сохраните файл.");
        //Console.WriteLine("Нажмите Enter для остановки наблюдения.");
        //Console.ReadLine();


        // TODO 3: Безопасно передавать события наблюдателя в обработчик.


        Console.WriteLine("TODO 3");
        Console.WriteLine();

        var tailReader = new LogTailReader(logFilePath, new FileInfo(logFilePath).Length);
        using var changes = new BlockingCollection<string>();

        using var watcher = new LogFileWatcher(logFilePath);
        watcher.FileChanged += (_, path) => changes.Add(path);
        watcher.WatcherError += (_, ex) =>
            Console.WriteLine($"Ошибка наблюдателя: {ex.Message}");
        watcher.Start();

        Task handler = Task.Run(() => HandleChanges(changes, tailReader));

        Console.WriteLine("Наблюдение запущено.");
        Console.WriteLine("Нажмите Enter для остановки наблюдения.");
        Console.ReadLine();


        // TODO 4: Считать количество сообщений каждого уровня потокобезопасно.
        // TODO 5: Обрабатывать новые записи через фиксированных worker-ов.
        // TODO 6: Корректно завершать наблюдатель и освобождать ресурсы.
        // TODO 7: Вынести обработку записей в отдельную DLL.
        // TODO 8: Добавить bounded Channel между наблюдателем и обработчиками.
        // TODO 9: Сохранять счётчики в реестре Windows.
        // TODO 10: Экспортировать статистику в JSON.

        Console.WriteLine(" ");
        await Task.CompletedTask;
    }

    private static void HandleChanges(BlockingCollection<string> changes, LogTailReader tailReader)
    {
        foreach (string _ in changes.GetConsumingEnumerable())
        {
            try
            {
                foreach (string line in tailReader.ReadNewLines())
                {
                    LogEntry? entry = LogParser.TryParse(line);

                    if (entry is null)
                    {
                        continue;
                    }

                    Console.WriteLine(
                        $"[{entry.Timestamp:HH:mm:ss}] " +
                        $"[{entry.Level,-7}] " +
                        $"{entry.Message}");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка чтения журнала: {ex.Message}");
            }
        }
    }
}