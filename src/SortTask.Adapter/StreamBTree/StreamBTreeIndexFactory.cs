using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeIndexFactory : IIndexFactory<StreamBTreeIndex>
{
    public StreamBTreeIndex CreateIndexFromRow(
        Row row,
        long offset,
        long length) => new(offset, length);
}