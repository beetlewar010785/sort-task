using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeStore(StreamBTreeNodeReadWriter bTreeNodeReadWriter)
    : IBTreeStore<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>, IInitializer
{
    public Task Initialize(CancellationToken cancellationToken)
    {
        return bTreeNodeReadWriter.WriteHeader(new StreamBTreeHeader(0, null), cancellationToken);
    }

    public async Task<StreamBTreeNodeId> AllocateId(CancellationToken cancellationToken)
    {
        var header = await bTreeNodeReadWriter.ReadHeader(cancellationToken);
        var newNodePosition = bTreeNodeReadWriter.CalculateNodePosition(header.NumNodes);

        var emptyNode = new StreamBTreeNode(
            new StreamBTreeNodeId(newNodePosition),
            null,
            [],
            []
        );
        await SaveNode(emptyNode, cancellationToken);
        await bTreeNodeReadWriter.WriteHeader(header.IncrementNodes(), cancellationToken);

        return emptyNode.Id;
    }

    public async Task<StreamBTreeNode?> GetRoot(CancellationToken cancellationToken)
    {
        var header = await bTreeNodeReadWriter.ReadHeader(cancellationToken);
        return header.Root == null ? null : await GetNode(header.Root, cancellationToken);
    }

    public async Task SetRoot(StreamBTreeNodeId id, CancellationToken cancellationToken)
    {
        var header = await bTreeNodeReadWriter.ReadHeader(cancellationToken);
        await bTreeNodeReadWriter.WriteHeader(header.SetRoot(id), cancellationToken);
    }

    public Task<StreamBTreeNode> GetNode(StreamBTreeNodeId id, CancellationToken cancellationToken)
    {
        return bTreeNodeReadWriter.ReadNode(id, cancellationToken);
    }

    public Task SaveNode(StreamBTreeNode node, CancellationToken cancellationToken)
    {
        return bTreeNodeReadWriter.WriteNode(node, cancellationToken);
    }
}