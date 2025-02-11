using System.Runtime.CompilerServices;
using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowReadWriter(
    Stream stream,
    Encoding encoding,
    IOph oph) : IRowWriter, IRowLookup, IRowIterator
{
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
        // todo improve buffer
        var buf = new byte[length];
        stream.Position = offset;
        await stream.ReadExactAsync(buf, cancellationToken);
        var rowString = encoding.GetString(buf);
        return DeserializeRow(rowString);
    }

    // todo extract
    public async IAsyncEnumerable<RowIteration> ReadAsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new BufferedStreamReader(stream, encoding);
        while (await reader.ReadLine(cancellationToken) is { } result)
        {
            var row = DeserializeRow(result.Line);
            var sentenceBytes = encoding.GetBytes(row.Sentence);
            var sentenceOph = oph.Hash(sentenceBytes);
            yield return new RowIteration(row, sentenceOph, result.Offset, result.Length);
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