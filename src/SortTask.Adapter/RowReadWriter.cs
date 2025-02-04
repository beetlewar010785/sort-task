using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class RowReadWriter(Stream stream, Encoding encoding) : IRowReadWriter
{
    private readonly StreamWriter _streamWriter = new(stream, encoding, leaveOpen: true);

    public Task Write(Row row)
    {
        return _streamWriter.WriteLineAsync($"{row.Number}. {row.Sentence}");
    }

    public Task Flush(CancellationToken cancellationToken)
    {
        return _streamWriter.FlushAsync(cancellationToken);
    }
}