namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeIndex(RowIndexKey key) : IBTreeIndex
{
    public RowIndexKey Key { get; } = key;

    public override string ToString()
    {
        return Key.ToString();
    }
}