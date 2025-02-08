namespace SortTask.Adapter.StreamBTree;

public record StreamBTreeHeader(long NumNodes, StreamBTreeNodeId? Root)
{
    public StreamBTreeHeader SetRoot(StreamBTreeNodeId root) => this with { Root = root };

    public StreamBTreeHeader IncrementNodes() => this with { NumNodes = NumNodes + 1 };
}