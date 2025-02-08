namespace SortTask.Adapter.StreamBTree;

public record StreamBTreeNodeId(long Position)
{
    private const int NullPosition = -1;

    public static StreamBTreeNodeId? FromValue(long position)
    {
        return position == NullPosition ? null : new StreamBTreeNodeId(position);
    }

    public static long ToValue(StreamBTreeNodeId? id)
    {
        return id?.Position ?? NullPosition;
    }

    public override string ToString()
    {
        return Position.ToString();
    }
}