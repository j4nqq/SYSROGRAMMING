using System.Text;
using ExporterStrategies;

Console.OutputEncoding = Encoding.UTF8;

var mode = args.Length > 0 ? args[0] : "console";
IReportExporter exporter;
if (mode.Equals("console", StringComparison.OrdinalIgnoreCase))
{
    exporter = new LegacyReportAdapter(new LegacyReportPrinter());
}
else if (mode.Equals("file", StringComparison.OrdinalIgnoreCase))
{
    exporter = new TextFileReportExporter(Path.Combine(Path.GetTempPath(), "daily-summary.txt"));
}
else
{
    Console.Error.WriteLine($"Неизвестный режим '{mode}'. Допустимо: console, file.");
    return 1;
}

var service = new ReportService(exporter);
try
{
    service.PublishDailySummary(totalJobs: 42, failedJobs: 2);
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
{
    Console.Error.WriteLine($"Не удалось экспортировать отчёт: {ex.Message}");
    return 2;
}
Console.WriteLine($"Использован экспортёр: {exporter.GetType().Name}");
return 0;