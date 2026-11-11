namespace ConsoleApp11.Services;

public static class FileReaderWithoutUsing
{
    public static string Read(string filePath)
    {
        // Освобождения требуют оба объекта: FileStream (держит файл в ОС)
        // и StreamReader (оборачивает поток и тоже реализует IDisposable).
        // Владелец FileStream - этот метод: он его создал, значит он и должен закрыть.
        // GC следит только за управляемой памятью и срабатывает когда захочет,
        // поэтому файл может оставаться открытым неопределённо долго.
        var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read);

        var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}
