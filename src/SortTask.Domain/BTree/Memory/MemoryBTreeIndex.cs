namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeIndex(MemoryBTreeRow bTreeRow) : IIndex
{
    public MemoryBTreeRow BTreeRow { get; } = bTreeRow;

    public override string ToString()
    {
        return BTreeRow.ToString();
    }
}