namespace AsyncFileScanner.Models;

public sealed record ScanOptions(
    string RootPath,
    IReadOnlySet<string> Extensions,
    long MinBytes,
    long MaxBytes,
    int MaxConcurrency);
