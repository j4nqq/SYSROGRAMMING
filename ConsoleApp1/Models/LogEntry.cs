namespace ConsoleApp1.Models;

public sealed record LogEntry(
    DateTime Timestamp,
    LogLevel Level,
    string Message);