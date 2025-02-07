using SortTask.Adapter.MemoryBTree;
using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeIndexFactory : IIndexFactory<MemoryBTreeIndex>
{
    public MemoryBTreeIndex CreateIndexFromRow(ReadRow row) => new(row);
}