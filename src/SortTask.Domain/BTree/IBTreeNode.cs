namespace SortTask.Domain.BTree;

public interface IBTreeNode
{
    long Id { get; }
    long? ParentId { get; }
    IReadOnlyList<BTreeIndex> Indices { get; }
    IReadOnlyList<long> Children { get; }
}