namespace SortTask.Adapter.BTree;

public readonly struct StreamBTreeHeader(long numNodes, long? root)
{
    public const int NoRootId = -1;

    public long? Root { get; } = root;
    public long NumNodes { get; } = numNodes;

    public StreamBTreeHeader SetRoot(long newRoot) => new(NumNodes, newRoot);

    public StreamBTreeHeader IncrementNodes() => new(NumNodes + 1, Root);
}