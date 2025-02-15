using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowStore(Stream stream, Encoding encoding) : IRowWriter, IRowLookup, IRowIterator
{
    private const string RowFieldsSplitter = ". ";

    private byte[] _buf = [];

    public IEnumerable<RowIteration> IterateOverRows()
    {
        using var reader = new BufferedStreamReader(stream, encoding);
        while (reader.ReadLine() is { } result)
        {
            var row = DeserializeRow(result.Line);
            yield return new RowIteration(row, result.Offset, result.Length);
        }
    }

    public Row FindRow(long offset, int length)
    {
        if (_buf.Length < length) _buf = new byte[length];

        stream.Position = offset;
        stream.ReadAll(_buf.AsSpan(0, length));
        var rowString = encoding.GetString(_buf.AsSpan(0, length));
        return DeserializeRow(rowString);
    }

    public void Write(Row row)
    {
        var bytes = encoding.GetBytes(SerializeRow(row));
        stream.Write(bytes);
    }

    public void Flush()
    {
        stream.FlushAsync();
    }

    private static string SerializeRow(Row row)
    {
        return $"{row.Number}{RowFieldsSplitter}{row.Sentence}\n";
    }

    private static Row DeserializeRow(string rowString)
    {
        var splitterIndex = rowString.IndexOf(RowFieldsSplitter, StringComparison.Ordinal);
        return splitterIndex < 0
            ? throw new InvalidOperationException($"Invalid row format {rowString}")
            : new Row(
                int.Parse(rowString[..splitterIndex], CultureInfo.InvariantCulture),
                rowString[(splitterIndex + RowFieldsSplitter.Length)..]);
    }
}
