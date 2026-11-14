using System.Text.Json;
using ConsoleApp2.Models;

namespace ConsoleApp2.Services;

public static class StatisticsExporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task ExportAsync(LogStatistics statistics, string filePath)
    {
        int info = statistics.Get(LogLevel.Info);
        int warning = statistics.Get(LogLevel.Warning);
        int error = statistics.Get(LogLevel.Error);
        int unknown = statistics.Get(LogLevel.Unknown);

        var report = new LogStatisticsReport(
            DateTime.Now,
            info,
            warning,
            error,
            unknown,
            statistics.Skipped,
            info + warning + error + unknown);

        string json = JsonSerializer.Serialize(report, Options);
        await File.WriteAllTextAsync(filePath, json);
    }
}