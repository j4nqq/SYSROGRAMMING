using System.Collections.Concurrent;
using ConsoleApp2.Models;

namespace ConsoleApp2.Services;

// Фиксированное число worker-ов обрабатывает записи из очереди.
public sealed class LogEntryProcessor : IDisposable
{
    private readonly BlockingCollection<LogEntry> _entries = new();
    private readonly LogStatistics _statistics;
    private readonly Action<int, LogEntry> _onProcessed;
    private readonly Task[] _workers;

    public LogEntryProcessor(
        int workerCount,
        LogStatistics statistics,
        Action<int, LogEntry> onProcessed)
    {
        _statistics = statistics;
        _onProcessed = onProcessed;
        _workers = new Task[workerCount];

        for (int i = 0; i < workerCount; i++)
        {
            int workerId = i + 1;
            _workers[i] = Task.Run(() => ProcessEntries(workerId));
        }
    }

    public void Add(LogEntry entry)
    {
        _entries.Add(entry);
    }

    public async Task CompleteAsync()
    {
        _entries.CompleteAdding();
        await Task.WhenAll(_workers);
    }

    private void ProcessEntries(int workerId)
    {
        foreach (LogEntry entry in _entries.GetConsumingEnumerable())
        {
            _statistics.Increment(entry.Level);
            _onProcessed(workerId, entry);
        }
    }

    public void Dispose()
    {
        _entries.Dispose();
    }
}