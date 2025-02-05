namespace SortTask.Domain.BTree;

public interface IBTreeNodeFactory<out TNode, in TIndex, TNodeId>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId> where TIndex : IBTreeIndex
{
    public TNode Create(
        TNodeId id,
        TNodeId? parentId,
        BTreeNodeCollection<TNodeId> children,
        IEnumerable<TIndex> indexes
    );
}