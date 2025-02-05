namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeNode(
    MemoryBTreeNodeId id,
    MemoryBTreeNodeId? parentId,
    BTreeNodeCollection<MemoryBTreeNodeId> children,
    List<MemoryBTreeIndex> indexes
) : IBTreeNode<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>
{
    public MemoryBTreeNodeId Id { get; } = id;
    public MemoryBTreeNodeId? ParentId { get; } = parentId;
    public BTreeNodeCollection<MemoryBTreeNodeId> Children => children;
    public IReadOnlyList<MemoryBTreeIndex> Indexes => indexes;

    public override string ToString()
    {
        return Id.ToString();
    }
}