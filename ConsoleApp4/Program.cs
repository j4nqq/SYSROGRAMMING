using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Text;

public class Program
{
    public static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        ProcessStartInfo startInfo = new()
        {
            FileName = "dotnet",
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("--info");
        startInfo.WorkingDirectory = Environment.CurrentDirectory;

        using Process process = new()
        {
            StartInfo = startInfo
        };

        Stopwatch stopwatch = new();

        try
        {
            stopwatch.Start();
            process.Start();
            Console.WriteLine($"PID: {process.Id}");
            await process.WaitForExitAsync();
            stopwatch.Stop();
            Console.WriteLine($"Длительность: {stopwatch.Elapsed}");
            Console.WriteLine($"Exit code: {process.ExitCode}");
        }
        catch (Win32Exception ex)
        {
            stopwatch.Stop();
            Console.WriteLine($"Ошибка запуска: {ex.Message}");
        }
    }
}
//Доп. вопрос: Start() не ждёт завершения процесса, поэтому без WaitForExitAsync() код завершения ещё не существует: ExitCode выбросит InvalidOperationException (или прочитается случайно, если процесс успел закончиться), а длительность покажет только время запуска.