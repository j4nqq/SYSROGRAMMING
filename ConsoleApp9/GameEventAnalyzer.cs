namespace GameEventsAnalyzer;

public sealed record EventSummary(
    int TotalEvents,
    long TotalPoints,
    int KillCount,
    int DeathCount);

public sealed record EventTypeStatistics(
    string Type,
    int Count);

public sealed record LootSummary(
    int LootCount,
    long LootPoints);

// LINQ.
public static partial class GameEventAnalyzer
{
    public static List<PlayerStatistics> AnalyzeWithLinq(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .Where(e => e.Type == "Kill")
            .GroupBy(e => e.PlayerId)
            .Select(group => new PlayerStatistics(
                PlayerId: group.Key,
                KillCount: group.Count(),
                TotalPoints: group.Sum(e => (long)e.Points)))
            .OrderByDescending(x => x.TotalPoints)
            .ThenBy(x => x.PlayerId)
            .ToList();
    }
}

// PLINQ.
public static partial class GameEventAnalyzer
{
    public static List<PlayerStatistics> AnalyzeWithPlinq(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        // GroupBy, Select и OrderByDescending здесь выполняются в рамках PLINQ-запроса.
        // Итоговый список уже отсортирован по сумме очков, поэтому AsOrdered()
        // для сохранения исходного порядка событий нам не требуется.
        // ThenBy по PlayerId даёт одинаковый порядок при равных очках.
        return events
            .AsParallel()
            .Where(e => e.Type == "Kill")
            .GroupBy(e => e.PlayerId)
            .Select(group => new PlayerStatistics(
                PlayerId: group.Key,
                KillCount: group.Count(),
                TotalPoints: group.Sum(e => (long)e.Points)))
            .OrderByDescending(x => x.TotalPoints)
            .ThenBy(x => x.PlayerId)
            .ToList();
    }
}

// Побочные эффекты.
public static partial class GameEventAnalyzer
{
    // Неправильный вариант: несколько потоков одновременно меняют List<int>.
    public static List<int> GetPlayerIdsWrong(
        IEnumerable<GameEvent> events)
    {
        var result = new List<int>();

        events
            .AsParallel()
            .ForAll(e =>
            {
                // ОШИБКА: List<T> не рассчитан на одновременную запись из потоков.
                result.Add(e.PlayerId);
            });

        return result;
    }

    // Исправленный вариант.
    // ToList() в конце запроса безопаснее, чем Add из ForAll: PLINQ сам
    // собирает частичные результаты потоков и объединяет их в одну коллекцию,
    // а общий список изменяют не потоки, а сам механизм запроса, с синхронизацией.
    public static List<int> GetPlayerIds(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .Select(e => e.PlayerId)
            .ToList();
    }
}

// Порядок обработки и AsOrdered.
public static partial class GameEventAnalyzer
{
    // PLINQ без сохранения порядка.
    public static List<GameEvent> GetKillsUnordered(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .Where(e => e.Type == "Kill")
            .ToList();
    }

    // PLINQ с сохранением порядка.
    public static List<GameEvent> GetKillsOrdered(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .AsOrdered()
            .Where(e => e.Type == "Kill")
            .ToList();
    }

    // Формирует ленту событий по времени.
    public static List<GameEvent> GetKillsSortedByTime(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        // Здесь AsOrdered() не нужен: порядок задаётся явно сортировкой.
        return events
            .AsParallel()
            .Where(e => e.Type == "Kill")
            .OrderBy(e => e.Timestamp)
            .ThenBy(e => e.Id)
            .ToList();
    }

    // TODO 6: AsOrdered вместе с OrderBy. OrderBy сам задаёт порядок результата,
    // поэтому итоговый порядок должен совпасть с запросом без AsOrdered.
    public static List<GameEvent> GetKillsSortedByTimeWithAsOrdered(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .AsOrdered()
            .Where(e => e.Type == "Kill")
            .OrderBy(e => e.Timestamp)
            .ThenBy(e => e.Id)
            .ToList();
    }

    // TODO 8: сортировка без ThenBy. Если у нескольких событий одинаковое время,
    // порядок между ними не определён, результат может отличаться от запуска к запуску.
    public static List<GameEvent> GetKillsSortedByTimeWithoutThenBy(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .Where(e => e.Type == "Kill")
            .OrderBy(e => e.Timestamp)
            .ToList();
    }
}

// Aggregate.
public static partial class GameEventAnalyzer
{
    public static EventSummary AnalyzeWithAggregate(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .Aggregate(
                // Создание локального аккумулятора.
                () => new EventSummaryAccumulator(),
                // Обработка одного элемента.
                (local, gameEvent) =>
                {
                    local.TotalEvents++;
                    local.TotalPoints += gameEvent.Points;

                    if (gameEvent.Type == "Kill")
                    {
                        local.KillCount++;
                    }

                    if (gameEvent.Type == "Death")
                    {
                        local.DeathCount++;
                    }

                    return local;
                },
                // Объединение двух аккумуляторов.
                (first, second) =>
                {
                    first.TotalEvents += second.TotalEvents;
                    first.TotalPoints += second.TotalPoints;
                    first.KillCount += second.KillCount;
                    first.DeathCount += second.DeathCount;

                    return first;
                },
                // Получение конечного результата.
                result => new EventSummary(
                    result.TotalEvents,
                    result.TotalPoints,
                    result.KillCount,
                    result.DeathCount));
    }

    private sealed class EventSummaryAccumulator
    {
        public int TotalEvents { get; set; }
        public long TotalPoints { get; set; }
        public int KillCount { get; set; }
        public int DeathCount { get; set; }
    }
}

// Дополнительные задания П2.
public static partial class GameEventAnalyzer
{
    // Статистика по типу события, LINQ.
    // Для подсчёта количества обычного LINQ может быть достаточно: работа над
    // каждым элементом очень простая (взять Type и увеличить счётчик), групп всего
    // четыре, поэтому расходы PLINQ на разделение данных и объединение групп
    // сопоставимы с самим подсчётом.
    public static List<EventTypeStatistics> AnalyzeByTypeWithLinq(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .GroupBy(e => e.Type)
            .Select(group => new EventTypeStatistics(group.Key, group.Count()))
            .OrderBy(x => x.Type, StringComparer.Ordinal)
            .ToList();
    }

    // Статистика по типу события, PLINQ.
    public static List<EventTypeStatistics> AnalyzeByTypeWithPlinq(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .GroupBy(e => e.Type)
            .Select(group => new EventTypeStatistics(group.Key, group.Count()))
            .OrderBy(x => x.Type, StringComparer.Ordinal)
            .ToList();
    }

    // Aggregate: количество событий Loot и сумма очков за них.
    public static LootSummary AnalyzeLootWithAggregate(
        IEnumerable<GameEvent> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .AsParallel()
            .Aggregate(
                () => new LootAccumulator(),
                (local, gameEvent) =>
                {
                    if (gameEvent.Type == "Loot")
                    {
                        local.LootCount++;
                        local.LootPoints += gameEvent.Points;
                    }

                    return local;
                },
                (first, second) =>
                {
                    first.LootCount += second.LootCount;
                    first.LootPoints += second.LootPoints;

                    return first;
                },
                result => new LootSummary(
                    result.LootCount,
                    result.LootPoints));
    }

    private sealed class LootAccumulator
    {
        public int LootCount { get; set; }
        public long LootPoints { get; set; }
    }
}