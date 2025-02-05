namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeNodeFactory
    : IBTreeNodeFactory<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>
{
    public MemoryBTreeNode Create(
        MemoryBTreeNodeId id,
        MemoryBTreeNodeId? parentId,
        BTreeNodeCollection<MemoryBTreeNodeId> children,
        IEnumerable<MemoryBTreeIndex> indexes
    )
    {
        return new MemoryBTreeNode(
            id,
            parentId,
            children,
            indexes.ToList()
        );
    }
}