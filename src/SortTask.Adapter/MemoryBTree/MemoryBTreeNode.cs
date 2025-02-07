using SortTask.Domain.BTree;

namespace SortTask.Adapter.MemoryBTree;

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