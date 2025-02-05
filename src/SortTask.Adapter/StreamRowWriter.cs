using System.Text;
using SortTask.Domain;
using SortTask.Domain.RowGeneration;

namespace SortTask.Adapter;

public class StreamRowWriter(Stream stream, Encoding encoding) : IRowWriter<GeneratingRow>
{
    private readonly StreamWriter _streamWriter = new(stream, encoding, leaveOpen: true);

    public Task Write(GeneratingRow row)
    {
        var serializedRow = SerializeRow(row);
        return _streamWriter.WriteLineAsync(serializedRow);
    }

    public Task Flush(CancellationToken cancellationToken)
    {
        return _streamWriter.FlushAsync(cancellationToken);
    }

    private static string SerializeRow(GeneratingRow row)
    {
        return $"{row.Number}{Const.RowFieldsSplitter}{row.Sentence}";
    }
}