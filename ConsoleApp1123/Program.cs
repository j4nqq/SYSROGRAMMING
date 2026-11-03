using System.Globalization;
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

long minBytes = ReadKilobytes("Минимальный размер (КБ): ", 0);
long maxBytes = ReadKilobytes("Максимальный размер (КБ): ", long.MaxValue);

Console.Write("Введите максимальное количество одновременных операций: ");
if (!int.TryParse(Console.ReadLine(), out int maxConcurrency) || maxConcurrency < 1)
    maxConcurrency = 4;

var options = new ScanOptions(
    root,
    new HashSet<string>(
        extensions,
        StringComparer.OrdinalIgnoreCase),
    minBytes,
    maxBytes,
    maxConcurrency);

var scanner = new FileScanner(new FileProbe());

using var cts = new CancellationTokenSource();

Task<ScanSummary> scanTask = scanner.ScanAsync(
    options,
    cts.Token,
    result => Console.WriteLine(
        $"{result.FileName,-30} " +
        $"{result.SizeBytes,12} байт " +
        $"прочитано: {result.BytesRead}"));

Console.WriteLine();
Console.WriteLine("Сканирование запущено.");
Console.WriteLine("Нажмите Enter для отмены.");
Console.WriteLine();

Task cancelTask = Task.Run(Console.ReadLine);
await Task.WhenAny(scanTask, cancelTask);

if (!scanTask.IsCompleted)
    cts.Cancel();

try
{
    ScanSummary summary = await scanTask;

    double megabytes = summary.TotalMatchedBytes / 1024.0 / 1024.0;

    Console.WriteLine();
    Console.WriteLine("=== РЕЗУЛЬТАТ СКАНИРОВАНИЯ ===");
    Console.WriteLine();
    Console.WriteLine($"Просмотрено файлов: {summary.ScannedFiles}");
    Console.WriteLine($"Подходящих файлов: {summary.MatchedFiles}");
    Console.WriteLine($"Общий размер: {megabytes.ToString("F2", CultureInfo.InvariantCulture)} МБ");
    Console.WriteLine($"Ошибок: {summary.Errors.Count}");
    Console.WriteLine($"Время выполнения: {summary.Duration.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)} сек.");
    Console.WriteLine();
    Console.WriteLine($"Ограничение: {summary.MaxConcurrency}");
    Console.WriteLine($"Максимум одновременно обрабатываемых файлов: {summary.MaxObservedOperations}");

    foreach (string error in summary.Errors)
        Console.WriteLine($"Ошибка: {error}");
}
catch (OperationCanceledException)
{
    Console.WriteLine();
    Console.WriteLine("Сканирование отменено пользователем.");
}
catch (Exception ex) when (ex is ArgumentException or DirectoryNotFoundException)
{
    Console.WriteLine($"Ошибка: {ex.Message}");
}

static long ReadKilobytes(string prompt, long defaultBytes)
{
    Console.Write(prompt);

    if (long.TryParse(Console.ReadLine(), out long kb) && kb >= 0)
        return kb * 1024;

    return defaultBytes;
}
