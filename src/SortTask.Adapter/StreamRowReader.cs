using System.Runtime.CompilerServices;
using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowReader(StreamReader streamReader) : IRowReader
{
    // public async IAsyncEnumerable<ReadRow> ReadAsAsyncEnumerable(
    //     [EnumeratorCancellation] CancellationToken cancellationToken)
    // {
    //     var position = 0L;
    //     while (!streamReader.EndOfStream)
    //     {
    //         var row = await streamReader.DeserializeRow(position, cancellationToken);
    //         yield return row;
    //         position = row.Position;
    //     }
    // }

    public async IAsyncEnumerable<ReadRow> ReadAsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(streamReader.BaseStream, Encoding.UTF8, leaveOpen: true);
        reader.BaseStream.Position = 0;
        var position = streamReader.BaseStream.Position;
        while (await reader.ReadLineAsync(cancellationToken) is { } rowString)
        {
            var row = ParseRow(rowString, position);
            position = reader.BaseStream.Position;
            yield return row;
        }
    }

    public static ReadRow ParseRow(string serializedRow, long position)
    {
        var splitterIndex = serializedRow.IndexOf(AdapterConst.RowFieldsSplitter, StringComparison.Ordinal);
        return new ReadRow(
            int.Parse(serializedRow[..splitterIndex]),
            serializedRow[(splitterIndex + AdapterConst.RowFieldsSplitter.Length)..],
            position
        );
    }
}