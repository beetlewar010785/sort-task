using SortTask.Application;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class StreamBTreeStore<TOphValue>(StreamBTreeNodeReadWriter<TOphValue> bTreeNodeReadWriter)
    : IBTreeStore<TOphValue>, IInitializer where TOphValue : struct
{
    public long AllocateId()
    {
        return bTreeNodeReadWriter.Allocate();
    }

    public long? GetRoot()
    {
        return bTreeNodeReadWriter.GetRoot();
    }

    public void SetRoot(long id)
    {
        bTreeNodeReadWriter.SetRoot(id);
    }

    public BTreeNode<TOphValue> GetNode(long id)
    {
        return bTreeNodeReadWriter.ReadNode(id);
    }

    public void SaveNode(BTreeNode<TOphValue> node)
    {
        bTreeNodeReadWriter.WriteNode(node);
    }

    // todo remove
    public void Initialize()
    {
    }
}
