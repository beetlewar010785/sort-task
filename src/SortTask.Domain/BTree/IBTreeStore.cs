namespace SortTask.Domain.BTree;

public interface IBTreeStore
{
    Task<long> AllocateId(CancellationToken cancellationToken);
    Task<long?> GetRoot(CancellationToken cancellationToken);
    Task SetRoot(long id, CancellationToken cancellationToken);
    Task<BTreeNode> GetNode(long id, CancellationToken cancellationToken);
    Task SaveNode(BTreeNode node, CancellationToken cancellationToken);
}