namespace SortTask.Domain.BTree;

public interface IBTreeStore<TNode, TIndex, TNodeId>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IIndex
{
    Task Initialize(CancellationToken cancellationToken);
    Task<TNodeId> AllocateId(CancellationToken cancellationToken);
    Task<TNode?> GetRoot(CancellationToken cancellationToken);
    Task SetRoot(TNodeId id, CancellationToken cancellationToken);
    Task<TNode> GetNode(TNodeId id, CancellationToken cancellationToken);
    Task SaveNode(TNode node, CancellationToken cancellationToken);
}