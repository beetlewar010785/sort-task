namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeIndexFactory : IIndexFactory<MemoryBTreeIndex, MemoryBTreeRow>
{
    public MemoryBTreeIndex CreateIndexFromRow(MemoryBTreeRow row)
    {
        return new MemoryBTreeIndex(row);
    }
}