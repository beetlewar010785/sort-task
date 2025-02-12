namespace SortTask.Domain.BTree;

public interface IBTreeStore<TOphValue>
    where TOphValue : struct
{
    Task<long> AllocateId(CancellationToken cancellationToken);
    Task<long?> GetRoot(CancellationToken cancellationToken);
    Task SetRoot(long id, CancellationToken cancellationToken);
    Task<BTreeNode<TOphValue>> GetNode(long id, CancellationToken cancellationToken);
    Task SaveNode(BTreeNode<TOphValue> node, CancellationToken cancellationToken);
}