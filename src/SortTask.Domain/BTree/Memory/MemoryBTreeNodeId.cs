namespace SortTask.Domain.BTree.Memory;

public record MemoryBTreeNodeId(Guid Id)
{
    public static MemoryBTreeNodeId New() => new(Guid.NewGuid());
}