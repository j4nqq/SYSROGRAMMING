namespace DeadlockLab;

public class TransferDemo
{
    public static async Task RunUnsafeAsync()
    {
        Console.WriteLine("=== UNSAFE TRANSFER DEMO ===");

        BankAccount accountA = new(10000);
        BankAccount accountB = new(20000);

        BankTransferService service = new();

        const int operations = 10000;
        const decimal amount = 1;

        Task[] tasks = new Task[2];

        tasks[0] = Task.Run(() =>
        {
            for (int i = 0; i < operations; i++)
            {
                service.TransferUnsafe(
                    accountA,
                    accountB,
                    amount
                );
            }
        });

        tasks[1] = Task.Run(() =>
        {
            for (int i = 0; i < operations; i++)
            {
                service.TransferUnsafe(
                    accountB,
                    accountA,
                    amount
                );
            }
        });

        try
        {
            await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5));

            Console.WriteLine("Deadlock не воспроизвёлся, переводы завершены.");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("DEADLOCK ОБНАРУЖЕН!");
            Console.WriteLine("Переводы не завершились за 5 секунд.");
        }
    }

    public static async Task RunSafeAsync()
    {
        Console.WriteLine("=== SAFE TRANSFER DEMO ===");

        BankAccount accountA = new(10000);
        BankAccount accountB = new(20000);

        BankTransferService service = new();

        const int operations = 10000;
        const decimal amount = 1;

        Task[] tasks = new Task[2];

        tasks[0] = Task.Run(() =>
        {
            for (int i = 0; i < operations; i++)
            {
                service.TransferSafe(
                    accountA,
                    accountB,
                    amount
                );
            }
        });

        tasks[1] = Task.Run(() =>
        {
            for (int i = 0; i < operations; i++)
            {
                service.TransferSafe(
                    accountB,
                    accountA,
                    amount
                );
            }
        });

        await Task.WhenAll(tasks);

        Console.WriteLine(
            $"Баланс A: {accountA.Balance}"
        );

        Console.WriteLine(
            $"Баланс B: {accountB.Balance}"
        );

        Console.WriteLine(
            $"Общий баланс: {accountA.Balance + accountB.Balance}"
        );
    }

    public static void RunInsufficientFunds()
    {
        Console.WriteLine("=== INSUFFICIENT FUNDS DEMO ===");

        BankAccount accountA = new(100);
        BankAccount accountB = new(200);

        BankTransferService service = new();

        try
        {
            service.TransferSafe(accountA, accountB, 500);
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }

        Console.WriteLine($"Баланс A: {accountA.Balance}");
        Console.WriteLine($"Баланс B: {accountB.Balance}");
    }

    public static void RunSameAccount()
    {
        Console.WriteLine("=== SAME ACCOUNT DEMO ===");

        BankAccount account = new(1000);

        BankTransferService service = new();

        try
        {
            service.TransferSafe(account, account, 100);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }

        Console.WriteLine($"Баланс: {account.Balance}");
    }

    public static async Task RunThreeAccountsAsync()
    {
        Console.WriteLine("=== THREE ACCOUNTS DEMO ===");

        BankAccount accountA = new(10000);
        BankAccount accountB = new(10000);
        BankAccount accountC = new(10000);

        BankTransferService service = new();

        const int operations = 1000;
        const decimal amount = 1;

        (BankAccount From, BankAccount To)[] routes =
        {
            (accountA, accountB),
            (accountB, accountC),
            (accountC, accountA),
            (accountB, accountA),
            (accountC, accountB),
            (accountA, accountC)
        };

        Task[] tasks = routes
            .Select(route => Task.Run(() =>
            {
                for (int i = 0; i < operations; i++)
                {
                    service.TransferSafe(
                        route.From,
                        route.To,
                        amount
                    );
                }
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        Console.WriteLine($"Баланс A: {accountA.Balance}");
        Console.WriteLine($"Баланс B: {accountB.Balance}");
        Console.WriteLine($"Баланс C: {accountC.Balance}");

        Console.WriteLine(
            $"Общий баланс: {accountA.Balance + accountB.Balance + accountC.Balance}"
        );
    }
}
