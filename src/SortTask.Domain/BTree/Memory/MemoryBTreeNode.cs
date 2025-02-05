namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeNode(
    MemoryBTreeNodeId id,
    MemoryBTreeNodeId? parentId,
    BTreeNodeCollection<MemoryBTreeNodeId> children,
    BTreeIndexCollection<MemoryBTreeIndex> indexes
) : IBTreeNode<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>
{
    public MemoryBTreeNodeId Id { get; } = id;
    public MemoryBTreeNodeId? ParentId { get; } = parentId;
    public BTreeNodeCollection<MemoryBTreeNodeId> Children => children;
    public BTreeIndexCollection<MemoryBTreeIndex> Indices => indexes;

    public override string ToString()
    {
        return Id.ToString();
    }
}