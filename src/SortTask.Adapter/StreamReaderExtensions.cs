using SortTask.Domain;

namespace SortTask.Adapter;

public static class StreamReaderExtensions
{
    public static async Task<ReadRow> DeserializeRow(
        this StreamReader streamReader,
        CancellationToken cancellationToken)
    {
        var position = streamReader.BaseStream.Position;
        var line = await streamReader.ReadLineAsync(cancellationToken) ?? throw new Exception("End of stream");

        var splitterIndex = line.IndexOf(AdapterConst.RowFieldsSplitter, StringComparison.Ordinal);
        return new ReadRow(
            int.Parse(line[..splitterIndex]),
            line[(splitterIndex + AdapterConst.RowFieldsSplitter.Length)..],
            position
        );
    }
}