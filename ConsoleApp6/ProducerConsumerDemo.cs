using System.Collections.Concurrent;

namespace SynchronizationPrimitives;

public class ProducerConsumerDemo : IDisposable
{
    private const int ProducersCount = 4;
    private const int ItemsPerProducer = 10;
    private const int ProcessingDelayMs = 1000;

    private readonly ConcurrentQueue<int> _queue = new();

    private readonly AutoResetEvent _signal = new(false);

    private int _processedCount;

    public async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("=== PRODUCER-CONSUMER DEMO ===");

        Task consumer = Task.Run(Consume);

        Task[] producers = Enumerable.Range(1, ProducersCount)
            .Select(id => Task.Run(() => Produce(id, ItemsPerProducer)))
            .ToArray();

        await Task.WhenAll(producers);

        _queue.Enqueue(-1);

        _signal.Set();

        await consumer;

        Console.WriteLine();
        Console.WriteLine($"Обработано элементов: {_processedCount}");
    }

    private void Produce(int producerId, int count)
    {
        for (int i = 1; i <= count; i++)
        {
            int value = producerId * 100 + i;

            _queue.Enqueue(value);

            Console.WriteLine(
                $"Производитель {producerId}: создал {value}");

            _signal.Set();

            Thread.Sleep(Random.Shared.Next(100, 400));
        }

        Console.WriteLine(
            $"Производитель {producerId}: завершил работу");
    }

    private void Consume()
    {
        while (true)
        {
            _signal.WaitOne();

            while (_queue.TryDequeue(out int value))
            {
                if (value == -1)
                {
                    Console.WriteLine("Потребитель завершает работу.");
                    return;
                }

                if (value > 250)
                    Console.WriteLine("Высокоприоритетная задача");

                Console.WriteLine(
                    $"Потребитель обработал: {value}");

                Interlocked.Increment(ref _processedCount);

                Thread.Sleep(ProcessingDelayMs);
            }
        }
    }

    public void Dispose()
    {
        _signal.Dispose();
    }
}
