namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeRowLookup : IRowLookup<MemoryBTreeIndex>
{
    public Task<Row> FindRow(MemoryBTreeIndex index)
    {
        return Task.FromResult(index.Row);
    }
}