using System;
using System.Threading;

Thread first = new(() =>
{
    for (int i = 1; i <= 5; i++)
    {
        Console.WriteLine(
            $"Поток A: {i}, ID={Environment.CurrentManagedThreadId}"
        );
        Thread.Sleep(1000);
    }
});

Thread second = new(() =>
{
    for (int i = 1; i <= 5; i++)
    {
        Console.WriteLine(
            $"Поток B: {i}, ID={Environment.CurrentManagedThreadId}"
        );
        Thread.Sleep(1000);
    }
});

first.Start();
second.Start();

first.Join();
second.Join();

Console.WriteLine("Оба потока завершены.");