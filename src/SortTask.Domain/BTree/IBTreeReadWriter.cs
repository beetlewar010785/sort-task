namespace SortTask.Domain.BTree;

public interface IBTreeReadWriter<TNode, TIndex, TNodeId>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IBTreeIndex
{
    Task<TNodeId> AllocateId();
    Task<TNode?> GetRoot();
    Task SetRoot(TNodeId id);
    Task<TNode> GetNode(TNodeId id);
    Task SaveNode(TNode node);
}