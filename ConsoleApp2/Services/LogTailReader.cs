using System.Text;

namespace ConsoleApp2.Services;

public sealed class LogTailReader
{
    private readonly string _filePath;
    private long _offset;

    public LogTailReader(string filePath, long startOffset)
    {
        _filePath = filePath;
        _offset = startOffset;
    }

    public List<string> ReadNewLines()
    {
        var lines = new List<string>();

        using var stream = new FileStream(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        if (stream.Length < _offset)
        {
            _offset = 0;
        }

        stream.Seek(_offset, SeekOrigin.Begin);

        using var reader = new StreamReader(stream, Encoding.UTF8);
        string text = reader.ReadToEnd();

        int lastNewLine = text.LastIndexOf('\n');
        if (lastNewLine < 0)
        {
            return lines;
        }

        string complete = text.Substring(0, lastNewLine + 1);
        _offset += Encoding.UTF8.GetByteCount(complete);

        foreach (string rawLine in complete.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (line.Length > 0)
            {
                lines.Add(line);
            }
        }

        return lines;
    }
}