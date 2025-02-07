namespace SortTask.Domain.BTree.Memory;

public record MemoryBTreeNodeId(string Id)
{
    private static int _i = 0;

    public static MemoryBTreeNodeId New()
    {
        _i++;
        return new MemoryBTreeNodeId(_i.ToString());
    }
}