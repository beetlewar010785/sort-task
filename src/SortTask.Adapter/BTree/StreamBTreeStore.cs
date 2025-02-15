using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class StreamBTreeStore<TOphValue>
    : IBTreeStore<TOphValue> where TOphValue : struct
{
    private const int NoRootId = -1;
    private const int AllocateNodes = 10000;

    private readonly Stream _stream;
    private readonly IOphReadWriter<TOphValue> _ophReadWriter;

    // move to struct?
    private readonly int _nodeSize;

    private long? _rootId;
    private long _numNodes;
    private long _numAllocatedNodes;
    private byte[]? _allocationBuf;

    public StreamBTreeStore(
        Stream stream,
        BTreeOrder order,
        IOphReadWriter<TOphValue> ophReadWriter)
    {
        _stream = stream;
        _ophReadWriter = ophReadWriter;

        var indexSize =
            ophReadWriter.Size + // IndexOph
            sizeof(long) + // Row position
            sizeof(int); // row length

        _nodeSize =
            sizeof(long) + // ParentId
            sizeof(int) + // NumIndices
            sizeof(int) + // NumChildren
            indexSize * order.MaxIndices + // Indices
            sizeof(long) * order.MaxChildren; // Children;
    }

    public long AllocateId()
    {
        var newNodePosition = _numNodes * _nodeSize;
        _numNodes++;

        if (_numNodes > _numAllocatedNodes)
        {
            _allocationBuf ??= new byte[AllocateNodes * _nodeSize];
            _stream.Write(_allocationBuf);
            _numAllocatedNodes += AllocateNodes;
        }

        return newNodePosition;
    }

    public long? GetRoot() => _rootId;

    public void SetRoot(long id)
    {
        _rootId = id;
    }

    public BTreeNode<TOphValue> GetNode(long id)
    {
        _stream.Position = id;
        var buf = new byte[_nodeSize];
        _stream.ReadAll(buf);
        var position = 0;
        (var parentId, position) = BinaryReadWriter.ReadLong(buf, position);
        (var numIndices, position) = BinaryReadWriter.ReadInt(buf, position);
        (var numChildren, position) = BinaryReadWriter.ReadInt(buf, position);
        (var indices, position) = ReadIndices(numIndices, buf, position);
        var children = ReadChildren(numChildren, buf, position);

        return new BTreeNode<TOphValue>(
            id,
            parentId == NoRootId ? null : parentId,
            children,
            indices
        );
    }

    public void SaveNode(BTreeNode<TOphValue> node)
    {
        var buf = new byte[_nodeSize];
        var position = BinaryReadWriter.WriteLong(node.ParentId ?? NoRootId, buf, 0);
        position = BinaryReadWriter.WriteInt(node.Indices.Length, buf, position);
        position = BinaryReadWriter.WriteInt(node.Children.Length, buf, position);
        position = WriteIndices(node.Indices.Values, buf, position);
        _ = WriteChildren(node.Children.Values, buf, position);

        _stream.Position = node.Id;
        _stream.Write(buf);
        _stream.Flush();
    }

    private int WriteIndices(IEnumerable<BTreeIndex<TOphValue>> indices, Span<byte> target, int position)
    {
        foreach (var index in indices)
        {
            position = _ophReadWriter.Write(index.OphValue, target, position);
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
            (var ophValue, position) = _ophReadWriter.Read(buf, position);
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
