using System.Threading.Channels;
using ConsoleApp2.Models;

namespace ConsoleApp2.Services;

// Фиксированное число worker-ов обрабатывает записи из bounded Channel.
public sealed class LogEntryProcessor
{
    private readonly Channel<LogEntry> _channel;
    private readonly LogStatistics _statistics;
    private readonly Action<int, LogEntry> _onProcessed;
    private readonly Task[] _workers;

    public LogEntryProcessor(
        int workerCount,
        int capacity,
        LogStatistics statistics,
        Action<int, LogEntry> onProcessed)
    {
        _statistics = statistics;
        _onProcessed = onProcessed;

        _channel = Channel.CreateBounded<LogEntry>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
            SingleReader = false
        });

        _workers = new Task[workerCount];

        for (int i = 0; i < workerCount; i++)
        {
            int workerId = i + 1;
            _workers[i] = Task.Run(() => ProcessEntriesAsync(workerId));
        }
    }

    public async ValueTask AddAsync(LogEntry entry)
    {
        await _channel.Writer.WriteAsync(entry);
    }

    public async Task CompleteAsync()
    {
        _channel.Writer.Complete();
        await Task.WhenAll(_workers);
    }

    private async Task ProcessEntriesAsync(int workerId)
    {
        await foreach (LogEntry entry in _channel.Reader.ReadAllAsync())
        {
            _statistics.Increment(entry.Level);
            _onProcessed(workerId, entry);
        }
    }
}