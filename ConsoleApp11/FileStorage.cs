namespace ConsoleApp11.Services;

public sealed class FileStorage : IDisposable
{
    private readonly FileStream _stream;

    private bool _disposed;

    public FileStorage(string path)
    {
        _stream = new FileStream(
            path,
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None);
    }

    public void Write(string text)
    {
        ThrowIfDisposed();

        byte[] data = System.Text.Encoding.UTF8.GetBytes(text);

        _stream.Write(data);
        _stream.Flush();
    }

    public string Read()
    {
        ThrowIfDisposed();

        _stream.Position = 0;

        using var reader = new StreamReader(
            _stream,
            System.Text.Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024,
            leaveOpen: true);

        return reader.ReadToEnd();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _stream.Dispose();

        _disposed = true;

        Console.WriteLine("FileStorage: ресурс освобождён.");
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);
    }
}
