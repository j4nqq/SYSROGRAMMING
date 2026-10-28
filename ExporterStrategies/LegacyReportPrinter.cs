namespace ExporterStrategies;
// Не изменять: имитация старой внешней библиотеки.
public sealed class LegacyReportPrinter
{
    public void PrintReport(string reportTitle, string reportText)
    {
        Console.WriteLine($"=== {reportTitle} ===");
        Console.WriteLine(reportText);
        Console.WriteLine("=== END OF REPORT ===");
    }
}
