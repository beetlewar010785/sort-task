using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowWriter(StreamWriter streamWriter) : IRowWriter
{
    public Task Write(WriteRow row)
    {
        var serializedRow = SerializeRow(row);
        return streamWriter.WriteLineAsync(serializedRow);
    }

    public Task Flush(CancellationToken cancellationToken)
    {
        return streamWriter.FlushAsync(cancellationToken);
    }

    private static string SerializeRow(WriteRow row)
    {
        return $"{row.Number}{AdapterConst.RowFieldsSplitter}{row.Sentence}";
    }
}