using System.Text.Json;
using FileProcessingPipeline.Models;
using FileProcessingPipeline.Services;

namespace FileProcessingPipeline;

internal class Program
{
    private static async Task Main()
    {
        Console.WriteLine("================================");
        Console.WriteLine("     FILE PROCESSING PIPELINE");
        Console.WriteLine("================================");

        string dataDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "Data");

        Directory.CreateDirectory(dataDirectory);

        string[] paths = Directory.GetFiles(
            dataDirectory,
            "*.txt");

        if (paths.Length == 0)
        {
            Console.WriteLine("Нет файлов для обработки.");
            return;
        }

        var tasks = paths
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .Select((path, index) =>
                new FileTask(index + 1, path))
            .ToList();

        Console.WriteLine(
            $"Найдено файлов: {tasks.Count}");

        Console.WriteLine();
        Console.WriteLine("1. Последовательная обработка");
        Console.WriteLine("2. Параллельная обработка");
        Console.WriteLine("3. Демо П1 (задания 3-5: несколько обработчиков, один файл)");
        Console.WriteLine("4. П2: сравнение производительности (1, 2, 4, 8 обработчиков)");
        Console.WriteLine("5. П2: сценарий с ошибочным файлом (missing.txt)");

        Console.Write("Выберите режим: ");

        string? choice = Console.ReadLine();

        AnalysisReport report;

        switch (choice)
        {
            case "1":

                report = await SequentialProcessor.ProcessAsync(
                    tasks);

                break;

            case "2":

                Console.Write("Количество обработчиков: ");

                if (!int.TryParse(
                        Console.ReadLine(),
                        out int workerCount) ||
                    workerCount < 1)
                {
                    Console.WriteLine("Некорректное значение.");
                    return;
                }

                report = await PipelineProcessor.ProcessAsync(
                    tasks,
                    workerCount);

                break;

            case "3":

                await WorkerDemo.RunAsync(tasks);
                return;

            case "4":

                Console.Write("Сколько раз повторить набор файлов (Enter = 50): ");

                string? repeatInput = Console.ReadLine();

                int repeat = 50;

                if (!string.IsNullOrWhiteSpace(repeatInput) &&
                    (!int.TryParse(repeatInput, out repeat) || repeat < 1))
                {
                    Console.WriteLine("Некорректное значение.");
                    return;
                }

                await BenchmarkRunner.RunAsync(tasks, repeat);
                return;

            case "5":

                var errorTasks = ErrorScenario.CreateTasks(dataDirectory);

                report = await PipelineProcessor.ProcessAsync(
                    errorTasks,
                    3);

                PrintReport(report);

                Console.WriteLine();
                Console.WriteLine(
                    "Проверка (4 успешно, 1 ошибка, программа не упала): " +
                    (ErrorScenario.Verify(report) ? "OK" : "НЕ ПРОЙДЕНА"));

                await SaveReportAsync(report);
                return;

            default:

                Console.WriteLine("Неизвестный режим.");
                return;
        }

        PrintReport(report);

        await SaveReportAsync(report);
    }

    private static void PrintReport(AnalysisReport report)
    {
        Console.WriteLine();
        Console.WriteLine("========== ОТЧЁТ ==========");
        Console.WriteLine($"Режим: {report.Mode}");
        Console.WriteLine($"Всего файлов: {report.TotalFiles}");
        Console.WriteLine($"Успешно: {report.SuccessfulFiles}");
        Console.WriteLine($"Ошибок: {report.FailedFiles}");
        Console.WriteLine($"Обработчиков: {report.WorkerCount}");
        Console.WriteLine($"Строк: {report.TotalLines}");
        Console.WriteLine($"Слов: {report.TotalWords}");
        Console.WriteLine($"Символов: {report.TotalCharacters}");
        Console.WriteLine($"Время: {report.ProcessingTimeMs} мс");
        Console.WriteLine();
        Console.WriteLine("РЕЗУЛЬТАТЫ:");

        foreach (var result in report.Results)
        {
            Console.WriteLine(
                $"{result.FileName} | " +
                $"Worker: {result.WorkerId} | " +
                $"Строк: {result.Lines} | " +
                $"Слов: {result.Words}");
        }

        if (report.Errors.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("ОШИБКИ:");

            foreach (var error in report.Errors)
            {
                Console.WriteLine(
                    $"{error.FileName}: {error.ErrorType} - {error.Message}");
            }
        }

        PrintWorkerStatistics(report);
    }

    private static void PrintWorkerStatistics(AnalysisReport report)
    {
        Console.WriteLine();
        Console.WriteLine("========== СТАТИСТИКА WORKERS ==========");
        Console.WriteLine();

        foreach (var stat in report.WorkerStatistics)
        {
            Console.WriteLine(
                $"Worker {stat.WorkerId}: {stat.Count} заданий");
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Всего обработано: {report.WorkerStatistics.Sum(x => x.Count)}");
        Console.WriteLine();
        Console.WriteLine("=======================================");
    }

    private static async Task SaveReportAsync(
        AnalysisReport report)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        string json = JsonSerializer.Serialize(
            report,
            options);
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "report.json");

        await File.WriteAllTextAsync(path, json);

        Console.WriteLine();
        Console.WriteLine($"Отчёт сохранён: {path}");
    }
}
