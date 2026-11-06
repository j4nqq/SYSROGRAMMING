using System.Diagnostics;

namespace ParallelComputingLab;

public sealed record ComparisonReport(
    int Size,
    double Sequential,
    Dictionary<int, double> Parallel);

public static class BenchmarkRunner
{
    private const int MeasurementCount = 10;

    public static readonly int[] Degrees = { 1, 2, 3, 4, -1 };

    public static void Run(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        Console.WriteLine();
        Console.WriteLine($"Размер массива: {numbers.Length:N0}");

        Calculator.CalculateSequential(numbers);
        Calculator.CalculateParallel(numbers);

        var sequentialTimes = new List<double>();
        var parallelTimes = new List<double>();

        CalculationResult? expectedResult = null;

        for (int i = 0; i < MeasurementCount; i++)
        {
            var sequential = Measure(
                () => Calculator.CalculateSequential(numbers));

            var parallel = Measure(
                () => Calculator.CalculateParallel(numbers));

            expectedResult ??= sequential.Result;

            if (sequential.Result != parallel.Result)
            {
                throw new InvalidOperationException(
                    "Последовательный и параллельный результаты не совпадают.");
            }

            sequentialTimes.Add(sequential.ElapsedMilliseconds);
            parallelTimes.Add(parallel.ElapsedMilliseconds);
        }

        Console.WriteLine($"Сумма:           {expectedResult!.Sum:N0}");
        Console.WriteLine($"Количество:      {expectedResult.Count:N0}");
        Console.WriteLine($"Максимум:        {expectedResult.Max}");
        Console.WriteLine($"Последовательно: {Median(sequentialTimes):F3} мс");
        Console.WriteLine($"Параллельно:     {Median(parallelTimes):F3} мс");
    }

    public static ComparisonReport CompareParallelism(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        Console.WriteLine();
        Console.WriteLine($"Размер массива: {numbers.Length:N0}");
        Console.WriteLine(
            $"{"Степень параллелизма",20} | {"Время, мс",10} | {"Ускорение",9} | Совпадает");
        Console.WriteLine(new string('-', 62));

        CalculationResult expected = Calculator.CalculateSequential(numbers);

        foreach (int degree in Degrees)
        {
            Calculator.CalculateParallel(numbers, degree);
        }

        double sequentialTime = MeasureMedian(
            () => Calculator.CalculateSequential(numbers),
            expected);

        Console.WriteLine(
            $"{"Последовательно",20} | {sequentialTime,10:F3} | {1.0,9:F2} | да");

        var parallelTimes = new Dictionary<int, double>();

        foreach (int degree in Degrees)
        {
            double time = MeasureMedian(
                () => Calculator.CalculateParallel(numbers, degree),
                expected);

            parallelTimes[degree] = time;

            Console.WriteLine(
                $"{degree,20} | {time,10:F3} | {sequentialTime / time,9:F2} | да");
        }

        return new ComparisonReport(
            numbers.Length,
            sequentialTime,
            parallelTimes);
    }

    private static double MeasureMedian(
        Func<CalculationResult> action,
        CalculationResult expected)
    {
        var times = new List<double>();

        for (int i = 0; i < MeasurementCount; i++)
        {
            var measurement = Measure(action);

            if (measurement.Result != expected)
            {
                throw new InvalidOperationException(
                    "Результат варианта не совпал с последовательным.");
            }

            times.Add(measurement.ElapsedMilliseconds);
        }

        return Median(times);
    }

    public static (CalculationResult Result, double ElapsedMilliseconds)
        Measure(Func<CalculationResult> action)
    {
        var stopwatch = Stopwatch.StartNew();
        CalculationResult result = action();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed.TotalMilliseconds);
    }

    private static double Median(List<double> values)
    {
        double[] sorted = values.OrderBy(x => x).ToArray();
        int middle = sorted.Length / 2;

        if (sorted.Length % 2 == 1)
        {
            return sorted[middle];
        }

        return (sorted[middle - 1] + sorted[middle]) / 2;
    }
}
