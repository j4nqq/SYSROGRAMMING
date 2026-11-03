namespace AsyncFileScanner.Services;

public sealed class FileProbe
{
    public async Task<int> ReadHeaderAsync(
        string path,
        CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024];

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: buffer.Length,
            useAsync: true);

        return await stream.ReadAsync(
            buffer.AsMemory(),
            cancellationToken);
    }
}
