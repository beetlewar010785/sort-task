using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowReader(Stream stream, Encoding encoding) : IRowReader
{
    public async IAsyncEnumerable<ReadRow> ReadAsAsyncEnumerable()
    {
        using var reader = new StreamReader(stream, encoding, leaveOpen: true);
        long position = 0;
        while (await reader.ReadLineAsync() is { } rowString)
        {
            var row = DeserializeRow(rowString, position);
            position = stream.Position;
            yield return row;
        }
    }

    private static ReadRow DeserializeRow(string serializedRow, long position)
    {
        var splitterIndex = serializedRow.IndexOf(Const.RowFieldsSplitter, StringComparison.Ordinal);
        return new ReadRow(
            int.Parse(serializedRow[..splitterIndex]),
            serializedRow[(splitterIndex + Const.RowFieldsSplitter.Length)..],
            position
        );
    }
}