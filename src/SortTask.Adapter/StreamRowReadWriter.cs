using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Adapter;

public record struct RowWithOffset(WriteRow Row, long Offset, long Length);

public class StreamRowReadWriter(Stream stream)
{
    public async Task Write(WriteRow row, CancellationToken cancellationToken)
    {
        var bytes = AdapterConst.Encoding.GetBytes(SerializeRow(row));
        await stream.WriteAsync(bytes, cancellationToken);
    }

    public async Task<WriteRow> ReadAt(long offset, long length, CancellationToken cancellationToken)
    {
        var buf = new byte[length];
        stream.Position = offset;
        await stream.ReadExactAsync(buf, cancellationToken);
        var rowString = AdapterConst.Encoding.GetString(buf);
        return DeserializeRow(rowString);
    }

    public async IAsyncEnumerable<RowWithOffset> ReadAsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        stream.Position = 0;
        var reader = new BufferedStreamReader(stream, AdapterConst.Encoding);
        while (await reader.ReadLine(cancellationToken) is { } result)
        {
            var row = DeserializeRow(result.Line);
            yield return new RowWithOffset(row, result.Offset, result.Length);
        }
    }

    private static string SerializeRow(WriteRow row)
    {
        return $"{row.Number}{AdapterConst.RowFieldsSplitter}{row.Sentence}\n";
    }

    private static WriteRow DeserializeRow(string rowString)
    {
        var splitterIndex = rowString.IndexOf(AdapterConst.RowFieldsSplitter, StringComparison.Ordinal);
        return new WriteRow(
            int.Parse(rowString[..splitterIndex]),
            rowString[(splitterIndex + AdapterConst.RowFieldsSplitter.Length)..]);
    }
}