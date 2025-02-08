namespace SortTask.Adapter;

public static class StreamExtensions
{
    public static async Task ReadExactAsync(
        this Stream stream,
        Memory<byte> buffer,
        CancellationToken cancellationToken)
    {
        var totalRead = 0;

        while (totalRead < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(buffer[totalRead..], cancellationToken);

            if (bytesRead == 0)
            {
                throw new EndOfStreamException("Unexpected end of stream.");
            }

            totalRead += bytesRead;
        }
    }
}