namespace SystemInfoLibrary;

public sealed class SystemInfo
{
    public string GetApplicationName()
    {
        return "Student System Programming Lab";
    }

    public string GetLibraryVersion()
    {
        return "1.2.0";
    }

    public string GetRuntimeDescription()
    {
        return $".NET {Environment.Version}";
    }

    public string GetComputerName()
    {
        return Environment.MachineName;
    }
}
