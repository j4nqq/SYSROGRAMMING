namespace SynchronizationPrimitives;

public class SemaphoreDemo
{
    private readonly SemaphoreSlim _semaphore = new(2, 2);
    private readonly int? _failingTaskId = null;

    private int _activeTasks;
    private int _maxActiveTasks;

    public async Task RunAsync()
    {
        Console.WriteLine("=== SEMAPHORE DEMO ===");

        using CancellationTokenSource cts = new();

        Task[] tasks = Enumerable.Range(1, 12)
            .Select(id => ProcessAsync(id, cts.Token))
            .ToArray();

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Обработка отменена");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка выполнения: {ex.Message}");
        }

        Console.WriteLine($"Максимум одновременных задач: {_maxActiveTasks}");
        Console.WriteLine($"Свободных разрешений: {_semaphore.CurrentCount}");

        await ProcessAsync(99, CancellationToken.None);
    }

    private async Task ProcessAsync(int id, CancellationToken token)
    {
        Console.WriteLine($"Задача {id}: ожидает");

        bool acquired = await _semaphore.WaitAsync(TimeSpan.FromSeconds(2), token);

        if (!acquired)
        {
            Console.WriteLine($"Задача {id}: тайм-аут ожидания");
            return;
        }

        try
        {
            int active = Interlocked.Increment(ref _activeTasks);
            UpdateMaximum(active);
            Console.WriteLine($"Задача {id}: начала работу. Активно: {active}");

            await Task.Delay(Random.Shared.Next(500, 1500), token);

            if (id == _failingTaskId)
                throw new InvalidOperationException($"Ошибка задания №{id}");

            Console.WriteLine($"Задача {id}: завершена");
        }
        finally
        {
            Interlocked.Decrement(ref _activeTasks);
            _semaphore.Release();
        }
    }

    private void UpdateMaximum(int current)
    {
        int previous;

        do
        {
            previous = Volatile.Read(ref _maxActiveTasks);

            if (current <= previous)
                return;

        } while (Interlocked.CompareExchange(ref _maxActiveTasks, current, previous) != previous);
    }
}
