namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeIndex(Row row) : IIndex
{
    public Row Row { get; } = row;

    public override string ToString()
    {
        return Row.ToString();
    }
}