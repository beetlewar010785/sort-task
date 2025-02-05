using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowReadWriter(Stream stream, Encoding encoding) : IRowReadWriter
{
    private const string RowFieldsSplitter = ". ";

    private readonly StreamWriter _streamWriter = new(stream, encoding, leaveOpen: true);

    public async IAsyncEnumerable<Row> ReadAsAsyncEnumerable()
    {
        using var reader = new StreamReader(stream);
        while (await reader.ReadLineAsync() is { } rowString)
        {
            var row = DeserializeRow(rowString);
            yield return row;
        }
    }

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
        return $"{row.Number}{RowFieldsSplitter}{row.Sentence}";
    }

    private static Row DeserializeRow(string serializedRow)
    {
        var splitterIndex = serializedRow.IndexOf(RowFieldsSplitter, StringComparison.Ordinal);
        return new Row(
            int.Parse(serializedRow[..splitterIndex]),
            serializedRow[(splitterIndex + RowFieldsSplitter.Length)..]
        );
    }
}