using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.MemoryBTree;

public class MemoryBTreeIndexComparer(IComparer<ReadRow> rowComparer)
    : IBTreeIndexComparer<MemoryBTreeIndex>
{
    public Task<int> Compare(MemoryBTreeIndex x, MemoryBTreeIndex y)
    {
        return Task.FromResult(rowComparer.Compare(x.Row, y.Row));
    }
}