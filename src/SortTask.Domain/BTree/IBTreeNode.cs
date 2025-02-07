namespace SortTask.Domain.BTree;

public interface IBTreeNode<out TNode, out TIndex, out TNodeId>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IIndex
{
    TNodeId Id { get; }
    TNodeId? ParentId { get; }
    IReadOnlyList<TIndex> Indices { get; }
    IReadOnlyList<TNodeId> Children { get; }
}