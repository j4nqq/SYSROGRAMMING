namespace GameEventsAnalyzer;

internal class Program
{
    // П1, TODO 10. Почему PLINQ не обязательно быстрее LINQ:
    // распараллеливание не бесплатно: нужно разделить данные между потоками,
    // запустить и планировать работу, объединить частичные результаты
    // (при GroupBy и сортировке это особенно дорого). Если обработка одного
    // элемента простая (сравнение строки и сложение), а данных мало, эти расходы
    // больше выигрыша. Количество элементов само по себе не определяет стоимость
    // работы. PLINQ выигрывает на больших объёмах и тяжёлых независимых вычислениях.

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=== Game Events Analyzer ===");

        List<GameEvent> events =
            GameEventGenerator.Generate(
                count: 1_000_000,
                seed: 12345);

        Console.WriteLine($"Всего событий: {events.Count:N0}");

        var linqResult =
            GameEventAnalyzer.AnalyzeWithLinq(events);

        var plinqResult =
            GameEventAnalyzer.AnalyzeWithPlinq(events);

        Console.WriteLine();
        Console.WriteLine("Топ игроков:");

        foreach (var player in plinqResult.Take(10))
        {
            Console.WriteLine(
                $"Игрок {player.PlayerId}: " +
                $"убийств {player.KillCount}, " +
                $"очков {player.TotalPoints}");
        }

        Console.WriteLine();

        // TODO 5: сравнение результатов LINQ и PLINQ через SequenceEqual.
        bool same = linqResult
            .SequenceEqual(plinqResult);

        Console.WriteLine(
            $"Результаты LINQ и PLINQ совпадают: {same}");

        RunScenarios();
        DemoSideEffects(events);

        Console.WriteLine();
        Console.WriteLine("=== Измерения ===");

        int[] sizes =
        {
            1_000,
            10_000,
            100_000,
            1_000_000
        };

        foreach (int size in sizes)
        {
            var sizedEvents = GameEventGenerator.Generate(
                size,
                seed: 12345);

            Console.WriteLine();
            Console.WriteLine($"Событий: {size:N0}");

            BenchmarkRunner.Compare(sizedEvents);
        }

        Console.WriteLine();
        Console.WriteLine("Работа завершена.");
    }

    // TODO 6, 7, 8, 9: контрольные и дополнительные сценарии.
    static void RunScenarios()
    {
        Console.WriteLine();
        Console.WriteLine("=== Контрольные сценарии ===");

        var testEvents = new List<GameEvent>
        {
            new(1, 1, "Kill", 100, DateTime.Now),
            new(2, 1, "Kill", 150, DateTime.Now),
            new(3, 2, "Kill", 200, DateTime.Now),
            new(4, 1, "Death", 0, DateTime.Now),
            new(5, 2, "Loot", 20, DateTime.Now)
        };

        CheckScenario("Контрольная коллекция (ожидается: 1 - 2 убийства, 250; 2 - 1 убийство, 200)", testEvents);

        // TODO 6.
        CheckScenario("Пустая коллекция (ожидается: пустой список)", new List<GameEvent>());

        // TODO 7.
        CheckScenario("Только Death и Loot (ожидается: пустой список)", new List<GameEvent>
        {
            new(1, 1, "Death", 0, DateTime.Now),
            new(2, 2, "Loot", 20, DateTime.Now)
        });

        // TODO 8.
        CheckScenario("Все убийства одного игрока (ожидается: одна запись)", new List<GameEvent>
        {
            new(1, 7, "Kill", 100, DateTime.Now),
            new(2, 7, "Kill", 120, DateTime.Now),
            new(3, 7, "Kill", 80, DateTime.Now)
        });

        // TODO 9: null вместо коллекции должен давать ArgumentNullException,
        // его выбрасывает ArgumentNullException.ThrowIfNull(events) в начале метода:
        // null не имеет смысла как набор событий.
        Console.WriteLine();
        Console.WriteLine("null вместо коллекции:");

        try
        {
            GameEventAnalyzer.AnalyzeWithLinq(null!);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"  LINQ:  {ex.GetType().Name}, параметр: {ex.ParamName}");
        }

        try
        {
            GameEventAnalyzer.AnalyzeWithPlinq(null!);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"  PLINQ: {ex.GetType().Name}, параметр: {ex.ParamName}");
        }
    }

    static void CheckScenario(string name, List<GameEvent> events)
    {
        var linq = GameEventAnalyzer.AnalyzeWithLinq(events);
        var plinq = GameEventAnalyzer.AnalyzeWithPlinq(events);

        Console.WriteLine();
        Console.WriteLine(name);

        if (plinq.Count == 0)
        {
            Console.WriteLine("  (пусто)");
        }

        foreach (var player in plinq)
        {
            Console.WriteLine(
                $"  Игрок {player.PlayerId}: убийств {player.KillCount}, очков {player.TotalPoints}");
        }

        Console.WriteLine($"  Записей: {plinq.Count}, LINQ и PLINQ совпадают: {linq.SequenceEqual(plinq)}");
    }

    // Типичная ошибка: побочные эффекты и её исправление.
    static void DemoSideEffects(List<GameEvent> events)
    {
        Console.WriteLine();
        Console.WriteLine("=== Побочные эффекты (ForAll и общий List) ===");
        Console.WriteLine($"Ожидается элементов: {events.Count:N0}");

        for (int attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                var wrong = GameEventAnalyzer.GetPlayerIdsWrong(events);
                Console.WriteLine(
                    $"Ошибочный вариант, попытка {attempt}: получено {wrong.Count:N0}, " +
                    $"верно: {wrong.Count == events.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Ошибочный вариант, попытка {attempt}: исключение {ex.GetType().Name}");
            }
        }

        var correct = GameEventAnalyzer.GetPlayerIds(events);
        Console.WriteLine(
            $"Исправленный вариант: получено {correct.Count:N0}, верно: {correct.Count == events.Count}");
    }
}
