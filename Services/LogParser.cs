    using System.Globalization;
using ConsoleApp2.Models;

namespace ConsoleApp2.Services;

public static class LogParser
{
    private static readonly string[] Formats =
    {
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
        "dd.MM.yyyy HH:mm:ss"
    };

    public static LogEntry? TryParse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        int firstBracket = line.IndexOf('[');
        if (firstBracket < 0)
        {
            return null;
        }

        int firstClose = line.IndexOf(']', firstBracket);
        if (firstClose < 0)
        {
            return null;
        }

        string timestampPart = line.Substring(
            firstBracket + 1,
            firstClose - firstBracket - 1);

        int secondBracket = line.IndexOf('[', firstClose);
        if (secondBracket < 0)
        {
            return null;
        }

        int secondClose = line.IndexOf(']', secondBracket);
        if (secondClose < 0)
        {
            return null;
        }

        string levelPart = line.Substring(
            secondBracket + 1,
            secondClose - secondBracket - 1);

        int messageStart = secondClose + 1;
        while (messageStart < line.Length &&
               char.IsWhiteSpace(line[messageStart]))
        {
            messageStart++;
        }

        string message = messageStart < line.Length
            ? line.Substring(messageStart)
            : string.Empty;

        if (!DateTime.TryParseExact(
                timestampPart,
                Formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out DateTime timestamp))
        {
            return null;
        }

        LogLevel level = levelPart.ToUpperInvariant() switch
        {
            "INFO" => LogLevel.Info,
            "WARNING" or "WARN" => LogLevel.Warning,
            "ERROR" or "ERR" => LogLevel.Error,
            _ => LogLevel.Unknown
        };

        return new LogEntry(timestamp, level, message);
    }
}