namespace SortTask.Domain.BTree;

public record BTreeIndex(OphULong SentenceOph, long Offset, int Length);

public class StreamBTreeIndexEqualityComparer : IEqualityComparer<BTreeIndex>
{
    public bool Equals(BTreeIndex? x, BTreeIndex? y)
    {
        return x?.Offset == y?.Offset;
    }

    public int GetHashCode(BTreeIndex obj)
    {
        return obj.Offset.GetHashCode();
    }
}