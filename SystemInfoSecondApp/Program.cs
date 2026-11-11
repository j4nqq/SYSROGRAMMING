using SystemInfoLibrary;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var systemInfo = new SystemInfo();

Console.WriteLine("Второе приложение");
Console.WriteLine("------------------");

Console.WriteLine(
    $"Название: {systemInfo.GetApplicationName()}");

Console.WriteLine(
    $"Версия библиотеки: {systemInfo.GetLibraryVersion()}");

Console.WriteLine(
    $"Среда выполнения: {systemInfo.GetRuntimeDescription()}");

Console.WriteLine(
    $"Имя компьютера: {systemInfo.GetComputerName()}");

Console.WriteLine();
Console.WriteLine("Изменения:");
Console.WriteLine($"- ссылка на библиотеку подключена: {typeof(SystemInfo).Assembly.GetName().Name}");
