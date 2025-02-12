namespace SortTask.Domain.BTree;

public readonly struct BTreeNode<TOphValue>(
    long id,
    long? parentId,
    PositioningCollection<long> children,
    PositioningCollection<BTreeIndex<TOphValue>> indices
) where TOphValue : struct
{
    public long Id => id;
    public long? ParentId => parentId;
    public PositioningCollection<BTreeIndex<TOphValue>> Indices => indices;
    public PositioningCollection<long> Children => children;

    public override string ToString()
    {
        return id.ToString();
    }
}