namespace ConsoleApp11.Services;

public static class GcGenerationsDemo
{
    public static void Run()
    {
        var first = new byte[1024];
        var second = new byte[1024 * 1024];
        var third = new byte[1024 * 1024 * 5];

        Console.WriteLine($"первое (1 КБ): Gen {GC.GetGeneration(first)}");
        Console.WriteLine($"второе (1 МБ): Gen {GC.GetGeneration(second)}");
        Console.WriteLine($"третье (5 МБ): Gen {GC.GetGeneration(third)}");

        Console.WriteLine($"Сборок Gen 0: {GC.CollectionCount(0)}");
        Console.WriteLine($"Сборок Gen 1: {GC.CollectionCount(1)}");
        Console.WriteLine($"Сборок Gen 2: {GC.CollectionCount(2)}");

        // Поколение зависит от того, сколько сборок объект пережил, а не от его размера.
        // Новые объекты начинают с Gen 0, а крупные (от ~85 КБ) просто лежат в отдельной
        // куче больших объектов, которая считается частью Gen 2.
        // Поэтому по выводу видно: 1 КБ и 1 МБ это Gen 0, а 5 МБ уже Gen 2.
        // Но Gen 2 тут из-за особенности кучи больших объектов, а не из-за того, что он "старый".
    }
}
