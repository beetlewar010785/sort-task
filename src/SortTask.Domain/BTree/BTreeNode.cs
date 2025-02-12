namespace SortTask.Domain.BTree;

public readonly struct BTreeNode(
    long id,
    long? parentId,
    PositioningCollection<long> children,
    PositioningCollection<BTreeIndex> indices
)
{
    public long Id => id;
    public long? ParentId => parentId;
    public PositioningCollection<BTreeIndex> Indices => indices;
    public PositioningCollection<long> Children => children;

    public override string ToString()
    {
        return id.ToString();
    }
}