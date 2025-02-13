using SortTask.Application;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class StreamBTreeStore<TOphValue>(StreamBTreeNodeReadWriter<TOphValue> bTreeNodeReadWriter)
    : IBTreeStore<TOphValue>, IInitializer where TOphValue : struct
{
    public async Task<long> AllocateId(CancellationToken cancellationToken)
    {
        var header = await bTreeNodeReadWriter.ReadHeader(cancellationToken);
        var newNodePosition = bTreeNodeReadWriter.CalculateNodePosition(header.NumNodes);

        var emptyNode = new BTreeNode<TOphValue>(
            newNodePosition,
            null,
            new PositioningItems<long>([]),
            new PositioningItems<BTreeIndex<TOphValue>>([])
        );
        await SaveNode(emptyNode, cancellationToken);
        await bTreeNodeReadWriter.WriteHeader(header.IncrementNodes(), cancellationToken);

        return emptyNode.Id;
    }

    public async Task<long?> GetRoot(CancellationToken cancellationToken)
    {
        var header = await bTreeNodeReadWriter.ReadHeader(cancellationToken);
        return header.Root ?? null;
    }

    public async Task SetRoot(long id, CancellationToken cancellationToken)
    {
        var header = await bTreeNodeReadWriter.ReadHeader(cancellationToken);
        await bTreeNodeReadWriter.WriteHeader(header.SetRoot(id), cancellationToken);
    }

    public Task<BTreeNode<TOphValue>> GetNode(long id, CancellationToken cancellationToken)
    {
        return bTreeNodeReadWriter.ReadNode(id, cancellationToken);
    }

    public Task SaveNode(BTreeNode<TOphValue> node, CancellationToken cancellationToken)
    {
        return bTreeNodeReadWriter.WriteNode(node, cancellationToken);
    }

    public Task Initialize(CancellationToken token)
    {
        return bTreeNodeReadWriter.WriteHeader(new StreamBTreeHeader(0, null), token);
    }
}
