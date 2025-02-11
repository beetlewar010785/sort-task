using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public record StreamBTreeIndex(OphULong SentenceOph, long RowOffset, long RowLength) : IIndex;

public class StreamBTreeIndexEqualityComparer : IEqualityComparer<StreamBTreeIndex>
{
    public bool Equals(StreamBTreeIndex? x, StreamBTreeIndex? y)
    {
        return x?.RowOffset == y?.RowOffset;
    }

    public int GetHashCode(StreamBTreeIndex obj)
    {
        return obj.RowOffset.GetHashCode();
    }
}