using AsyncFileScanner.Models;
using AsyncFileScanner.Services;

Console.WriteLine("=== FILE SCANNER ===");

Console.Write("Введите путь к директории: ");
string root = Console.ReadLine() ?? "";

Console.Write("Введите расширения через запятую: ");
string[] extensions = (Console.ReadLine() ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
    .Select(x => x.StartsWith('.') ? x : "." + x)
    .ToArray();

var options = new ScanOptions(
    root,
    new HashSet<string>(
        extensions,
        StringComparer.OrdinalIgnoreCase));

var scanner = new FileScanner(new FileProbe());

using var cts = new CancellationTokenSource();

Task<ScanSummary> scanTask = scanner.ScanAsync(
    options,
    cts.Token);

Console.WriteLine();
Console.WriteLine("Сканирование запущено.");
Console.WriteLine("Нажмите Enter для отмены.");

Console.ReadLine();

cts.Cancel();

try
{
    ScanSummary summary = await scanTask;

    Console.WriteLine();
    Console.WriteLine("Сканирование завершено.");

    foreach (var result in summary.Results)
    {
        Console.WriteLine(
            $"{result.FileName,-30} " +
            $"{result.SizeBytes,12} байт " +
            $"прочитано: {result.BytesRead}");
    }

    Console.WriteLine();
    Console.WriteLine($"Обработано: {summary.ScannedFiles}");
    Console.WriteLine($"Совпадений: {summary.MatchedFiles}");
    Console.WriteLine($"Общий размер: {summary.TotalMatchedBytes}");
    Console.WriteLine($"Время: {summary.Duration}");

    foreach (string error in summary.Errors)
        Console.WriteLine($"Ошибка: {error}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("Сканирование отменено.");
}