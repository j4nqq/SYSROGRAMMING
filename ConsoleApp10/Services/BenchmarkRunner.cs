using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class BenchmarkRunner
{
    public static async Task RunAsync(
        IReadOnlyList<FileTask> baseTasks,
        int repeat)
    {
        var tasks = Enumerable
            .Range(0, repeat)
            .SelectMany(_ => baseTasks)
            .Select((task, index) => new FileTask(index + 1, task.FilePath))
            .ToList();

        Console.WriteLine();
        Console.WriteLine($"Заданий в тесте: {tasks.Count}");
        Console.WriteLine("Прогрев...");

        await SequentialProcessor.ProcessAsync(baseTasks, false);
        await PipelineProcessor.ProcessAsync(baseTasks, 2, false);

        var reports = new List<AnalysisReport>
        {
            await SequentialProcessor.ProcessAsync(tasks, false)
        };

        foreach (int workerCount in new[] { 1, 2, 4, 8 })
        {
            Console.WriteLine($"Запуск: {workerCount} обработчиков...");

            reports.Add(await PipelineProcessor.ProcessAsync(
                tasks,
                workerCount,
                false));
        }

        var lines = new List<string>
        {
            "| Режим | Обработчики | Время, мс | Обработано | Ошибок | Распределение заданий |",
            "|---|---|---|---|---|---|"
        };

        foreach (var report in reports)
        {
            string distribution = string.Join(
                ", ",
                report.WorkerStatistics.Select(
                    s => $"W{s.WorkerId}={s.Count}"));

            string mode = report.Mode.StartsWith("Sequential")
                ? "Sequential"
                : "Parallel";

            lines.Add(
                $"| {mode} | {report.WorkerCount} | {report.ProcessingTimeMs} | " +
                $"{report.SuccessfulFiles} | {report.FailedFiles} | {distribution} |");
        }

        Console.WriteLine();
        Console.WriteLine("========== СРАВНЕНИЕ ПРОИЗВОДИТЕЛЬНОСТИ ==========");

        foreach (string line in lines)
        {
            Console.WriteLine(line);
        }

        var first = reports[0];

        bool consistent = reports.All(r =>
            r.SuccessfulFiles == first.SuccessfulFiles &&
            r.FailedFiles == first.FailedFiles &&
            r.TotalLines == first.TotalLines &&
            r.TotalWords == first.TotalWords &&
            r.TotalCharacters == first.TotalCharacters);

        Console.WriteLine();
        Console.WriteLine(
            "Проверка корректности (итоги всех режимов совпадают): " +
            (consistent ? "OK" : "РАСХОЖДЕНИЕ"));

        string path = Path.Combine(
            AppContext.BaseDirectory,
            "benchmark.md");

        await File.WriteAllLinesAsync(path, lines);

        Console.WriteLine($"Таблица сохранена: {path}");
    }
}
