using System.Collections.Concurrent;

namespace SynchronizationPrimitives;

public class ProducerConsumerDemo
{
    private const int ProducersCount = 3;
    private const int ItemsPerProducer = 5;

    private readonly ConcurrentQueue<int> _queue = new();
    private readonly AutoResetEvent _dataReady = new(false);

    private int _producersLeft = ProducersCount;
    private int _consumed;

    public async Task RunAsync()
    {
        Console.WriteLine("=== PRODUCER / CONSUMER DEMO ===");

        Task consumer = Task.Run(Consume);

        Task[] producers = Enumerable.Range(1, ProducersCount)
            .Select(id => Task.Run(() => ProduceAsync(id)))
            .ToArray();

        await Task.WhenAll(producers);
        await consumer;

        _dataReady.Dispose();

        Console.WriteLine($"Обработано элементов: {_consumed} из {ProducersCount * ItemsPerProducer}");
    }

    private async Task ProduceAsync(int producerId)
    {
        try
        {
            for (int i = 1; i <= ItemsPerProducer; i++)
            {
                int item = producerId * 100 + i;

                await Task.Delay(Random.Shared.Next(100, 500));

                _queue.Enqueue(item);
                Console.WriteLine($"Производитель {producerId}: добавил {item}");

                _dataReady.Set();
            }
        }
        finally
        {
            if (Interlocked.Decrement(ref _producersLeft) == 0)
                _dataReady.Set();
        }
    }

    private void Consume()
    {
        Console.WriteLine("Потребитель: ожидает данные");

        while (true)
        {
            _dataReady.WaitOne();

            while (_queue.TryDequeue(out int item))
            {
                Console.WriteLine($"Потребитель: получил {item}");
                Interlocked.Increment(ref _consumed);
            }

            if (Volatile.Read(ref _producersLeft) == 0 && _queue.IsEmpty)
                break;
        }

        Console.WriteLine("Потребитель: все производители завершили работу");
    }
}
