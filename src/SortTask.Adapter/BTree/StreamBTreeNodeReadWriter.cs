using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class StreamBTreeNodeReadWriter<TOphValue>(
    Stream stream,
    BTreeOrder order,
    IOphReadWriter<TOphValue> ophReadWriter)
    where TOphValue : struct
{
    // move to struct?
    private readonly int _nodeSize =
        sizeof(long) + // ParentId
        sizeof(int) + // NumIndices
        sizeof(int) + // NumChildren
        (ophReadWriter.Size + sizeof(long) + sizeof(int)) * order.MaxIndices + // Indices
        sizeof(long) * order.MaxChildren; // Children;

    private bool _dirty;

    private static int HeaderSize =>
        sizeof(long) + // RootId
        sizeof(long); // NumNodes

    public long CalculateNodePosition(long nodeIndex)
    {
        return HeaderSize + nodeIndex * _nodeSize;
    }

    public StreamBTreeHeader ReadHeader()
    {
        FlushIfRequired();

        stream.Position = 0;
        var buf = new byte[HeaderSize];
        stream.ReadAll(buf);

        var (numNodes, position) = BinaryReadWriter.ReadLong(buf, 0);
        var (rootId, _) = BinaryReadWriter.ReadLong(buf, position);

        return new StreamBTreeHeader(numNodes, rootId == StreamBTreeHeader.NoRootId ? null : rootId);
    }

    public void WriteHeader(StreamBTreeHeader header)
    {
        var buf = new byte[HeaderSize];
        var position = BinaryReadWriter.WriteLong(header.NumNodes, buf, 0);
        _ = BinaryReadWriter.WriteLong(header.Root ?? StreamBTreeHeader.NoRootId, buf, position);

        stream.Position = 0;
        stream.Write(buf);
        _dirty = true;
    }

    public BTreeNode<TOphValue> ReadNode(long id)
    {
        FlushIfRequired();

        stream.Position = id;
        var buf = new byte[_nodeSize];
        stream.ReadAll(buf);
        var position = 0;
        (var parentId, position) = BinaryReadWriter.ReadLong(buf, position);
        (var numIndices, position) = BinaryReadWriter.ReadInt(buf, position);
        (var numChildren, position) = BinaryReadWriter.ReadInt(buf, position);
        (var indices, position) = ReadIndices(numIndices, buf, position);
        var children = ReadChildren(numChildren, buf, position);

        return new BTreeNode<TOphValue>(
            id,
            parentId == StreamBTreeHeader.NoRootId ? null : parentId,
            children,
            indices
        );
    }

    public void WriteNode(BTreeNode<TOphValue> node)
    {
        var buf = new byte[_nodeSize];
        var position = BinaryReadWriter.WriteLong(node.ParentId ?? StreamBTreeHeader.NoRootId, buf, 0);
        position = BinaryReadWriter.WriteInt(node.Indices.Length, buf, position);
        position = BinaryReadWriter.WriteInt(node.Children.Length, buf, position);
        position = WriteIndices(node.Indices.Values, buf, position);
        _ = WriteChildren(node.Children.Values, buf, position);

        stream.Position = node.Id;
        stream.Write(buf);
        _dirty = true;
    }

    private void FlushIfRequired()
    {
        if (!_dirty) return;

        stream.Flush();
        _dirty = false;
    }

    private int WriteIndices(IEnumerable<BTreeIndex<TOphValue>> indices, Span<byte> target, int position)
    {
        foreach (var index in indices)
        {
            position = ophReadWriter.Write(index.OphValue, target, position);
            position = BinaryReadWriter.WriteLong(index.Offset, target, position);
            position = BinaryReadWriter.WriteInt(index.Length, target, position);
        }

        return position;
    }

    private (PositioningItems<BTreeIndex<TOphValue>>, int) ReadIndices(int count, ReadOnlySpan<byte> buf,
        int position)
    {
        var indices = new BTreeIndex<TOphValue>[count];

        for (var i = 0; i < count; i++)
        {
            (var ophValue, position) = ophReadWriter.Read(buf, position);
            (var offset, position) = BinaryReadWriter.ReadLong(buf, position);
            (var length, position) = BinaryReadWriter.ReadInt(buf, position);

            var index = new BTreeIndex<TOphValue>(ophValue, offset, length);
            indices[i] = index;
        }

        return (new PositioningItems<BTreeIndex<TOphValue>>(indices), position);
    }

    private static int WriteChildren(IEnumerable<long> children, Span<byte> target, int position)
    {
        foreach (var id in children) position = BinaryReadWriter.WriteLong(id, target, position);

        return position;
    }


    private static PositioningItems<long> ReadChildren(int count, ReadOnlySpan<byte> buf, int position)
    {
        var children = new long[count];

        for (var i = 0; i < count; i++)
        {
            (var value, position) = BinaryReadWriter.ReadLong(buf, position);
            children[i] = value;
        }

        return new PositioningItems<long>(children);
    }
}
