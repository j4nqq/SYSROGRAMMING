namespace AsyncFileScanner.Models;

// Модель содержит настройки сканирования.
public sealed record ScanOptions(
    string RootPath,
    IReadOnlySet<string> Extensions);
