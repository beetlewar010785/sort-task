namespace SortTask.Domain.BTree.Memory;

public record MemoryBTreeNode(
    MemoryBTreeNodeId Id,
    MemoryBTreeNodeId? ParentId,
    IReadOnlyList<MemoryBTreeNodeId> Children,
    IReadOnlyList<MemoryBTreeIndex> Indices
) : IBTreeNode<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>
{
    public override string ToString()
    {
        return Id.ToString();
    }
}