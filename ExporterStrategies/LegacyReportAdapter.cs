namespace ExporterStrategies;
public sealed class LegacyReportAdapter : IReportExporter
{
    private readonly LegacyReportPrinter _printer;
    public LegacyReportAdapter(LegacyReportPrinter printer) => _printer = printer;
    public void Export(ReportDocument document)
    {
        // TODO 3: Согласуйте проверку входных данных с контрактом второго экспортёра.
        _printer.PrintReport(document.Title, document.Body);
    }
}
