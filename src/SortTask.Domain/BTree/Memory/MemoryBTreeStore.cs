using System.Runtime.CompilerServices;

namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeStore
    : IBTreeStore<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>
{
    private MemoryBTreeNodeId? _rootId;
    private readonly Dictionary<MemoryBTreeNodeId, MemoryBTreeNode> _nodes = [];

    public Task<MemoryBTreeNodeId> AllocateId()
    {
        return Task.FromResult(MemoryBTreeNodeId.New());
    }

    public async Task<MemoryBTreeNode?> GetRoot()
    {
        if (_rootId == null)
        {
            return null;
        }

        return await GetNode(_rootId);
    }

    public Task SetRoot(MemoryBTreeNodeId id)
    {
        _rootId = id;
        return Task.CompletedTask;
    }

    public Task<MemoryBTreeNode> GetNode(MemoryBTreeNodeId id)
    {
        return Task.FromResult(_nodes[id]);
    }

    public Task SaveNode(MemoryBTreeNode node)
    {
        _nodes[node.Id] = node;
        return Task.CompletedTask;
    }
}