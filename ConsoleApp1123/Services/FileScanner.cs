using System.Diagnostics;
using AsyncFileScanner.Models;

namespace AsyncFileScanner.Services;

/**
 * В этом примере есть несколько важных особенностей.
 * 1. перечисление файлов выполняется через `Directory.EnumerateFiles`. Это синхронный API. Он не становится асинхронным оттого, что находится внутри `async` метода.
 * 2. асинхронная операция здесь — чтение файла через `ReadHeaderAsync`.
 * 3. ошибки одного файла сохраняются в список. Они не приводят к остановке всего сканирования.
 * 4. отмена проверяется перед обработкой каждого следующего файла, а также передаётся непосредственно в `ReadAsync`.
 * Важно: отмена не может гарантированно мгновенно прервать системный вызов перечисления файлов. В данном варианте она проверяется между итерациями.
 */
public sealed class FileScanner
{
    private readonly FileProbe _probe;

    public FileScanner(FileProbe probe)
    {
        _probe = probe;
    }

    public async Task<ScanSummary> ScanAsync(
        ScanOptions options,
        CancellationToken cancellationToken,
        Action<ScanResult>? onFileFound = null)
    {
        ValidateOptions(options);

        var stopwatch = Stopwatch.StartNew();
        var results = new List<ScanResult>();
        var errors = new List<string>();

        int scannedFiles = 0;
        long totalBytes = 0;

        await Task.Yield();

        var enumerationOptions = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true
        };

        try
        {
            foreach (string path in Directory.EnumerateFiles(
                         options.RootPath,
                         "*",
                         enumerationOptions))
            {
                cancellationToken.ThrowIfCancellationRequested();

                scannedFiles++;

                if (!options.Extensions.Contains(Path.GetExtension(path)))
                    continue;

                try
                {
                    var info = new FileInfo(path);

                    int bytesRead = await _probe.ReadHeaderAsync(
                        path,
                        cancellationToken);

                    var result = new ScanResult(
                        info.Name,
                        info.Length,
                        bytesRead);

                    results.Add(result);

                    onFileFound?.Invoke(result);

                    totalBytes += info.Length;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    errors.Add($"Нет доступа к файлу {path}: {ex.Message}");
                }
                catch (IOException ex)
                {
                    errors.Add($"Ошибка ввода-вывода {path}: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            errors.Add($"Ошибка перечисления файлов: {ex.Message}");
        }

        stopwatch.Stop();

        return new ScanSummary(
            scannedFiles,
            results.Count,
            totalBytes,
            stopwatch.Elapsed,
            results,
            errors);
    }

    private static void ValidateOptions(ScanOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.RootPath))
            throw new ArgumentException(
                "Путь к директории не задан.",
                nameof(options));

        if (!Directory.Exists(options.RootPath))
            throw new DirectoryNotFoundException(
                $"Директория не найдена: {options.RootPath}");

        if (options.Extensions is null || options.Extensions.Count == 0)
            throw new ArgumentException(
                "Список расширений пуст.",
                nameof(options));
    }
}
