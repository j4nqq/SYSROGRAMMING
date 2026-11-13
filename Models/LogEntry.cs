namespace ConsoleApp2.Models;

public sealed record LogEntry(
    DateTime Timestamp,
    LogLevel Level,
    string Message);