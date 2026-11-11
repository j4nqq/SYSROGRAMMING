namespace ConsoleApp11.Services;

public static class StreamConsumer
{
    public static string ReadAll(Stream stream)
    {
        stream.Position = 0;

        using var reader = new StreamReader(
            stream,
            leaveOpen: true);

        return reader.ReadToEnd();
    }
}
