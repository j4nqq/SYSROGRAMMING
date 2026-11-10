using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class PipelineProcessor
{
    public static async Task<AnalysisReport> ProcessAsync(
        IEnumerable<FileTask> tasks,
        int workerCount)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        if (workerCount < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(workerCount));
        }

        var taskList = tasks.ToList();

        var results = new ConcurrentBag<FileAnalysisResult>();

        var errors = new ConcurrentBag<FileProcessingError>();

        var stopwatch = Stopwatch.StartNew();

        var options = new BoundedChannelOptions(workerCount * 2)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = false
        };

        var channel = Channel.CreateBounded<FileTask>(options);

        Task producer = ProduceAsync(taskList, channel.Writer);

        Task[] workers = Enumerable
            .Range(1, workerCount)
            .Select(id => WorkerAsync(
                id,
                channel.Reader,
                results,
                errors))
            .ToArray();

        await Task.WhenAll(workers.Append(producer));

        stopwatch.Stop();

        return ReportService.Create(
            $"Pipeline ({workerCount} workers)",
            taskList.Count,
            results.ToList(),
            errors.ToList(),
            workerCount,
            stopwatch.ElapsedMilliseconds);
    }

    private static async Task ProduceAsync(
        List<FileTask> tasks,
        ChannelWriter<FileTask> writer)
    {
        try
        {
            foreach (var task in tasks)
            {
                await writer.WriteAsync(task);
            }
        }
        finally
        {
            writer.TryComplete();
        }
    }

    private static async Task WorkerAsync(
        int workerId,
        ChannelReader<FileTask> reader,
        ConcurrentBag<FileAnalysisResult> results,
        ConcurrentBag<FileProcessingError> errors)
    {
        await foreach (var task in reader.ReadAllAsync())
        {
            try
            {
                Console.WriteLine(
                    $"[WORKER {workerId}] Начал: {task.FilePath}");

                var result = await FileAnalyzer.AnalyzeAsync(
                    task,
                    workerId);

                results.Add(result);

                Console.WriteLine(
                    $"[WORKER {workerId}] Завершил: {result.FileName}");
            }
            catch (IOException ex)
            {
                errors.Add(new FileProcessingError(
                    task.Id,
                    Path.GetFileName(task.FilePath),
                    nameof(IOException),
                    ex.Message));
            }
            catch (UnauthorizedAccessException ex)
            {
                errors.Add(new FileProcessingError(
                    task.Id,
                    Path.GetFileName(task.FilePath),
                    nameof(UnauthorizedAccessException),
                    ex.Message));
            }
        }
    }
}
