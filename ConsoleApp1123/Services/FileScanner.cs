using System.Collections.Concurrent;
using System.Diagnostics;
using AsyncFileScanner.Models;

namespace AsyncFileScanner.Services;

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
        var errors = new ConcurrentQueue<string>();
        var tasks = new List<Task<ScanResult?>>();
        var tracker = new ConcurrencyTracker();
        using var semaphore = new SemaphoreSlim(options.MaxConcurrency);

        int scannedFiles = 0;

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

                long length;

                try
                {
                    length = new FileInfo(path).Length;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    errors.Enqueue($"Не удалось получить размер {path}: {ex.Message}");
                    continue;
                }

                if (length < options.MinBytes || length > options.MaxBytes)
                    continue;

                tasks.Add(ProcessFileLimitedAsync(
                    path,
                    length,
                    semaphore,
                    tracker,
                    errors,
                    onFileFound,
                    cancellationToken));
            }
        }
        catch (OperationCanceledException)
        {
            await DrainAsync(tasks);
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            errors.Enqueue($"Ошибка перечисления файлов: {ex.Message}");
        }

        ScanResult?[] processed = await Task.WhenAll(tasks);

        stopwatch.Stop();

        List<ScanResult> results = processed
            .OfType<ScanResult>()
            .ToList();

        return new ScanSummary(
            scannedFiles,
            results.Count,
            results.Sum(x => x.SizeBytes),
            stopwatch.Elapsed,
            results,
            errors.ToList(),
            options.MaxConcurrency,
            tracker.MaxObserved);
    }

    private async Task<ScanResult?> ProcessFileLimitedAsync(
        string path,
        long length,
        SemaphoreSlim semaphore,
        ConcurrencyTracker tracker,
        ConcurrentQueue<string> errors,
        Action<ScanResult>? onFileFound,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            tracker.Enter();

            try
            {
                return await ProcessFileAsync(
                    path,
                    length,
                    errors,
                    onFileFound,
                    cancellationToken);
            }
            finally
            {
                tracker.Exit();
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task<ScanResult?> ProcessFileAsync(
        string path,
        long length,
        ConcurrentQueue<string> errors,
        Action<ScanResult>? onFileFound,
        CancellationToken cancellationToken)
    {
        try
        {
            int bytesRead = await _probe.ReadHeaderAsync(
                path,
                cancellationToken);

            var result = new ScanResult(
                Path.GetFileName(path),
                length,
                bytesRead);

            onFileFound?.Invoke(result);

            return result;
        }
        catch (UnauthorizedAccessException ex)
        {
            errors.Enqueue($"Нет доступа к файлу {path}: {ex.Message}");
            return null;
        }
        catch (IOException ex)
        {
            errors.Enqueue($"Ошибка ввода-вывода {path}: {ex.Message}");
            return null;
        }
    }

    private static async Task DrainAsync(IEnumerable<Task> tasks)
    {
        try
        {
            await Task.WhenAll(tasks);
        }
        catch
        {
        }
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

        if (options.MinBytes < 0)
            throw new ArgumentException(
                "Минимальный размер не может быть отрицательным.",
                nameof(options));

        if (options.MaxBytes < options.MinBytes)
            throw new ArgumentException(
                "Максимальный размер меньше минимального.",
                nameof(options));

        if (options.MaxConcurrency < 1)
            throw new ArgumentException(
                "Количество одновременных операций должно быть не меньше 1.",
                nameof(options));
    }

    private sealed class ConcurrencyTracker
    {
        private int _active;
        private int _maxObserved;

        public int MaxObserved => Volatile.Read(ref _maxObserved);

        public void Enter()
        {
            int current = Interlocked.Increment(ref _active);
            int observed = Volatile.Read(ref _maxObserved);

            while (current > observed)
            {
                int previous = Interlocked.CompareExchange(
                    ref _maxObserved,
                    current,
                    observed);

                if (previous == observed)
                    break;

                observed = previous;
            }
        }

        public void Exit()
        {
            Interlocked.Decrement(ref _active);
        }
    }
}
