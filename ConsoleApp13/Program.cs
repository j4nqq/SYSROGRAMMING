using ConsoleApp13.Interop;
using SystemInfoLibrary;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Информация о приложении");
Console.WriteLine("-----------------------");

var systemInfo = new SystemInfo();

Console.WriteLine(
    $"Название: {systemInfo.GetApplicationName()}");

Console.WriteLine(
    $"Версия библиотеки: {systemInfo.GetLibraryVersion()}");

uint processId = WindowsApi.GetCurrentProcessId();

Console.WriteLine($"PID процесса: {processId}");

Console.WriteLine();
Console.WriteLine("Изменения:");
Console.WriteLine("- название приложения изменено");
Console.WriteLine("- версия библиотеки изменена на 1.1.0");
Console.WriteLine();
Console.WriteLine("Нажмите Enter для завершения.");
Console.ReadLine();
