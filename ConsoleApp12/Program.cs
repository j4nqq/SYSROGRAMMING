using ConsoleApp12.Models;
using ConsoleApp12.Services;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var service = new RegistrySettingsService();

try
{
    if (args.Length > 0 && args[0] == "--reset")
    {
        service.Reset();

        Console.WriteLine("Учебные настройки сброшены.");
        return;
    }

    AppSettings settings = service.Load();

    Console.WriteLine("Настройки приложения");
    Console.WriteLine("--------------------");
    Console.WriteLine($"Тема: {settings.Theme}");
    Console.WriteLine($"Запусков: {settings.LaunchCount}");
    Console.WriteLine($"Версия схемы: {settings.SchemaVersion}");

    Console.WriteLine();
    Console.WriteLine("Выберите тему:");
    Console.WriteLine("1 - Light");
    Console.WriteLine("2 - Dark");
    Console.WriteLine("Enter - оставить текущую");

    string? choice = Console.ReadLine();

    if (choice == "1")
        settings.Theme = "Light";
    else if (choice == "2")
        settings.Theme = "Dark";

    settings.LaunchCount = checked(settings.LaunchCount + 1);

    service.Save(settings);

    Console.WriteLine();
    Console.WriteLine("Настройки сохранены.");
    Console.WriteLine($"Тема: {settings.Theme}");
    Console.WriteLine($"Запуск № {settings.LaunchCount}");
}
catch (UnauthorizedAccessException ex)
{
    Console.WriteLine($"Нет доступа к реестру: {ex.Message}");
}
catch (IOException ex)
{
    Console.WriteLine($"Ошибка работы с реестром: {ex.Message}");
}
catch (OverflowException ex)
{
    Console.WriteLine($"Счётчик запусков переполнен: {ex.Message}");
}
catch (InvalidDataException ex)
{
    Console.WriteLine($"Некорректные настройки: {ex.Message}");
}
catch (ArgumentException ex)
{
    Console.WriteLine($"Некорректные данные: {ex.Message}");
}
