namespace ExporterStrategies;

public sealed class ReportService
{
    private readonly IReportExporter _exporter;
    public ReportService(IReportExporter exporter)
    {
        _exporter = exporter ?? throw new ArgumentNullException(nameof(exporter));
    }
    public void PublishDailySummary(int totalJobs, int failedJobs)
    {
        var body = $"Всего задач: {totalJobs}\nОшибок: {failedJobs}";
        _exporter.Export(new ReportDocument("Сводка за день", body));
    }
}