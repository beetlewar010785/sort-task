namespace SortTask.Adapter.StreamBTree;

public record StreamBTreeNodeId(long Position)
{
    public override string ToString()
    {
        return Position.ToString();
    }
}