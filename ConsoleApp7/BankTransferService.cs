namespace DeadlockLab;

public class BankTransferService
{
    public void TransferUnsafe(
        BankAccount from,
        BankAccount to,
        decimal amount)
    {
        if (ReferenceEquals(from, to))
            throw new ArgumentException(
                "Нельзя переводить деньги на тот же счёт."
            );

        lock (from.SyncRoot)
        {
            lock (to.SyncRoot)
            {
                if (from.GetBalanceUnsafe() < amount)
                    throw new InvalidOperationException(
                        "Недостаточно средств."
                    );

                from.SetBalanceUnsafe(
                    from.GetBalanceUnsafe() - amount
                );

                to.SetBalanceUnsafe(
                    to.GetBalanceUnsafe() + amount
                );
            }
        }
    }

    public void TransferSafe(
        BankAccount from,
        BankAccount to,
        decimal amount)
    {
        if (ReferenceEquals(from, to))
            throw new ArgumentException(
                "Нельзя переводить деньги на тот же счёт."
            );

        BankAccount first = from.Id < to.Id ? from : to;
        BankAccount second = from.Id < to.Id ? to : from;

        lock (first.SyncRoot)
        {
            lock (second.SyncRoot)
            {
                if (from.GetBalanceUnsafe() < amount)
                    throw new InvalidOperationException(
                        "Недостаточно средств."
                    );

                from.SetBalanceUnsafe(
                    from.GetBalanceUnsafe() - amount
                );

                to.SetBalanceUnsafe(
                    to.GetBalanceUnsafe() + amount
                );
            }
        }
    }
}
