using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeRowLookup(StreamReader streamReader) : IRowLookup<StreamBTreeIndex>
{
    public Task<ReadRow> FindRow(StreamBTreeIndex index, CancellationToken cancellationToken)
    {
        return streamReader.DeserializeRow(index.RowPosition, cancellationToken);
    }
}