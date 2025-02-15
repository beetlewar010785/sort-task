namespace SortTask.Adapter;

public static class StreamExtensions
{
    public static void ReadAll(
        this Stream stream,
        Span<byte> buffer)
    {
        var totalRead = 0;

        while (totalRead < buffer.Length)
        {
            var bytesRead = stream.Read(buffer[totalRead..]);

            if (bytesRead == 0) throw new EndOfStreamException("Unexpected end of stream.");

            totalRead += bytesRead;
        }
    }
}
