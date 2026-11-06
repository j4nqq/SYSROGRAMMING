namespace ParallelComputingLab;

internal class Program
{
    static void Main()
    {
        Console.WriteLine("=== Parallel Computing Lab ===");

        int[] numbers = DataGenerator.Generate(
            size: 1_000_000,
            seed: 12345);

        BenchmarkRunner.Run(numbers);

        Check("Один элемент (делится)", new[] { 26 });
        Check("Один элемент (не делится)", new[] { 7 });

        Console.WriteLine();
        Console.WriteLine("Работа завершена.");
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
}