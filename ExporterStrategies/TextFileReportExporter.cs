namespace ExporterStrategies;

public sealed class TextFileReportExporter : IReportExporter
{
    private readonly string _path;
    public TextFileReportExporter(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
    }
    public void Export(ReportDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Title);
        ArgumentException.ThrowIfNullOrWhiteSpace(document.Body);
        File.WriteAllText(_path, $"{document.Title}{Environment.NewLine}{document.Body}");
    }
}