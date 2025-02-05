namespace SortTask.Domain.BTree;

public interface IBTreeNode<out TNode, TIndex, TNodeId>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IIndex
{
    TNodeId Id { get; }
    TNodeId? ParentId { get; }
    BTreeIndexCollection<TIndex> Indices { get; }
    BTreeNodeCollection<TNodeId> Children { get; }
}