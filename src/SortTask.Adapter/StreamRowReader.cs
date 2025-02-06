using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowReader(Stream stream, Encoding encoding) : IRowReader
{
    public async IAsyncEnumerable<Row> ReadAsAsyncEnumerable()
    {
        using var reader = new StreamReader(stream, encoding, leaveOpen: true);
        while (await reader.ReadLineAsync() is { } rowString)
        {
            var row = DeserializeRow(rowString);
            yield return row;
        }
    }

    private static Row DeserializeRow(string serializedRow)
    {
        var splitterIndex = serializedRow.IndexOf(Const.RowFieldsSplitter, StringComparison.Ordinal);
        return new Row(
            int.Parse(serializedRow[..splitterIndex]),
            serializedRow[(splitterIndex + Const.RowFieldsSplitter.Length)..]
        );
    }
}