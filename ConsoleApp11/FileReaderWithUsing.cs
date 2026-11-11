namespace ConsoleApp11.Services;

public static class FileReaderWithUsing
{
    public static string Read(string filePath)
    {
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read);

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    public static string ReadFileWithError(string filePath)
    {
        using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read);

        using var reader = new StreamReader(stream);

        string content = reader.ReadToEnd();

        throw new InvalidOperationException(
            "Искусственная ошибка после чтения файла.");
    }
}
