using BitFaster.Caching.Lru;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class BTreeStoreCache<TOphValue>(
    IBTreeStore<TOphValue> inner,
    int capacity = 100000) : IBTreeStore<TOphValue>
    where TOphValue : struct
{
    private readonly ConcurrentLru<long, BTreeNode<TOphValue>> _lru = new(
        1,
        capacity,
        EqualityComparer<long>.Default);

    private long? _rootId;

    public long GetNodeSkipCount { get; private set; }
    public long GetNodeExecuteCount { get; private set; }

    public long AllocateId()
    {
        return inner.AllocateId();
    }

    public long? GetRoot()
    {
        return _rootId ??= inner.GetRoot();
    }

    public void SetRoot(long id)
    {
        inner.SetRoot(id);
        _rootId = id;
    }

    public BTreeNode<TOphValue> GetNode(long id)
    {
        if (_lru.TryGet(id, out var node))
        {
            GetNodeSkipCount++;
            return node;
        }

        node = inner.GetNode(id);
        GetNodeExecuteCount++;

        _lru.AddOrUpdate(node.Id, node);

        return node;
    }

    public void SaveNode(BTreeNode<TOphValue> node)
    {
        inner.SaveNode(node);
        _lru.AddOrUpdate(node.Id, node);
    }
}
