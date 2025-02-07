namespace SortTask.Domain.BTree;

public interface IBTreeNodeFactory<out TNode, in TIndex, in TNodeId>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId> where TIndex : IIndex
{
    public TNode Create(
        TNodeId id,
        TNodeId? parentId,
        IReadOnlyList<TNodeId> children,
        IReadOnlyList<TIndex> indices
    );
}