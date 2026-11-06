namespace ParallelComputingLab;

public static class WrongCalculator
{
    public static CalculationResult CalculateParallelWrong(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        long sum = 0;
        int count = 0;
        int? max = null;

        Parallel.For(0, numbers.Length, i =>
        {
            int number = numbers[i];

            if (number % 13 == 0)
            {
                sum += number;
                count++;

                if (max is null || number > max)
                {
                    max = number;
                }
            }
        });

        return new CalculationResult(sum, count, max);
    }
}