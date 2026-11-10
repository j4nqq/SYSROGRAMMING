using FileProcessingPipeline.Models;

namespace FileProcessingPipeline.Services;

public static class ErrorScenario
{
    public static List<FileTask> CreateTasks(string dataDirectory)
    {
        return new List<FileTask>
        {
            new(1, Path.Combine(dataDirectory, "file1.txt")),
            new(2, Path.Combine(dataDirectory, "file2.txt")),
            new(3, Path.Combine(dataDirectory, "missing.txt")),
            new(4, Path.Combine(dataDirectory, "file3.txt")),
            new(5, Path.Combine(dataDirectory, "file4.txt"))
        };
    }

    public static bool Verify(AnalysisReport report)
    {
        return report.TotalFiles == 5
            && report.SuccessfulFiles == 4
            && report.FailedFiles == 1
            && report.Errors.Count == 1
            && report.Errors[0].FileName == "missing.txt";
    }
}
