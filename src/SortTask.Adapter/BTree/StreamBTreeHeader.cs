namespace SortTask.Adapter.BTree;

public readonly struct StreamBTreeHeader(long numNodes, long? root)
{
    public const int NoRootId = -1;

    public long? Root { get; } = root;
    public long NumNodes { get; } = numNodes;

    public StreamBTreeHeader SetRoot(long newRoot)
    {
        return new StreamBTreeHeader(NumNodes, newRoot);
    }

    public StreamBTreeHeader IncrementNodes()
    {
        return new StreamBTreeHeader(NumNodes + 1, Root);
    }
}
