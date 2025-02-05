namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeIndexFactory : IBTreeIndexFactory<MemoryBTreeIndex, MemoryBTreeRow>
{
    public MemoryBTreeIndex CreateIndexFromRow(MemoryBTreeRow row)
    {
        return new MemoryBTreeIndex(row);
    }
}