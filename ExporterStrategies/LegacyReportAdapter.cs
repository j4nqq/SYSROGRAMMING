namespace ExporterStrategies;

public sealed class LegacyReportAdapter : IReportExporter
{
    private readonly LegacyReportPrinter _printer;
    public LegacyReportAdapter(LegacyReportPrinter printer)
        => _printer = printer ?? throw new ArgumentNullException(nameof(printer));
    public void Export(ReportDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Body);
        _printer.PrintReport(document.Title, document.Body);
    }
}