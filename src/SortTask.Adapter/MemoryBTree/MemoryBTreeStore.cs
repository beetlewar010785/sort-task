using SortTask.Domain.BTree;

namespace SortTask.Adapter.MemoryBTree;

public class MemoryBTreeStore
    : IBTreeStore<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>
{
    private MemoryBTreeNodeId? _rootId;
    private readonly Dictionary<MemoryBTreeNodeId, MemoryBTreeNode> _nodes = [];

    public Task Initialize(CancellationToken _)
    {
        return Task.CompletedTask;
    }

    public Task<MemoryBTreeNodeId> AllocateId(CancellationToken _)
    {
        return Task.FromResult(MemoryBTreeNodeId.New());
    }

    public async Task<MemoryBTreeNode?> GetRoot(CancellationToken _)
    {
        if (_rootId == null)
        {
            return null;
        }

        return await GetNode(_rootId, CancellationToken.None);
    }

    public Task SetRoot(MemoryBTreeNodeId id, CancellationToken _)
    {
        _rootId = id;
        return Task.CompletedTask;
    }

    public Task<MemoryBTreeNode> GetNode(MemoryBTreeNodeId id, CancellationToken _)
    {
        return Task.FromResult(_nodes[id]);
    }

    public Task SaveNode(MemoryBTreeNode node, CancellationToken _)
    {
        _nodes[node.Id] = node;
        return Task.CompletedTask;
    }
}