using SortTask.Domain.BTree;

namespace SortTask.Adapter.MemoryBTree;

public class MemoryBTreeNodeFactory
    : IBTreeNodeFactory<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>
{
    public MemoryBTreeNode Create(
        MemoryBTreeNodeId id,
        MemoryBTreeNodeId? parentId,
        IReadOnlyList<MemoryBTreeNodeId> children,
        IReadOnlyList<MemoryBTreeIndex> indices
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