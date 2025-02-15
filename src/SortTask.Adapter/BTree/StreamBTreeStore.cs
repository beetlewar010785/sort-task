using SortTask.Application;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class StreamBTreeStore<TOphValue>(StreamBTreeNodeReadWriter<TOphValue> bTreeNodeReadWriter)
    : IBTreeStore<TOphValue>, IInitializer where TOphValue : struct
{
    public long AllocateId()
    {
        var header = bTreeNodeReadWriter.ReadHeader();
        var newNodePosition = bTreeNodeReadWriter.CalculateNodePosition(header.NumNodes);

        var emptyNode = new BTreeNode<TOphValue>(
            newNodePosition,
            null,
            new PositioningItems<long>([]),
            new PositioningItems<BTreeIndex<TOphValue>>([])
        );
        SaveNode(emptyNode);
        bTreeNodeReadWriter.WriteHeader(header.IncrementNodes());

        return emptyNode.Id;
    }

    public long? GetRoot()
    {
        var header = bTreeNodeReadWriter.ReadHeader();
        return header.Root ?? null;
    }

    public void SetRoot(long id)
    {
        var header = bTreeNodeReadWriter.ReadHeader();
        bTreeNodeReadWriter.WriteHeader(header.SetRoot(id));
    }

    public BTreeNode<TOphValue> GetNode(long id)
    {
        return bTreeNodeReadWriter.ReadNode(id);
    }

    public void SaveNode(BTreeNode<TOphValue> node)
    {
        bTreeNodeReadWriter.WriteNode(node);
    }

    public void Initialize()
    {
        bTreeNodeReadWriter.WriteHeader(new StreamBTreeHeader(0, null));
    }
}
