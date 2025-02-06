namespace SortTask.Domain.BTree.Memory;

public record MemoryBTreeNode(
    MemoryBTreeNodeId Id,
    MemoryBTreeNodeId? ParentId,
    BTreeNodeCollection<MemoryBTreeNodeId> Children,
    BTreeIndexCollection<MemoryBTreeIndex> Indices
) : IBTreeNode<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>
{
    public override string ToString()
    {
        return Id.ToString();
    }
}