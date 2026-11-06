using Xunit;
using GameEventsAnalyzer;

namespace ConsoleApp9.Tests;

public class GameEventOrderingTests
{
    private static readonly DateTime TestDate =
        new(2026, 1, 1);

    [Fact]
    public void GetKillsUnordered_ReturnsSameEventsAsOrdered()
    {
        var events = GameEventGenerator.Generate(50_000, seed: 12345);

        var unordered = GameEventAnalyzer.GetKillsUnordered(events);
        var ordered = GameEventAnalyzer.GetKillsOrdered(events);

        Assert.Equal(
            ordered.Select(e => e.Id).OrderBy(id => id),
            unordered.Select(e => e.Id).OrderBy(id => id));
    }

    [Fact]
    public void GetKillsOrdered_GeneratedData_IdsAreAscending()
    {
        var events = GameEventGenerator.Generate(100_000, seed: 12345);

        var ordered = GameEventAnalyzer.GetKillsOrdered(events);

        Assert.Equal(
            ordered.Select(e => e.Id).OrderBy(id => id),
            ordered.Select(e => e.Id));
    }

    [Fact]
    public void GetKillsSortedByTime_EqualTimes_UsesIdAsTieBreaker()
    {
        var events = new List<GameEvent>
        {
            new(3, 1, "Kill", 100, TestDate),
            new(1, 2, "Kill", 100, TestDate),
            new(2, 3, "Kill", 100, TestDate)
        };

        var result = GameEventAnalyzer.GetKillsSortedByTime(events);

        Assert.Equal(new[] { 1, 2, 3 }, result.Select(e => e.Id));
    }

    [Fact]
    public void GetKillsSortedByTime_WithAndWithoutAsOrdered_GiveSameOrder()
    {
        var events = GameEventGenerator.Generate(
            20_000,
            seed: 12345,
            eventsPerSecond: 3);

        var plain = GameEventAnalyzer.GetKillsSortedByTime(events);
        var asOrdered = GameEventAnalyzer.GetKillsSortedByTimeWithAsOrdered(events);

        Assert.Equal(plain, asOrdered);
    }

    [Fact]
    public void Generator_EventsPerSecond_CreatesEqualTimestamps()
    {
        var events = GameEventGenerator.Generate(9, seed: 12345, eventsPerSecond: 3);

        Assert.Equal(3, events.Select(e => e.Timestamp).Distinct().Count());
    }

    [Fact]
    public void Generator_InvalidEventsPerSecond_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => GameEventGenerator.Generate(10, 12345, 0));
    }

    [Fact]
    public void AnalyzeByType_LinqAndPlinq_ReturnSameResult()
    {
        var events = GameEventGenerator.Generate(100_000, seed: 12345);

        var linq = GameEventAnalyzer.AnalyzeByTypeWithLinq(events);
        var plinq = GameEventAnalyzer.AnalyzeByTypeWithPlinq(events);

        Assert.Equal(linq, plinq);
        Assert.Equal(events.Count, linq.Sum(x => x.Count));
    }

    [Fact]
    public void AnalyzeLootWithAggregate_MatchesManualCalculation()
    {
        var events = GameEventGenerator.Generate(100_000, seed: 12345);

        var loot = events.Where(e => e.Type == "Loot").ToList();

        var summary = GameEventAnalyzer.AnalyzeLootWithAggregate(events);

        Assert.Equal(
            new LootSummary(loot.Count, loot.Sum(e => (long)e.Points)),
            summary);
    }

    [Fact]
    public void GetKills_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(
            () => GameEventAnalyzer.GetKillsUnordered(null!));

        Assert.Throws<ArgumentNullException>(
            () => GameEventAnalyzer.GetKillsOrdered(null!));

        Assert.Throws<ArgumentNullException>(
            () => GameEventAnalyzer.GetKillsSortedByTime(null!));
    }
}
