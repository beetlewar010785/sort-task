using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StreamRowReader(StreamReader streamReader) : IRowReader
{
    public async IAsyncEnumerable<ReadRow> ReadAsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!streamReader.EndOfStream)
        {
            yield return await streamReader.DeserializeRow(cancellationToken);
        }
    }
}