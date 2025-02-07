using SortTask.Domain;

namespace SortTask.Adapter.MemoryBTree;

public class MemoryBTreeRowLookup : IRowLookup<MemoryBTreeIndex>
{
    public Task<ReadRow> FindRow(MemoryBTreeIndex index)
    {
        return Task.FromResult(index.Row);
    }
}