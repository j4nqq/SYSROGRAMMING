using Microsoft.Win32;
using ConsoleApp12.Models;

namespace ConsoleApp12.Services;

public sealed class RegistrySettingsService
{
    private const string RegistryPath =
        @"Software\StudentSystemProgramming";

    private const int CurrentSchemaVersion = 1;

    public AppSettings Load()
    {
        var settings = new AppSettings();

        using RegistryKey? key =
            Registry.CurrentUser.OpenSubKey(RegistryPath);

        if (key is null)
        {
            return settings;
        }

        object? schemaValue = key.GetValue("SchemaVersion");

        int storedSchemaVersion = 0;

        if (schemaValue is int version)
        {
            storedSchemaVersion = version;
        }
        else if (schemaValue is not null)
        {
            throw new InvalidDataException(
                "Некорректный тип SchemaVersion в реестре.");
        }

        if (storedSchemaVersion < 0)
        {
            throw new InvalidDataException(
                "Версия схемы не может быть отрицательной.");
        }

        if (storedSchemaVersion > CurrentSchemaVersion)
        {
            throw new InvalidDataException(
                "Настройки созданы более новой версией приложения.");
        }

        object? themeValue = key.GetValue("Theme");
        object? launchCountValue = key.GetValue("LaunchCount");

        if (themeValue is string theme &&
            (theme == "Light" || theme == "Dark"))
        {
            settings.Theme = theme;
        }

        if (launchCountValue is int launchCount &&
            launchCount >= 0)
        {
            settings.LaunchCount = launchCount;
        }

        if (storedSchemaVersion < CurrentSchemaVersion)
        {
            Save(settings);
        }

        return settings;
    }

    public void Save(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.Theme != "Light" &&
            settings.Theme != "Dark")
        {
            throw new ArgumentException(
                "Недопустимая тема оформления.");
        }

        if (settings.LaunchCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings.LaunchCount));
        }

        using RegistryKey key =
            Registry.CurrentUser.CreateSubKey(
                RegistryPath,
                writable: true);

        key.SetValue(
            "Theme",
            settings.Theme,
            RegistryValueKind.String);

        key.SetValue(
            "LaunchCount",
            settings.LaunchCount,
            RegistryValueKind.DWord);

        key.SetValue(
            "SchemaVersion",
            CurrentSchemaVersion,
            RegistryValueKind.DWord);
    }

    public void Reset()
    {
        Registry.CurrentUser.DeleteSubKeyTree(
            RegistryPath,
            throwOnMissingSubKey: false);
    }
}
