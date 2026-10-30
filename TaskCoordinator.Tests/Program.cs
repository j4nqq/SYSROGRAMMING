using TaskCoordinator;
var pass = 0;
var fail = 0;
void Check(bool ok, string n)
{
    if (ok)
    {
        pass++;
        Console.WriteLine("PASS " + n);
    }
    else
    {
        fail++;
        Console.WriteLine("FAIL " + n);
    }
}
var s = new DiagnosticService();
var one = await s.RunAsync("Сеть", TimeSpan.FromMilliseconds(20));
Check(one.Status == DiagnosticStatus.Completed, "одна диагностика завершается");
var all = await s.RunAllAsync();
Check(
    all.Count == 3 && all.All(x => x.Status == DiagnosticStatus.Completed),
    "WhenAll ждёт три диагностики"
);
var first = await s.FirstFinishedAsync();
Check(first == "Быстрая", "WhenAny возвращает первую завершившуюся");
var cpuCts = new CancellationTokenSource();
var cpuTask = s.RunCpuBoundAsync("CPU", int.MaxValue, cpuCts.Token);
await Task.Delay(50);
cpuCts.Cancel();
var cpuResult = await cpuTask;
Check(cpuResult.Status == DiagnosticStatus.Cancelled, "кооперативная отмена CPU-bound цикла");
var timedOut = false;
try
{
    await s.RunWithTimeoutAsync("Тест", TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(50));
}
catch (TimeoutException)
{
    timedOut = true;
}
Check(timedOut, "тайм-аут сообщается TimeoutException");
var externalCancelled = false;
using var externalCts = new CancellationTokenSource();
externalCts.CancelAfter(50);
try
{
    await s.RunWithTimeoutAsync("Тест", TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), externalCts.Token);
}
catch (TimeoutException)
{
}
catch (OperationCanceledException)
{
    externalCancelled = true;
}
Check(externalCancelled, "внешняя отмена не считается тайм-аутом");
var emptyNameThrown = false;
try
{
    await s.RunAsync("", TimeSpan.FromMilliseconds(20));
}
catch (ArgumentException)
{
    emptyNameThrown = true;
}
Check(emptyNameThrown, "пустое имя отклоняется");
var negativeThrown = false;
try
{
    await s.RunAsync("Тест", TimeSpan.FromMilliseconds(-1));
}
catch (ArgumentOutOfRangeException)
{
    negativeThrown = true;
}
Check(negativeThrown, "отрицательное время отклоняется");
var safe = await s.RunSafeAsync("", TimeSpan.FromMilliseconds(20));
Check(safe.Status == DiagnosticStatus.Failed, "ошибка возвращается статусом Failed");
Console.WriteLine($"Итого: {pass} пройдено, {fail} ошибок.");
return fail == 0 ? 0 : 1;
