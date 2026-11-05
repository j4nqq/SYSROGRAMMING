namespace SynchronizationPrimitives;

public class InterlockedDemo
{
    public void Run()
    {
        Console.WriteLine("=== INTERLOCKED DEMO ===");

        int unsafeCounter = 0;
        int safeCounter = 0;

        Parallel.For(0, 100_000, _ =>
        {
            unsafeCounter++;
            Interlocked.Increment(ref safeCounter);
        });

        Console.WriteLine($"Обычный counter++: {unsafeCounter}");
        Console.WriteLine($"Interlocked.Increment: {safeCounter}");

        int value = 10;
        int oldValue = Interlocked.CompareExchange(ref value, 20, 10);
        Console.WriteLine($"CompareExchange(10 -> 20): value = {value}, oldValue = {oldValue}");

        oldValue = Interlocked.CompareExchange(ref value, 30, 10);
        Console.WriteLine($"CompareExchange(10 -> 30): value = {value}, oldValue = {oldValue}");
    }
}
