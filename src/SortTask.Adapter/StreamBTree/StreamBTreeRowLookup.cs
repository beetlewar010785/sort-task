using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeRowLookup(IRowReadWriter rowReadWriter) : IRowLookup<StreamBTreeIndex>
{
    public Task<Row> FindRow(StreamBTreeIndex index, CancellationToken cancellationToken)
    {
        return rowReadWriter.ReadAt(index.RowOffset, index.RowLength, cancellationToken);
    }
}