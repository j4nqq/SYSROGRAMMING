using System.Diagnostics;
namespace AsyncMonitor;
public sealed class CheckService
{
    public async Task<CheckResult> CheckServiceAsync(string name, TimeSpan delay, CancellationToken cancellationToken = default)
    {
        // TODO 1: Отклонить пустое имя проверки.
        // TODO 2: Отклонить отрицательную длительность через ArgumentOutOfRangeException.
        var sw = Stopwatch.StartNew();
        // TODO 3: Использовать await Task.Delay(delay, cancellationToken), а не Thread.Sleep.
        sw.Stop();
        // TODO 4: Вернуть корректный CheckResult.
    }
    public async Task<IReadOnlyList<CheckResult>> RunSequentialAsync(IReadOnlyList<(string Name, TimeSpan Delay)> checks, CancellationToken token = default)
    {
        var results = new List<CheckResult>();
        // TODO 5: Выполнить проверки по одной, ожидая каждую через await.
    }
    public async Task<IReadOnlyList<CheckResult>> RunParallelAsync(IReadOnlyList<(string Name, TimeSpan Delay)> checks, CancellationToken token = default)
    {
        // TODO 6: Сначала создать задачи для всех проверок.
        // TODO 7: Совместно дождаться завершения всех задач через Task.WhenAll.
    }
}
