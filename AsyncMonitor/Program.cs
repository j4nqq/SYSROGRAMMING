using AsyncMonitor;
var service = new CheckService();
var checks = new (string Name, TimeSpan Delay)[]
{
    ("Сеть", TimeSpan.FromMilliseconds(700)),
    ("База данных", TimeSpan.FromMilliseconds(900)),
    ("Файлы", TimeSpan.FromMilliseconds(500))
};
Task<CheckResult>? longCheck = null;
CancellationTokenSource? longCts = null;
while (true)
{
    Console.WriteLine("\n1 — одна проверка; 2 — последовательно; 3 — параллельно; 4 — запустить длительную; 5 — статус; 6 — отмена; 0 — выход");
    var cmd = Console.ReadLine();
    if (cmd == "0")
        break;
    try
    {
        if (cmd == "1")
        {
            var r = await service.CheckServiceAsync(checks[0].Name, checks[0].Delay);
            Print(new[] { r });
        }
        else if (cmd == "2")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunSequentialAsync(checks);
            sw.Stop();
            Console.WriteLine($"Последовательно: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "3")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunParallelAsync(checks);
            sw.Stop();
            Console.WriteLine($"Параллельно: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "4")
        {
            if (longCheck != null && !longCheck.IsCompleted)
                Console.WriteLine("Длительная проверка уже выполняется.");
            else
            {
                longCts = new CancellationTokenSource();
                longCheck = service.CheckServiceAsync("Длительная проверка", TimeSpan.FromSeconds(15), longCts.Token);
                Console.WriteLine("Длительная проверка запущена, меню не заблокировано.");
            }
        }
        else if (cmd == "5")
        {
            if (longCheck == null)
                Console.WriteLine("Длительная проверка не запускалась.");
            else
            {
                Console.WriteLine($"Статус: {longCheck.Status}");
                if (longCheck.IsCompleted)
                    Print(new[] { await longCheck });
            }
        }
        else if (cmd == "6")
        {
            if (longCts == null || longCheck == null || longCheck.IsCompleted)
                Console.WriteLine("Нет выполняющейся проверки.");
            else
            {
                longCts.Cancel();
                await longCheck;
            }
        }
        else
            Console.WriteLine("Неизвестная команда.");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Проверка отменена.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка {ex.GetType().Name}: {ex.Message}");
    }
}
static void Print(IEnumerable<CheckResult> xs)
{

}
