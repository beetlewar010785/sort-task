using SortTask.Domain;

namespace SortTask.Adapter.MemoryBTree;

public class MemoryBTreeIndexFactory : IIndexFactory<MemoryBTreeIndex>
{
    public MemoryBTreeIndex CreateIndexFromRow(ReadRow row) => new(row);
}