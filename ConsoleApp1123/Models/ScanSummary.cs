namespace AsyncFileScanner.Models;

public sealed record ScanSummary(
    int ScannedFiles,
    int MatchedFiles,
    long TotalMatchedBytes,
    TimeSpan Duration,
    IReadOnlyList<ScanResult> Results,
    IReadOnlyList<string> Errors,
    int MaxConcurrency,
    int MaxObservedOperations);
