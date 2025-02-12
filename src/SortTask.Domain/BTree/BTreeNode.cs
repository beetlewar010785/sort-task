namespace SortTask.Domain.BTree;

public readonly struct BTreeNode(
    long id,
    long? parentId,
    IReadOnlyList<long> children,
    IReadOnlyList<BTreeIndex> indices
) : IBTreeNode
{
    public long Id => id;
    public long? ParentId => parentId;
    public IReadOnlyList<BTreeIndex> Indices => indices;
    public IReadOnlyList<long> Children => children;

    public override string ToString()
    {
        return id.ToString();
    }
}