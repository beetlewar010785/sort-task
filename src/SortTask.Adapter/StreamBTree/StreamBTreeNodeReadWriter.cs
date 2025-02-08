using SortTask.Domain.BTree;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeNodeReadWriter(Stream stream, BTreeOrder order)
{
    private int NodeSize => sizeof(long) + // ParentId
                            sizeof(int) + // NumIndices
                            sizeof(int) + // NumChildren
                            sizeof(long) * order.MaxIndices + // Indices
                            sizeof(long) * order.MaxChildren; // Children;

    private static int HeaderSize => sizeof(long) + // RootId
                                     sizeof(long); // NumNodes

    /// <summary>
    /// Calculates position in the stream where the node with the specified nodeIndex should be inserted
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <returns></returns>
    public long CalculateNodePosition(long nodeIndex)
    {
        return HeaderSize + nodeIndex * NodeSize;
    }

    public async Task<StreamBTreeHeader> ReadHeader(CancellationToken cancellationToken)
    {
        stream.Position = 0;
        var buf = new byte[HeaderSize];
        await stream.ReadExactAsync(buf, cancellationToken);

        var (numNodes, position) = ReadLong(buf, 0);
        var (rootId, _) = ReadLong(buf, position);

        return new StreamBTreeHeader(numNodes, StreamBTreeNodeId.FromValue(rootId));
    }

    public async Task WriteHeader(StreamBTreeHeader header, CancellationToken cancellationToken)
    {
        var buf = new byte[HeaderSize];
        var position = WriteLong(header.NumNodes, buf, 0);
        WriteLong(StreamBTreeNodeId.ToValue(header.Root), buf, position);

        stream.Position = 0;
        await stream.WriteAsync(buf, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    public async Task<StreamBTreeNode> ReadNode(StreamBTreeNodeId id, CancellationToken cancellationToken)
    {
        stream.Position = id.Position;
        var buf = new byte[NodeSize];
        await stream.ReadExactAsync(buf, cancellationToken);
        var position = 0;
        (var parentId, position) = ReadLong(buf, position);
        (var numIndices, position) = ReadInt(buf, position);
        (var numChildren, position) = ReadInt(buf, position);
        (var indices, position) = ReadIndices(numIndices, buf, position);
        var children = ReadChildren(numChildren, buf, position);

        return new StreamBTreeNode(
            id,
            StreamBTreeNodeId.FromValue(parentId),
            children,
            indices
        );
    }

    public async Task WriteNode(StreamBTreeNode node, CancellationToken cancellationToken)
    {
        var buf = new byte[NodeSize];
        var position = WriteLong(StreamBTreeNodeId.ToValue(node.ParentId), buf, 0);
        position = WriteInt(node.Indices.Count, buf, position);
        position = WriteInt(node.Children.Count, buf, position);
        position = WriteIndices(node.Indices, buf, position);
        WriteChildren(node.Children, buf, position);

        stream.Position = node.Id.Position;
        await stream.WriteAsync(buf, cancellationToken);
        await stream.FlushAsync(cancellationToken);
    }

    private int WriteLong(long value, Span<byte> target, int position)
    {
        if (!BitConverter.TryWriteBytes(target[position..], value))
        {
            throw new Exception("Failed to write long.");
        }

        return position + sizeof(long);
    }

    private static (long, int) ReadLong(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToInt64(buf[position..]), position + sizeof(long));
    }

    private int WriteInt(int count, Span<byte> target, int position)
    {
        if (!BitConverter.TryWriteBytes(target[position..], count))
        {
            throw new Exception("Failed to write int.");
        }

        return position + sizeof(int);
    }

    private static (int, int) ReadInt(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToInt32(buf[position..]), position + sizeof(int));
    }

    private int WriteIndices(IReadOnlyList<StreamBTreeIndex> indices, Span<byte> target, int position)
    {
        foreach (var index in indices)
        {
            position = WriteLong(index.RowPosition, target, position);
        }

        return position;
    }

    private (IReadOnlyList<StreamBTreeIndex>, int) ReadIndices(int count, ReadOnlySpan<byte> buf, int position)
    {
        var indices = new List<StreamBTreeIndex>();

        for (var i = 0; i < count; i++)
        {
            (var value, position) = ReadLong(buf, position);
            indices.Add(new StreamBTreeIndex(value));
        }

        return (indices, position);
    }

    private void WriteChildren(IReadOnlyList<StreamBTreeNodeId> children, Span<byte> target, int position)
    {
        foreach (var id in children)
        {
            position = WriteLong(id.Position, target, position);
        }
    }

    private IReadOnlyList<StreamBTreeNodeId> ReadChildren(int count, ReadOnlySpan<byte> buf, int position)
    {
        var children = new List<StreamBTreeNodeId>();

        for (var i = 0; i < count; i++)
        {
            (var value, position) = ReadLong(buf, position);
            var child = StreamBTreeNodeId.FromValue(value) ?? throw new Exception("Unexpected null child.");
            children.Add(child);
        }

        return children;
    }
}