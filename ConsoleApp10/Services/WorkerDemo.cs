using System.Collections.Concurrent;
using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class WorkerDemo
{
    public static async Task RunAsync(IReadOnlyList<FileTask> tasks)
    {
        var task = tasks[0];

        var results = new ConcurrentBag<FileAnalysisResult>();

        Task[] workers = Enumerable
            .Range(1, 3)
            .Select(id => WorkerAsync(id, task, results))
            .ToArray();

        await Task.WhenAll(workers);

        Console.WriteLine();
        Console.WriteLine("--- Проверка сбора результатов ---");
        Console.WriteLine(
            $"Результатов собрано: {results.Count} (ожидалось 3) -> " +
            (results.Count == 3 ? "ничего не потеряно" : "ПОТЕРЯНЫ"));

        var sorted = results
            .OrderBy(r => r.WorkerId)
            .ToList();

        foreach (var r in sorted)
        {
            Console.WriteLine(
                $"Worker {r.WorkerId} | {r.FileName} | " +
                $"Строк: {r.Lines} | Слов: {r.Words} | " +
                $"Символов: {r.Characters}");
        }

        bool eachWorkerWrote = sorted
            .Select(r => r.WorkerId)
            .Distinct()
            .Count() == 3;

        Console.WriteLine(
            $"Каждый обработчик записал свой результат: {eachWorkerWrote}");
    }

    private static async Task WorkerAsync(
        int workerId,
        FileTask task,
        ConcurrentBag<FileAnalysisResult> results)
    {
        Console.WriteLine(
            $"[WORKER {workerId}] Начал: {Path.GetFileName(task.FilePath)}");

        var result = await FileAnalyzer.AnalyzeAsync(task, workerId);

        results.Add(result);

        Console.WriteLine(
            $"[WORKER {workerId}] Завершил: {result.FileName} | " +
            $"Строк: {result.Lines} | Слов: {result.Words} | " +
            $"Символов: {result.Characters}");
    }
}
