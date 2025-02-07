using SortTask.Domain.BTree;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeStore(Stream stream)
    : IBTreeStore<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>
{
    private StreamBTreeNodeId? _rootId;
    private readonly Dictionary<StreamBTreeNodeId, StreamBTreeNode> _nodes = [];

    public Task<StreamBTreeNodeId> AllocateId()
    {
        return Task.FromResult(new StreamBTreeNodeId(stream.Position));
    }

    public async Task<StreamBTreeNode?> GetRoot()
    {
        if (_rootId == null)
        {
            return null;
        }

        return await GetNode(_rootId);
    }

    public Task SetRoot(StreamBTreeNodeId id)
    {
        _rootId = id;
        return Task.CompletedTask;
    }

    public Task<StreamBTreeNode> GetNode(StreamBTreeNodeId id)
    {
        return Task.FromResult(_nodes[id]);
    }

    public Task SaveNode(StreamBTreeNode node)
    {
        _nodes[node.Id] = node;
        return Task.CompletedTask;
    }

    private async Task WriteNode(StreamBTreeNode node)
    {
        stream.Position = node.Id.Position;
        await WriteNodeId(node.Id);
        await WriteNodeId(node.ParentId);
    }

    private async Task WriteNodeId(StreamBTreeNodeId? id)
    {
    }
}