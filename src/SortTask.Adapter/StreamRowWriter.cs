using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowWriter(Stream stream, Encoding encoding) : IRowWriter
{
    private readonly StreamWriter _streamWriter = new(stream, encoding, leaveOpen: true);

    public Task Write(Row row)
    {
        var serializedRow = SerializeRow(row);
        return _streamWriter.WriteLineAsync(serializedRow);
    }

    public Task Flush(CancellationToken cancellationToken)
    {
        return _streamWriter.FlushAsync(cancellationToken);
    }

    private static string SerializeRow(Row row)
    {
        return $"{row.Number}{Const.RowFieldsSplitter}{row.Sentence}";
    }
}