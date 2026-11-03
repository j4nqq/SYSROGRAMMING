using BankRaceCondition.Models;
using BankRaceCondition.Services;

const int workers = 10;
const int operations = 1000;
const decimal amount = 10;

var simulator = new BankSimulator();
decimal expected = workers * operations * amount;

Console.WriteLine("BANK RACE CONDITION");
Console.WriteLine($"Количество задач: {workers}");
Console.WriteLine($"Операций на задачу: {operations}");
Console.WriteLine($"Сумма пополнения: {amount}");
Console.WriteLine($"Ожидаемый баланс: {expected}");

var unsafeAccount = new BankAccount();
Console.WriteLine();
Console.WriteLine("Без синхронизации");
Console.WriteLine($"Начальный баланс: {unsafeAccount.Balance}");
await simulator.RunUnsafeAsync(unsafeAccount, workers, operations, amount);
Console.WriteLine($"Фактический баланс: {unsafeAccount.Balance}");
Console.WriteLine($"Разница: {expected - unsafeAccount.Balance}");

var safeAccount = new BankAccount();
Console.WriteLine();
Console.WriteLine("С lock");
Console.WriteLine($"Начальный баланс: {safeAccount.Balance}");
await simulator.RunSafeAsync(safeAccount, workers, operations, amount);
Console.WriteLine($"Фактический баланс: {safeAccount.Balance}");
Console.WriteLine($"Разница: {expected - safeAccount.Balance}");
