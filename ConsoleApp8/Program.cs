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

        Console.WriteLine();
        Console.WriteLine("Работа завершена.");
    }
}