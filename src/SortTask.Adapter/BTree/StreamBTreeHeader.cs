namespace SortTask.Adapter.BTree;

public record StreamBTreeHeader(long NumNodes, long? Root)
{
    public const int NoRootId = -1;
    
    public StreamBTreeHeader SetRoot(long root) => this with { Root = root };

    public StreamBTreeHeader IncrementNodes() => this with { NumNodes = NumNodes + 1 };
}