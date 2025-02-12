using BitFaster.Caching.Lru;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class BTreeStoreCache<TOphValue>(
    IBTreeStore<TOphValue> inner,
    int capacity = 100000) : IBTreeStore<TOphValue>
    where TOphValue : struct
{
    private long? _rootId;

    private readonly ConcurrentLru<long, BTreeNode<TOphValue>> _lru = new(
        1,
        capacity,
        EqualityComparer<long>.Default);

    public long GetNodeSkipCount { get; private set; }
    public long GetNodeExecuteCount { get; private set; }

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

    public async Task<BTreeNode<TOphValue>> GetNode(long id, CancellationToken cancellationToken)
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

    public async Task SaveNode(BTreeNode<TOphValue> node, CancellationToken cancellationToken)
    {
        await inner.SaveNode(node, cancellationToken);
        _lru.AddOrUpdate(node.Id, node);
    }
}