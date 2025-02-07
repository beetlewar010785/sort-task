namespace SortTask.Domain.BTree.Memory;

public record MemoryBTreeNodeId(string Id)
{
    private static int _i;

    public static MemoryBTreeNodeId New()
    {
        _i++;
        return new MemoryBTreeNodeId(_i.ToString());
    }

    public override string ToString()
    {
        return Id;
    }
}