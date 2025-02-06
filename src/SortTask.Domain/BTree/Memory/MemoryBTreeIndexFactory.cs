namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeIndexFactory : IIndexFactory<MemoryBTreeIndex>
{
    public MemoryBTreeIndex CreateIndexFromRow(Row row) => new(row);
}