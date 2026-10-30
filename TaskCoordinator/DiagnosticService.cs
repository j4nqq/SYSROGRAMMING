using System.Diagnostics;
namespace TaskCoordinator;
public sealed class DiagnosticService
{
    public async Task<DiagnosticResult> RunAsync(string name, TimeSpan duration, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя диагностики не может быть пустым.", nameof(name));
        if (duration < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Длительность не может быть отрицательной.");
        var sw = Stopwatch.StartNew();
        await Task.Delay(duration, token);
        sw.Stop();
        return new(name, DiagnosticStatus.Completed, "Диагностика завершена.", sw.Elapsed);
    }
    public async Task<DiagnosticResult> RunCpuBoundAsync(string name, int iterations, CancellationToken token = default)
    {
        long sum = 0;
        var cancelled = await Task.Run(() =>
        {
            for (int i = 0; i < iterations; i++)
            {
                if (i % 1000 == 0 && token.IsCancellationRequested) return true;
                sum += i % 7;
            }
            return false;
        });
        await Task.Yield();
        if (cancelled) return new(name, DiagnosticStatus.Cancelled, "Диагностика отменена.", TimeSpan.Zero);
        return new(name, DiagnosticStatus.Completed, $"Сумма: {sum}", TimeSpan.Zero);
    }
    public async Task<IReadOnlyList<DiagnosticResult>> RunAllAsync(CancellationToken token = default)
    {
        var tasks = new[] { RunAsync("Сеть", TimeSpan.FromMilliseconds(600), token), RunAsync("Диск", TimeSpan.FromMilliseconds(900), token), RunAsync("База", TimeSpan.FromMilliseconds(700), token) };
        return await Task.WhenAll(tasks);
    }
    public async Task<DiagnosticResult> RunWithTimeoutAsync(string name, TimeSpan duration, TimeSpan timeout, CancellationToken token = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(timeout);
        try
        {
            return await RunAsync(name, duration, cts.Token);
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested && cts.IsCancellationRequested)
        {
            throw new TimeoutException($"Тайм-аут диагностики {name}.");
        }
    }
    public async Task<string> FirstFinishedAsync(CancellationToken token = default)
    {
        var tasks = new[] { RunAsync("Быстрая", TimeSpan.FromMilliseconds(150), token), RunAsync("Средняя", TimeSpan.FromMilliseconds(350), token), RunAsync("Медленная", TimeSpan.FromMilliseconds(600), token) };
        var first = await Task.WhenAny(tasks);
        var result = await first;
        return result.Name;
    }
    public async Task<DiagnosticResult> RunSafeAsync(string name, TimeSpan duration, CancellationToken token = default)
    {
        try
        {
            return await RunAsync(name, duration, token);
        }
        catch (OperationCanceledException)
        {
            return new(name, DiagnosticStatus.Cancelled, "Диагностика отменена.", TimeSpan.Zero);
        }
        catch (Exception ex)
        {
            return new(name, DiagnosticStatus.Failed, $"{ex.GetType().Name}: {ex.Message}", TimeSpan.Zero);
        }
    }
}
