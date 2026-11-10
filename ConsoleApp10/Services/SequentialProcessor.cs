using System.Diagnostics;
using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class SequentialProcessor
{
    public static async Task<AnalysisReport> ProcessAsync(
        IEnumerable<FileTask> tasks,
        bool verbose = true)
    {
        var stopwatch = Stopwatch.StartNew();

        var results = new List<FileAnalysisResult>();
        var errors = new List<FileProcessingError>();

        var taskList = tasks.ToList();

        foreach (var task in taskList)
        {
            try
            {
                var result = await FileAnalyzer.AnalyzeAsync(
                    task,
                    1);

                results.Add(result);

                if (verbose)
                {
                    Console.WriteLine(
                        $"[OK] {result.FileName}");
                }
            }
            catch (Exception ex)
            {
                errors.Add(new FileProcessingError(
                    task.Id,
                    Path.GetFileName(task.FilePath),
                    ex.GetType().Name,
                    ex.Message));

                if (verbose)
                {
                    Console.WriteLine(
                        $"[ERR] {Path.GetFileName(task.FilePath)}: {ex.GetType().Name}");
                }
            }
        }

        stopwatch.Stop();

        return ReportService.Create(
            "Sequential",
            taskList.Count,
            results,
            errors,
            1,
            stopwatch.ElapsedMilliseconds);
    }
}
