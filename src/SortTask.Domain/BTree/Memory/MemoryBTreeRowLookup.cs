namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeRowLookup : IRowLookup<MemoryBTreeIndex>
{
    public Task<ReadRow> FindRow(MemoryBTreeIndex index)
    {
        return Task.FromResult(index.Row);
    }
}