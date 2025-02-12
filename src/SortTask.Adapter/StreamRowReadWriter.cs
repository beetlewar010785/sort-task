using System.Runtime.CompilerServices;
using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowStore(Stream stream, Encoding encoding) : IRowWriter, IRowLookup, IRowIterator
{
    private byte[] _buf = [];

    public async Task Write(Row row, CancellationToken cancellationToken)
    {
        var bytes = encoding.GetBytes(SerializeRow(row));
        await stream.WriteAsync(bytes, cancellationToken);
    }

    public Task Flush(CancellationToken cancellationToken)
    {
        return stream.FlushAsync(cancellationToken);
    }

    public async Task<Row> FindRow(long offset, int length, CancellationToken cancellationToken)
    {
        if (_buf.Length < length)
        {
            _buf = new byte[length];
        }

        stream.Position = offset;
        await stream.ReadExactAsync(_buf.AsMemory(0, length), cancellationToken);
        var rowString = encoding.GetString(_buf.AsSpan(0, length));
        return DeserializeRow(rowString);
    }

    public async IAsyncEnumerable<RowIteration> ReadAsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new BufferedStreamReader(stream, encoding);
        while (await reader.ReadLine(cancellationToken) is { } result)
        {
            var row = DeserializeRow(result.Line);
            yield return new RowIteration(row, result.Offset, result.Length);
        }
    }

    private static string SerializeRow(Row row)
    {
        return $"{row.Number}{AdapterConst.RowFieldsSplitter}{row.Sentence}\n";
    }

    private static Row DeserializeRow(string rowString)
    {
        var splitterIndex = rowString.IndexOf(AdapterConst.RowFieldsSplitter, StringComparison.Ordinal);
        if (splitterIndex < 0)
        {
            throw new InvalidOperationException($"Invalid row format {rowString}");
        }

        return new Row(
            int.Parse(rowString[..splitterIndex]),
            rowString[(splitterIndex + AdapterConst.RowFieldsSplitter.Length)..]);
    }
}