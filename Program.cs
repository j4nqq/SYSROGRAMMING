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
    private const int WorkerCount = 3;

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


        //Console.WriteLine("TODO 3");
        //Console.WriteLine();

        //var tailReader = new LogTailReader(logFilePath, new FileInfo(logFilePath).Length);
        //using var changes = new BlockingCollection<string>();

        //using var watcher = new LogFileWatcher(logFilePath);
        //watcher.FileChanged += (_, path) => changes.Add(path);
        //watcher.WatcherError += (_, ex) =>
        //    Console.WriteLine($"Ошибка наблюдателя: {ex.Message}");
        //watcher.Start();

        //Task handler = Task.Run(() => HandleChanges(changes, tailReader));

        //Console.WriteLine("Наблюдение запущено.");
        //Console.WriteLine("Нажмите Enter для остановки наблюдения.");
        //Console.ReadLine();


        // TODO 4: Считать количество сообщений каждого уровня потокобезопасно.

        //Console.WriteLine("TODO 4");
        //Console.WriteLine();

        //var statistics = new LogStatistics();
        //statistics.Add(LogLevel.Info, infoCount);
        //statistics.Add(LogLevel.Warning, warningCount);
        //statistics.Add(LogLevel.Error, errorCount);
        //statistics.Add(LogLevel.Unknown, unknownCount);
        //statistics.AddSkipped(skippedCount);

        //var tailReader = new LogTailReader(logFilePath, new FileInfo(logFilePath).Length);
        //using var changes = new BlockingCollection<string>();

        //using var watcher = new LogFileWatcher(logFilePath);
        //watcher.FileChanged += (_, path) => changes.Add(path);
        //watcher.WatcherError += (_, ex) =>
        //    Console.WriteLine($"Ошибка наблюдателя: {ex.Message}");
        //watcher.Start();

        //Task handler = Task.Run(() => HandleChanges(changes, tailReader, statistics));

        //Console.WriteLine("Наблюдение запущено. Допишите строку в журнал и сохраните файл.");
        //Console.WriteLine("Нажмите Enter для остановки наблюдения.");
        //Console.ReadLine();

        //PrintStatistics(statistics);

        // TODO 5: Обрабатывать новые записи через фиксированных worker-ов.

        //Console.WriteLine("TODO 5");
        //Console.WriteLine();

        //var statistics = new LogStatistics();
        //statistics.Add(LogLevel.Info, infoCount);
        //statistics.Add(LogLevel.Warning, warningCount);
        //statistics.Add(LogLevel.Error, errorCount);
        //statistics.Add(LogLevel.Unknown, unknownCount);
        //statistics.AddSkipped(skippedCount);

        //var tailReader = new LogTailReader(logFilePath, new FileInfo(logFilePath).Length);
        //using var changes = new BlockingCollection<string>();
        //using var entries = new BlockingCollection<LogEntry>();

        //Task[] workers = new Task[WorkerCount];
        //for (int i = 0; i < WorkerCount; i++)
        //{
        //    int workerId = i + 1;
        //    workers[i] = Task.Run(() => ProcessEntries(workerId, entries, statistics));
        //}

        //using var watcher = new LogFileWatcher(logFilePath);
        //watcher.FileChanged += (_, path) => changes.Add(path);
        //watcher.WatcherError += (_, ex) =>
        //    Console.WriteLine($"Ошибка наблюдателя: {ex.Message}");
        //watcher.Start();

        //Task handler = Task.Run(() => HandleChanges(changes, tailReader, entries, statistics));

        //Console.WriteLine("Наблюдение запущено.");
        //Console.WriteLine("Нажмите Enter для остановки наблюдения.");
        //Console.ReadLine();

        //PrintStatistics(statistics);

        // TODO 6: Корректно завершать наблюдатель и освобождать ресурсы.

        Console.WriteLine("TODO 6");
        Console.WriteLine();

        var statistics = new LogStatistics();
        statistics.Add(LogLevel.Info, infoCount);
        statistics.Add(LogLevel.Warning, warningCount);
        statistics.Add(LogLevel.Error, errorCount);
        statistics.Add(LogLevel.Unknown, unknownCount);
        statistics.AddSkipped(skippedCount);

        var tailReader = new LogTailReader(logFilePath, new FileInfo(logFilePath).Length);
        using var changes = new BlockingCollection<string>();
        using var entries = new BlockingCollection<LogEntry>();

        Task[] workers = new Task[WorkerCount];
        for (int i = 0; i < WorkerCount; i++)
        {
            int workerId = i + 1;
            workers[i] = Task.Run(() => ProcessEntries(workerId, entries, statistics));
        }

        using var watcher = new LogFileWatcher(logFilePath);
        watcher.FileChanged += (_, path) =>
        {
            try
            {
                changes.Add(path);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Наблюдатель остановлен, событие пропущено.");
            }
        };
        watcher.WatcherError += (_, ex) =>
            Console.WriteLine($"Ошибка наблюдателя: {ex.Message}");
        watcher.Start();

        Task handler = Task.Run(() => HandleChanges(changes, tailReader, entries, statistics));

        try
        {
            Console.WriteLine("Наблюдение запущено.");
            Console.WriteLine("Нажмите Enter для остановки наблюдения.");
            Console.ReadLine();
        }
        finally
        {
            watcher.Stop();

            changes.CompleteAdding();
            await handler;

            entries.CompleteAdding();
            await Task.WhenAll(workers);
        }

        PrintStatistics(statistics);

        // TODO 7: Вынести обработку записей в отдельную DLL.
        // TODO 8: Добавить bounded Channel между наблюдателем и обработчиками.
        // TODO 9: Сохранять счётчики в реестре Windows.
        // TODO 10: Экспортировать статистику в JSON.

        Console.WriteLine(" ");
        await Task.CompletedTask;
    }

    private static void HandleChanges(
       BlockingCollection<string> changes,
       LogTailReader tailReader,
       BlockingCollection<LogEntry> entries,
       LogStatistics statistics)
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
                        statistics.IncrementSkipped();
                        continue;
                    }

                    entries.Add(entry);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка чтения журнала: {ex.Message}");
            }
        }
    }

    private static void ProcessEntries(
        int workerId,
        BlockingCollection<LogEntry> entries,
        LogStatistics statistics)
    {
        foreach (LogEntry entry in entries.GetConsumingEnumerable())
        {
            statistics.Increment(entry.Level);

            Console.WriteLine(
                $"[Worker {workerId}] " +
                $"[{entry.Timestamp:HH:mm:ss}] " +
                $"[{entry.Level,-7}] " +
                $"{entry.Message}");
        }
    }

    private static void PrintStatistics(LogStatistics statistics)
    {
        Console.WriteLine();
        Console.WriteLine("--- Итоги наблюдения ---");
        Console.WriteLine($"INFO:    {statistics.Get(LogLevel.Info)}");
        Console.WriteLine($"WARNING: {statistics.Get(LogLevel.Warning)}");
        Console.WriteLine($"ERROR:   {statistics.Get(LogLevel.Error)}");
        Console.WriteLine($"UNKNOWN: {statistics.Get(LogLevel.Unknown)}");
        Console.WriteLine($"Пропущено строк: {statistics.Skipped}");
        Console.WriteLine();
    }
}