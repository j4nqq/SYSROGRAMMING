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

    static void CompareWrong(int size)
    {
        int[] numbers = DataGenerator.Generate(size);

        CalculationResult correct = Calculator.CalculateParallel(numbers);

        Console.WriteLine();
        Console.WriteLine($"Сравнение с ошибочным вариантом, размер: {size:N0}");
        Console.WriteLine($"Правильный:  {correct}");

        for (int attempt = 1; attempt <= 5; attempt++)
        {
            CalculationResult wrong = WrongCalculator.CalculateParallelWrong(numbers);
            Console.WriteLine($"Ошибочный {attempt}: {wrong}, совпадает: {wrong == correct}");
        }
    }
}