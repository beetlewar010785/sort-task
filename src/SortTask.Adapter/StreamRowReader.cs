using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowReader(Stream stream, Encoding encoding) : IRowReader<ReadRow>
{
    private readonly StreamReader _streamReader = new StreamReader(stream, encoding, leaveOpen: true);

    public async IAsyncEnumerable<ReadRow> ReadAsAsyncEnumerable()
    {
        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync() is { } rowString)
        {
            var row = DeserializeRow(rowString);
            yield return row;
        }
    }

    public Task Flush(CancellationToken cancellationToken)
    {
        return _streamRowWriter.FlushAsync(cancellationToken);
    }

    private static string SerializeRow(WriteRow row)
    {
        return $"{row.Number}{RowFieldsSplitter}{row.Sentence}";
    }

    private static ReadRow DeserializeRow(string serializedRow)
    {
        var splitterIndex = serializedRow.IndexOf(RowFieldsSplitter, StringComparison.Ordinal);
        return new ReadRow(
            int.Parse(serializedRow[..splitterIndex]),
            serializedRow[(splitterIndex + RowFieldsSplitter.Length)..]
        );
    }
}