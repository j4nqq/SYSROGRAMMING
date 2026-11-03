using BankRaceCondition.Models;

namespace BankRaceCondition.Services;

public sealed class BankSimulator
{
    public Task RunUnsafeAsync(
        BankAccount account,
        int workers,
        int operationsPerWorker,
        decimal amount) =>
        RunAsync(account.DepositUnsafe, workers, operationsPerWorker, amount);

    public Task RunSafeAsync(
        BankAccount account,
        int workers,
        int operationsPerWorker,
        decimal amount) =>
        RunAsync(account.Deposit, workers, operationsPerWorker, amount);

    private static async Task RunAsync(
        Action<decimal> deposit,
        int workers,
        int operationsPerWorker,
        decimal amount)
    {
        Task[] tasks = Enumerable
            .Range(0, workers)
            .Select(_ => Task.Run(() =>
            {
                for (int i = 0; i < operationsPerWorker; i++)
                {
                    deposit(amount);
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }
}
