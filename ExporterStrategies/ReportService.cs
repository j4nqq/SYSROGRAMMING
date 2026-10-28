namespace ExporterStrategies;
public sealed class ReportService
{
    private readonly IReportExporter _exporter;
    public ReportService(IReportExporter exporter)
    {
        // TODO 1: Проверьте обязательную зависимость.
        _exporter = exporter;
    }
    public void PublishDailySummary(int totalJobs, int failedJobs)
    {
        // TODO 2: Бизнес-логика формирует документ, но не знает способ экспорта.
        var body = $"Всего задач: {totalJobs}\nОшибок: {failedJobs}";
        _exporter.Export(new ReportDocument("Сводка за день", body));
    }
}
