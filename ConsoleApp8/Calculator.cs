namespace ParallelComputingLab;

public static class Calculator
{
    public static CalculationResult CalculateSequential(int[] numbers)
    {
        ArgumentNullException.ThrowIfNull(numbers);

        long sum = 0;
        int count = 0;
        int? max = null;

        foreach (int number in numbers)
        {
            if (number % 13 == 0)
            {
                sum += number;
                count++;

                if (max is null || number > max)
                {
                    max = number;
                }
            }
        }

        return new CalculationResult(sum, count, max);
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
        int? max = null;
        var maxLock = new object();

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism
        };

        Parallel.For(
            0,
            numbers.Length,
            options,
            () => (LocalSum: 0L, LocalCount: 0, LocalMax: (int?)null),
            (i, state, local) =>
            {
                int number = numbers[i];

                if (number % 13 == 0)
                {
                    local.LocalSum += number;
                    local.LocalCount++;

                    if (local.LocalMax is null || number > local.LocalMax)
                    {
                        local.LocalMax = number;
                    }
                }

                return local;
            },
            local =>
            {
                Interlocked.Add(ref sum, local.LocalSum);
                Interlocked.Add(ref count, local.LocalCount);

                if (local.LocalMax is not null)
                {
                    lock (maxLock)
                    {
                        if (max is null || local.LocalMax > max)
                        {
                            max = local.LocalMax;
                        }
                    }
                }
            });

        return new CalculationResult(sum, count, max);
    }
}