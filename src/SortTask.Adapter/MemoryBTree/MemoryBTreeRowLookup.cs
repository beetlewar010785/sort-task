using SortTask.Domain;

namespace SortTask.Adapter.MemoryBTree;

public class MemoryBTreeRowLookup : IRowLookup<MemoryBTreeIndex>
{
    public Task<ReadRow> FindRow(MemoryBTreeIndex index, CancellationToken cancellationToken)
    {
        return Task.FromResult(index.Row);
    }
}