namespace AsyncFileScanner.Models;

// Модель результата обработки одного файла.
public sealed record ScanResult(
    string FileName,
    long SizeBytes,
    int BytesRead);
