using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowReader(Stream stream, Encoding encoding) : IRowReader<StreamReadRow>
{
    public async IAsyncEnumerable<StreamReadRow> ReadAsAsyncEnumerable()
    {
        using var reader = new StreamReader(stream, encoding, leaveOpen: true);
        while (await reader.ReadLineAsync() is { } rowString)
        {
            var row = DeserializeRow(rowString);
            yield return row;
        }
    }

    private static StreamReadRow DeserializeRow(string serializedRow)
    {
        var splitterIndex = serializedRow.IndexOf(Const.RowFieldsSplitter, StringComparison.Ordinal);
        return new StreamReadRow(
            int.Parse(serializedRow[..splitterIndex]),
            serializedRow[(splitterIndex + Const.RowFieldsSplitter.Length)..]
        );
    }
}