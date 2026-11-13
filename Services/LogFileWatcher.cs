namespace ConsoleApp2.Services;

public sealed class LogFileWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;

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
        _watcher.EnableRaisingEvents = true;
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
        _watcher.Changed -= OnChanged;
        _watcher.Error -= OnError;
        _watcher.Dispose();
    }
}