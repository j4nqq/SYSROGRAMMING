namespace ExporterStrategies;
public sealed class TextFileReportExporter : IReportExporter
{
    private readonly string _path;
    public TextFileReportExporter(string path)
    {
        // TODO 4: Проверьте путь; пустая строка не должна считаться допустимым именем файла.
        _path = path;
    }
    public void Export(ReportDocument document)
    {
        // TODO 5: Проверьте документ.
        // TODO 6: Запишите отчёт в согласованном формате.
        // Ошибки записи не проглатывать.
        File.WriteAllText(_path, $"{document.Title}{Environment.NewLine}{document.Body}");
    }
}
