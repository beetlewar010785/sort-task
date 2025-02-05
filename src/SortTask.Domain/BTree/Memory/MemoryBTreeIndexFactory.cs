namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeIndexFactory : IBTreeIndexFactory<MemoryBTreeIndex>
{
    public MemoryBTreeIndex CreateIndex(RowIndexKey key)
    {
        return new MemoryBTreeIndex(key);
    }
}