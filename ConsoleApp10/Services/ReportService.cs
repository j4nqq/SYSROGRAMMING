using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class ReportService
{
    public static AnalysisReport Create(
        string mode,
        int totalFiles,
        List<FileAnalysisResult> results,
        List<FileProcessingError> errors,
        int workerCount,
        long processingTimeMs)
    {
        return new AnalysisReport
        {
            Mode = mode,
            TotalFiles = totalFiles,
            SuccessfulFiles = results.Count,
            FailedFiles = errors.Count,
            WorkerCount = workerCount,
            TotalLines = results.Sum(x => (long)x.Lines),
            TotalWords = results.Sum(x => (long)x.Words),
            TotalCharacters = results.Sum(x => x.Characters),
            ProcessingTimeMs = processingTimeMs,
            Results = results
                .OrderBy(x => x.FileName)
                .ThenBy(x => x.TaskId)
                .ToList(),
            Errors = errors
                .OrderBy(x => x.FileName)
                .ThenBy(x => x.TaskId)
                .ToList(),
            WorkerStatistics = Enumerable
                .Range(1, workerCount)
                .Select(id => new WorkerStatistic(
                    id,
                    results.Count(x => x.WorkerId == id)))
                .ToList()
        };
    }
}
