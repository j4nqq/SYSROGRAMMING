using ConsoleApp2.Models;

namespace ConsoleApp2.Services;

public sealed class LogStatistics
{
    private readonly int[] _counts = new int[Enum.GetValues<LogLevel>().Length];
    private int _skipped;

    public int Skipped => Volatile.Read(ref _skipped);

    public void Increment(LogLevel level)
    {
        Add(level, 1);
    }

    public void Add(LogLevel level, int count)
    {
        Interlocked.Add(ref _counts[(int)level], count);
    }

    public void IncrementSkipped()
    {
        Interlocked.Increment(ref _skipped);
    }

    public void AddSkipped(int count)
    {
        Interlocked.Add(ref _skipped, count);
    }

    public int Get(LogLevel level)
    {
        return Volatile.Read(ref _counts[(int)level]);
    }
}