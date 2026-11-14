namespace ConsoleApp2.Models;

public sealed record LogStatisticsReport(
    DateTime ExportedAt,
    int Info,
    int Warning,
    int Error,
    int Unknown,
    int Skipped,
    int Total);