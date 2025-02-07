namespace SortTask.Domain.BTree.Memory;

public readonly struct MemoryBTreeIndex(ReadRow row) : IIndex
{
    public ReadRow Row { get; } = row;

    public override string ToString()
    {
        return Row.ToString();
    }
}