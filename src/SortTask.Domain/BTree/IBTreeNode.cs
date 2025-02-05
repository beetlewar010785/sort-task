namespace SortTask.Domain.BTree;

public interface IBTreeNode<out TNode, out TIndex, TNodeId>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IBTreeIndex
{
    TNodeId Id { get; }
    TNodeId? ParentId { get; }
    BTreeNodeCollection<TNodeId> Children { get; }
    IReadOnlyList<TIndex> Indexes { get; }
}