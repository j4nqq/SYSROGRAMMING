namespace ConsoleApp2.Services;

public sealed class LogFileWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private bool _disposed;

    public event EventHandler<string>? FileChanged;
    public event EventHandler<Exception>? WatcherError;

    public LogFileWatcher(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath)
            ?? throw new ArgumentException("Не удалось определить папку журнала.", nameof(filePath));

        _watcher = new FileSystemWatcher(directory, Path.GetFileName(filePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Changed += OnChanged;
        _watcher.Error += OnError;
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        if (!_disposed)
        {
            _watcher.EnableRaisingEvents = false;
        }
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        FileChanged?.Invoke(this, e.FullPath);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        WatcherError?.Invoke(this, e.GetException());
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Stop();
        _disposed = true;
        _watcher.Changed -= OnChanged;
        _watcher.Error -= OnError;
        _watcher.Dispose();
    }
}