using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowReadWriter(Stream stream) : IRowReadWriter, IRowIterator
{
    public async Task Write(Row row, CancellationToken cancellationToken)
    {
        var bytes = AdapterConst.Encoding.GetBytes(SerializeRow(row));
        await stream.WriteAsync(bytes, cancellationToken);
    }

    public Task Flush(CancellationToken cancellationToken)
    {
        return stream.FlushAsync(cancellationToken);
    }

    public async Task<Row> ReadAt(long offset, long length, CancellationToken cancellationToken)
    {
        // todo improve buffer
        var buf = new byte[length];
        stream.Position = offset;
        await stream.ReadExactAsync(buf, cancellationToken);
        var rowString = AdapterConst.Encoding.GetString(buf);
        return DeserializeRow(rowString);
    }

    public async IAsyncEnumerable<RowWithOffset> ReadAsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new BufferedStreamReader(stream, AdapterConst.Encoding);
        while (await reader.ReadLine(cancellationToken) is { } result)
        {
            var row = DeserializeRow(result.Line);
            yield return new RowWithOffset(row, result.Offset, result.Length);
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