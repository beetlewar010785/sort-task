using SortTask.Domain.BTree;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeNode(
    StreamBTreeNodeId id,
    StreamBTreeNodeId? parentId,
    IReadOnlyList<StreamBTreeNodeId> children,
    IReadOnlyList<StreamBTreeIndex> indices
) : IBTreeNode<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>
{
    public StreamBTreeNodeId Id => id;
    public StreamBTreeNodeId? ParentId => parentId;
    public IReadOnlyList<StreamBTreeIndex> Indices => indices;
    public IReadOnlyList<StreamBTreeNodeId> Children => children;

    public override string ToString()
    {
        return id.ToString();
    }
}