using SystemInfoLibrary;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var systemInfo = new SystemInfo();

Console.WriteLine("Третье приложение");
Console.WriteLine("-----------------");

Console.WriteLine(
    $"Название: {systemInfo.GetApplicationName()}");

Console.WriteLine(
    $"Версия библиотеки: {systemInfo.GetLibraryVersion()}");

Console.WriteLine(
    $"Среда выполнения: {systemInfo.GetRuntimeDescription()}");

Console.WriteLine(
    $"Имя компьютера: {systemInfo.GetComputerName()}");
