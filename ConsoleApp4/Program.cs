using System;
using System.Diagnostics;

ProcessStartInfo startInfo = new()
{
    FileName = "dotnet",
    UseShellExecute = false,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    CreateNoWindow = true
};

startInfo.ArgumentList.Add("--info1");

using Process process = new()
{
    StartInfo = startInfo
};

var stopwatch = Stopwatch.StartNew();

try
{
    if (!process.Start())
    {
        Console.WriteLine("Не удалось запустить процесс.");
        return;
    }

    Console.WriteLine($"PID: {process.Id}");

    Task<string> stdoutTask =
        process.StandardOutput.ReadToEndAsync();

    Task<string> stderrTask =
        process.StandardError.ReadToEndAsync();

    await process.WaitForExitAsync();

    string stdout = await stdoutTask;
    string stderr = await stderrTask;

    stopwatch.Stop();

    Console.WriteLine($"Длительность: {stopwatch.Elapsed}");
    Console.WriteLine($"Exit code: {process.ExitCode}");

    Console.WriteLine("=== STDOUT ===");
    Console.WriteLine(stdout);

    Console.WriteLine("=== STDERR ===");
    Console.WriteLine(stderr);
}
catch (System.ComponentModel.Win32Exception ex)
{
    stopwatch.Stop();
    Console.WriteLine($"Ошибка запуска: {ex.Message}");
}