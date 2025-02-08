using SortTask.Domain.BTree;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeStore(Stream stream, BTreeOrder order)
    : IBTreeStore<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>
{
    private readonly Dictionary<StreamBTreeNodeId, StreamBTreeNode> _nodes = [];
    private readonly StreamBTreeNodeReadWriter _nodeReadWriter = new StreamBTreeNodeReadWriter(stream, order);

    public Task Initialize(CancellationToken cancellationToken)
    {
        return _nodeReadWriter.WriteHeader(new StreamBTreeHeader(0, null), cancellationToken);
    }

    public async Task<StreamBTreeNodeId> AllocateId(CancellationToken cancellationToken)
    {
        var header = await _nodeReadWriter.ReadHeader(cancellationToken);
        var newNodePosition = _nodeReadWriter.CalculateNodePosition(header.NumNodes + 1);

        var emptyNode = new StreamBTreeNode(
            new StreamBTreeNodeId(newNodePosition),
            null,
            [],
            []
        );
        await SaveNode(emptyNode, cancellationToken);

        return emptyNode.Id;
    }

    public async Task<StreamBTreeNode?> GetRoot(CancellationToken cancellationToken)
    {
        var header = await _nodeReadWriter.ReadHeader(cancellationToken);
        return header.Root == null ? null : await GetNode(header.Root, cancellationToken);
    }

    public async Task SetRoot(StreamBTreeNodeId id, CancellationToken cancellationToken)
    {
        var header = await _nodeReadWriter.ReadHeader(cancellationToken);
        await _nodeReadWriter.WriteHeader(header.SetRoot(id), cancellationToken);
    }

    public Task<StreamBTreeNode> GetNode(StreamBTreeNodeId id, CancellationToken cancellationToken)
    {
        return _nodeReadWriter.ReadNode(id, cancellationToken);
    }

    public async Task SaveNode(StreamBTreeNode node, CancellationToken cancellationToken)
    {
        await _nodeReadWriter.WriteNode(node, cancellationToken);
        var header = await _nodeReadWriter.ReadHeader(cancellationToken);
        await _nodeReadWriter.WriteHeader(header.IncrementNodes(), cancellationToken);
    }
}