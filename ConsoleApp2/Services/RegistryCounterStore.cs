using System.Runtime.Versioning;
using Microsoft.Win32;
using ConsoleApp2.Models;

namespace ConsoleApp2.Services;

[SupportedOSPlatform("windows")]
public static class RegistryCounterStore
{
    public const string KeyPath = @"Software\ConsoleApp2\LogMonitor";

    public static void Save(LogStatistics statistics)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(KeyPath);

        key.SetValue("Info", statistics.Get(LogLevel.Info), RegistryValueKind.DWord);
        key.SetValue("Warning", statistics.Get(LogLevel.Warning), RegistryValueKind.DWord);
        key.SetValue("Error", statistics.Get(LogLevel.Error), RegistryValueKind.DWord);
        key.SetValue("Unknown", statistics.Get(LogLevel.Unknown), RegistryValueKind.DWord);
        key.SetValue("Skipped", statistics.Skipped, RegistryValueKind.DWord);
    }
}