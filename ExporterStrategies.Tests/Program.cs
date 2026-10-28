using System.Text;
using ExporterStrategies;

Console.OutputEncoding = Encoding.UTF8;

var failed = new List<string>();
var total = 0;
void Check(string name, bool condition)
{
    total++;
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} — {name}");
    if (!condition) failed.Add(name);
}
bool Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return true; }
    return false;
}
string TempFile() => Path.Combine(Path.GetTempPath(), $"report-test-{Guid.NewGuid():N}.txt");
var validDoc = new ReportDocument("Заголовок", "Текст");

var path = TempFile();
try
{
    IReportExporter fileExporter = new TextFileReportExporter(path);
    var service = new ReportService(fileExporter);
    service.PublishDailySummary(10, 1);
    var contents = File.Exists(path) ? File.ReadAllText(path) : "";
    Check("файловый экспорт создаёт файл", File.Exists(path));
    Check("файл содержит заголовок", contents.Contains("Сводка за день"));
    Check("файл содержит значения отчёта", contents.Contains("Всего задач: 10") && contents.Contains("Ошибок: 1"));
}
finally { if (File.Exists(path)) File.Delete(path); }

var originalOut = Console.Out;
var writer = new StringWriter();
try
{
    Console.SetOut(writer);
    new ReportService(new LegacyReportAdapter(new LegacyReportPrinter())).PublishDailySummary(7, 3);
}
finally { Console.SetOut(originalOut); }
var consoleText = writer.ToString();
Check("адаптер печатает заголовок", consoleText.Contains("=== Сводка за день ==="));
Check("адаптер печатает значения отчёта", consoleText.Contains("Всего задач: 7") && consoleText.Contains("Ошибок: 3"));
Check("адаптер печатает завершающую строку", consoleText.Contains("=== END OF REPORT ==="));

var recorder = new RecordingExporter();
new ReportService(recorder).PublishDailySummary(5, 0);
var expected = new ReportDocument("Сводка за день", "Всего задач: 5\nОшибок: 0");
Check("сервис передаёт экспортёру корректный документ", recorder.Documents.Count == 1 && recorder.Documents[0] == expected);

var path2 = TempFile();
var swapOut = Console.Out;
try
{
    Console.SetOut(new StringWriter());
    var exporters = new IReportExporter[]
    {
        recorder,
        new TextFileReportExporter(path2),
        new LegacyReportAdapter(new LegacyReportPrinter()),
    };
    var allWork = true;
    foreach (var e in exporters)
    {
        try { new ReportService(e).PublishDailySummary(1, 1); }
        catch (Exception) { allWork = false; }
    }
    Check("один и тот же ReportService работает со всеми реализациями", allWork);
}
finally
{
    Console.SetOut(swapOut);
    if (File.Exists(path2)) File.Delete(path2);
}
Check("ReportService отклоняет null-экспортёр", Throws<ArgumentNullException>(() => new ReportService(null!)));

var emptyDoc = new ReportDocument("", "");
var path3 = TempFile();
try
{
    var fileExporter = new TextFileReportExporter(path3);
    Check("файловый экспортёр: пустой документ -> ArgumentException", Throws<ArgumentException>(() => fileExporter.Export(emptyDoc)));
    Check("файловый экспортёр: пустой документ не создаёт файл", !File.Exists(path3));
    Check("файловый экспортёр: null-документ -> ArgumentNullException", Throws<ArgumentNullException>(() => fileExporter.Export(null!)));
    Check("файловый экспортёр: пробельный заголовок -> ArgumentException", Throws<ArgumentException>(() => fileExporter.Export(new ReportDocument("   ", "текст"))));
}
finally { if (File.Exists(path3)) File.Delete(path3); }

var adapterOut = Console.Out;
var adapterWriter = new StringWriter();
try
{
    Console.SetOut(adapterWriter);
    var adapter = new LegacyReportAdapter(new LegacyReportPrinter());
    Check("адаптер: пустой документ -> ArgumentException", Throws<ArgumentException>(() => adapter.Export(emptyDoc)));
    Check("адаптер: null-документ -> ArgumentNullException", Throws<ArgumentNullException>(() => adapter.Export(null!)));
}
finally { Console.SetOut(adapterOut); }
Check("адаптер: при некорректном документе ничего не напечатано", adapterWriter.ToString().Length == 0);
Check("адаптер: null-принтер -> ArgumentNullException", Throws<ArgumentNullException>(() => new LegacyReportAdapter(null!)));

Check("пустой путь -> ArgumentException", Throws<ArgumentException>(() => new TextFileReportExporter("")));
Check("путь из пробелов -> ArgumentException", Throws<ArgumentException>(() => new TextFileReportExporter("   ")));
Check("null-путь -> ArgumentException", Throws<ArgumentException>(() => new TextFileReportExporter(null!)));

var missingDir = Path.Combine(Path.GetTempPath(), $"no-such-dir-{Guid.NewGuid():N}");
var badPath = Path.Combine(missingDir, "report.txt");
try
{
    var badExporter = new TextFileReportExporter(badPath);
    Check("ошибка записи (нет каталога) не скрывается: IOException", Throws<IOException>(() => badExporter.Export(validDoc)));
    Check("ошибка записи доходит через ReportService", Throws<IOException>(() => new ReportService(badExporter).PublishDailySummary(1, 0)));
    Check("при ошибке записи каталог не создаётся молча", !Directory.Exists(missingDir));
}
finally { if (Directory.Exists(missingDir)) Directory.Delete(missingDir, true); }

Console.WriteLine($"Провалено проверок: {failed.Count} из {total}.");
return failed.Count == 0 ? 0 : 1;

sealed class RecordingExporter : IReportExporter
{
    public List<ReportDocument> Documents { get; } = new();
    public void Export(ReportDocument document) => Documents.Add(document);
}