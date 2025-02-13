using System.Globalization;

namespace SortTask.Domain.BTree;

public readonly struct BTreeNode<TOphValue>(
    long id,
    long? parentId,
    PositioningItems<long> children,
    PositioningItems<BTreeIndex<TOphValue>> indices
) where TOphValue : struct
{
    public long Id => id;
    public long? ParentId => parentId;
    public PositioningItems<BTreeIndex<TOphValue>> Indices => indices;
    public PositioningItems<long> Children => children;

    public override string ToString()
    {
        return id.ToString(CultureInfo.InvariantCulture);
    }
}
