namespace ParallelComputingLab;

internal class Program
{
    // Схема таблицы эксперимента (время в мс, заполните по результатам своего запуска):
    //
    // | Размер массива | Последовательно | Degree = 1 | Degree = 2 | Degree = 3 | Degree = 4 | Default |
    // | -------------- | --------------- | ---------- | ---------- | ---------- | ---------- | ------- |
    // | 10 000         |                 |            |            |            |            |         |
    // | 100 000        |                 |            |            |            |            |         |
    // | 1 000 000      |                 |            |            |            |            |         |
    // | 5 000 000      |                 |            |            |            |            |         |
    // | 10 000 000     |                 |            |            |            |            |         |
    //
    // Программа сама печатает такую таблицу в конце работы (PrintSummary).

    // TODO 7: запустите программу ещё раз после перезапуска и сравните итоговые таблицы.
    // Абсолютное время будет немного отличаться (нагрузка системы, кэши, JIT, фоновые
    // процессы), но общая картина (на каких размерах параллельный вариант выигрывает)
    // должна повторяться.

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=== Parallel Computing Lab ===");

        int[] sizes =
        {
            10_000,
            100_000,
            1_000_000,
            5_000_000,
            10_000_000
        };

        var reports = new List<ComparisonReport>();

        foreach (int size in sizes)
        {
            int[] numbers = DataGenerator.Generate(
                size,
                seed: 12345);

            BenchmarkRunner.Run(numbers);
            reports.Add(BenchmarkRunner.CompareParallelism(numbers));
            Console.WriteLine();
        }

        PrintSummary(reports);
        PrintConclusions(reports);
        RunChecks();

        Console.WriteLine();
        Console.WriteLine("Работа завершена.");
    }

    static string Label(int degree) => degree == -1 ? "Default" : degree.ToString();

    static void PrintSummary(List<ComparisonReport> reports)
    {
        Console.WriteLine("=== Итоговая таблица, мс ===");

        Console.Write($"{"Размер",12} | {"Последов.",10}");
        foreach (int degree in BenchmarkRunner.Degrees)
        {
            Console.Write($" | {"D=" + Label(degree),9}");
        }
        Console.WriteLine();

        foreach (ComparisonReport report in reports)
        {
            Console.Write($"{report.Size,12:N0} | {report.Sequential,10:F3}");
            foreach (int degree in BenchmarkRunner.Degrees)
            {
                Console.Write($" | {report.Parallel[degree],9:F3}");
            }
            Console.WriteLine();
        }
    }

    static void PrintConclusions(List<ComparisonReport> reports)
    {
        Console.WriteLine();
        Console.WriteLine("=== Выводы ===");

        // TODO 5: размер, начиная с которого параллельная обработка стала быстрее.
        int? threshold = null;
        for (int i = reports.Count - 1;
             i >= 0 && reports[i].Parallel.Values.Min() < reports[i].Sequential;
             i--)
        {
            threshold = reports[i].Size;
        }

        Console.WriteLine(threshold is null
            ? "TODO 5: на проверенных размерах параллельная обработка не стала быстрее."
            : $"TODO 5: параллельная обработка быстрее, начиная с размера {threshold:N0}.");

        // TODO 6: лучшее значение MaxDegreeOfParallelism для каждого размера.
        foreach (ComparisonReport report in reports)
        {
            var best = report.Parallel.MinBy(p => p.Value);
            Console.WriteLine(
                $"TODO 6: размер {report.Size,12:N0} - лучший Degree = {Label(best.Key)} ({best.Value:F3} мс)");
        }

        // TODO 8: почему Degree = 4 может быть хуже Degree = 2.
        // Больше потоков не значит быстрее. Если физических ядер или свободных ядер
        // меньше четырёх (или 4 логических потока - это 2 ядра с Hyper-Threading),
        // потоки конкурируют за одни и те же ядра, растут переключения контекста.
        // Наш цикл простой и упирается в память: потоки делят кэш и пропускную
        // способность памяти. Плюс накладные расходы на запуск потоков и объединение
        // локальных результатов. На небольших данных эти расходы больше выигрыша.
        // Фоновые процессы тоже занимают ядра.
        Console.WriteLine(
            "TODO 8: см. комментарий в коде - Degree = 4 может быть хуже Degree = 2 из-за накладных расходов и конкуренции за ядра и память.");

        // TODO 10: всегда ли параллельная обработка быстрее последовательной.
        int notFaster = reports.Count(r => r.Parallel.Values.Min() >= r.Sequential);

        Console.WriteLine(notFaster > 0
            ? $"TODO 10: нет, не всегда. На {notFaster} из {reports.Count} размеров даже лучший параллельный вариант не быстрее последовательного: на малых данных накладные расходы больше выигрыша."
            : "TODO 10: на проверенных размерах параллельный вариант оказался быстрее, но это не гарантия: на малых данных накладные расходы могут сделать его медленнее, а ускорение не растёт пропорционально числу потоков.");
    }

    // П1: TODO 3, 4, 5, 6 - проверки граничных случаев и ошибочного варианта.
    static void RunChecks()
    {
        Console.WriteLine();
        Console.WriteLine("=== Проверки П1 ===");

        Check("Один элемент (делится)", new[] { 26 });
        Check("Один элемент (не делится)", new[] { 7 });
        Check("Пустой массив", Array.Empty<int>());

        try
        {
            Calculator.CalculateSequential(null!);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Последовательно: {ex.GetType().Name}, параметр: {ex.ParamName}");
        }

        try
        {
            Calculator.CalculateParallel(null!);
        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"Параллельно:     {ex.GetType().Name}, параметр: {ex.ParamName}");
        }

        CompareWrong(5_000_000);
    }

    static void Check(string name, int[] data)
    {
        CalculationResult sequential = Calculator.CalculateSequential(data);
        CalculationResult parallel = Calculator.CalculateParallel(data);

        Console.WriteLine();
        Console.WriteLine(name);
        Console.WriteLine($"Последовательно: {sequential}");
        Console.WriteLine($"Параллельно:     {parallel}");
        Console.WriteLine($"Совпадают:       {sequential == parallel}");
    }

    static void CompareWrong(int size)
    {
        int[] numbers = DataGenerator.Generate(size);

        CalculationResult correct = Calculator.CalculateParallel(numbers);

        Console.WriteLine();
        Console.WriteLine($"Сравнение с исправленным вариантом, размер: {size:N0}");
        Console.WriteLine($"Правильный:  {correct}");

        for (int attempt = 1; attempt <= 5; attempt++)
        {
            CalculationResult wrong = Calculator.CalculateParallelWrong(numbers);
            Console.WriteLine($"Попытка {attempt}: {wrong}, совпадает: {wrong == correct}");
        }
    }
}
