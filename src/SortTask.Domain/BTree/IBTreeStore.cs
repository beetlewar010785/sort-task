namespace SortTask.Domain.BTree;

public interface IBTreeStore<TOphValue>
    where TOphValue : struct
{
    long AllocateId();
    long? GetRoot();
    void SetRoot(long id);
    BTreeNode<TOphValue> GetNode(long id);
    void SaveNode(BTreeNode<TOphValue> node);
}
