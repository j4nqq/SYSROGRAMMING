namespace ExporterStrategies;
var mode = args.Length > 0 ? args[0] : "console";
IReportExporter exporter = mode.Equals("file", StringComparison.OrdinalIgnoreCase)
    ? new TextFileReportExporter(Path.Combine(Path.GetTempPath(), "daily-summary.txt"))
    : new LegacyReportAdapter(new LegacyReportPrinter());
var service = new ReportService(exporter);
service.PublishDailySummary(totalJobs: 42, failedJobs: 2);
Console.WriteLine($"Использован экспортёр: {exporter.GetType().Name}");
