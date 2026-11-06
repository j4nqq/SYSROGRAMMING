using Xunit;
using ParallelComputingLab;

namespace ConsoleApp8.Tests;

public class CalculatorTests
{
    [Theory]
    [InlineData(new[] { 13, 26, 20, 39 }, 78, 3)]
    [InlineData(new[] { 1, 2, 3, 4 }, 0, 0)]
    [InlineData(new[] { 13, 13, 13 }, 39, 3)]
    [InlineData(new int[] { }, 0, 0)]
    [InlineData(new[] { 26 }, 26, 1)]
    public void SequentialAndParallel_ReturnSameResult(
        int[] numbers,
        long expectedSum,
        int expectedCount)
    {
        var sequential = Calculator.CalculateSequential(numbers);
        var parallel = Calculator.CalculateParallel(numbers);

        Assert.Equal(expectedSum, sequential.Sum);
        Assert.Equal(expectedCount, sequential.Count);
        Assert.Equal(sequential, parallel);
    }

    [Fact]
    public void Max_IsLargestMatchingElement()
    {
        var result = Calculator.CalculateParallel(new[] { 13, 39, 26, 5 });

        Assert.Equal(39, result.Max);
    }

    [Fact]
    public void Max_IsNull_WhenNothingMatches()
    {
        var result = Calculator.CalculateParallel(new[] { 1, 2, 3 });

        Assert.Null(result.Max);
    }

    [Fact]
    public void Parallel_WithNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(
            () => Calculator.CalculateParallel(null!));
    }

    [Fact]
    public void Parallel_WithInvalidDegree_ThrowsException()
    {
        int[] numbers = { 13, 26, 39 };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => Calculator.CalculateParallel(numbers, 0));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(-1)]
    public void Parallel_WithDifferentDegrees_ReturnsSameResult(int degree)
    {
        int[] numbers = DataGenerator.Generate(100_000, seed: 12345);

        var expected = Calculator.CalculateSequential(numbers);
        var actual = Calculator.CalculateParallel(numbers, degree);

        Assert.Equal(expected, actual);
    }
}
