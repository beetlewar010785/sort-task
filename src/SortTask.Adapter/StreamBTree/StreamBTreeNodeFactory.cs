using SortTask.Domain.BTree;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeNodeFactory
    : IBTreeNodeFactory<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>
{
    public StreamBTreeNode Create(
        StreamBTreeNodeId id,
        StreamBTreeNodeId? parentId,
        IReadOnlyList<StreamBTreeNodeId> children,
        IReadOnlyList<StreamBTreeIndex> indices
    )
    {
        return new StreamBTreeNode(
            id,
            parentId,
            children,
            indices
        );
    }
}