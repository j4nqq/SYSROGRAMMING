namespace ParallelComputingLab;

public static class Calculator
{
    public static CalculationResult CalculateSequential(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        long sum = 0;
        int count = 0;

        foreach (int number in numbers)
        {
            if (number % 7 == 0)
            {
                sum += number;
                count++;
            }
        }

        return new CalculationResult(sum, count);
    }

    public static CalculationResult CalculateParallel(
        int[] numbers,
        int maxDegreeOfParallelism = -1)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        if (maxDegreeOfParallelism != -1 &&
            maxDegreeOfParallelism < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDegreeOfParallelism));
        }

        long sum = 0;
        int count = 0;

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        Parallel.For(
            0,
            numbers.Length,
            options,
            () => (LocalSum: 0L, LocalCount: 0),
            (i, state, local) =>
            {
                int number = numbers[i];

                if (number % 7 == 0)
                {
                    local.LocalSum += number;
                    local.LocalCount++;
                }

                return local;
            },
            local =>
            {
                Interlocked.Add(ref sum, local.LocalSum);
                Interlocked.Add(ref count, local.LocalCount);
            });

        return new CalculationResult(sum, count);
    }
}