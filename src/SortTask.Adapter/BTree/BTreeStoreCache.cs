using BitFaster.Caching.Lru;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class BTreeStoreCache(IBTreeStore inner, int capacity = 100000) : IBTreeStore
{
    private long? _rootId;
    private readonly ConcurrentLru<long, BTreeNode> _lru = new(1, capacity, EqualityComparer<long>.Default);

    public int GetNodeSkipCount { get; private set; }
    public int GetNodeExecuteCount { get; private set; }

    public Task<long> AllocateId(CancellationToken cancellationToken) =>
        inner.AllocateId(cancellationToken);

    public async Task<long?> GetRoot(CancellationToken cancellationToken)
    {
        return _rootId ??= await inner.GetRoot(cancellationToken);
    }

    public async Task SetRoot(long id, CancellationToken cancellationToken)
    {
        await inner.SetRoot(id, cancellationToken);
        _rootId = id;
    }

    public async Task<BTreeNode> GetNode(long id, CancellationToken cancellationToken)
    {
        if (_lru.TryGet(id, out var node))
        {
            GetNodeSkipCount++;
            return node;
        }

        node = await inner.GetNode(id, cancellationToken);
        GetNodeExecuteCount++;

        _lru.AddOrUpdate(node.Id, node);

        return node;
    }

    public async Task SaveNode(BTreeNode node, CancellationToken cancellationToken)
    {
        await inner.SaveNode(node, cancellationToken);
        _lru.AddOrUpdate(node.Id, node);
    }
}