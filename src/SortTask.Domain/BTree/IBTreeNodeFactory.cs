namespace SortTask.Domain.BTree;

public interface IBTreeNodeFactory<out TNode, TIndex, TNodeId>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId> where TIndex : IIndex
{
    public TNode Create(
        TNodeId id,
        TNodeId? parentId,
        BTreeNodeCollection<TNodeId> children,
        BTreeIndexCollection<TIndex> indices
    );
}