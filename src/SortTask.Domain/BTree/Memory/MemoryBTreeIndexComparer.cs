namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeIndexComparer(IComparer<IRow> rowComparer) : IBTreeIndexComparer<MemoryBTreeIndex>
{
    public Task<int> Compare(MemoryBTreeIndex? x, MemoryBTreeIndex? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        return Task.FromResult(rowComparer.Compare(x.BTreeRow, y.BTreeRow));
    }
}