namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeNodeFactory
    : IBTreeNodeFactory<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>
{
    public MemoryBTreeNode Create(
        MemoryBTreeNodeId id,
        MemoryBTreeNodeId? parentId,
        BTreeNodeCollection<MemoryBTreeNodeId> children,
        BTreeIndexCollection<MemoryBTreeIndex> indices
    )
    {
        return new MemoryBTreeNode(
            id,
            parentId,
            children,
            indices
        );
    }
}