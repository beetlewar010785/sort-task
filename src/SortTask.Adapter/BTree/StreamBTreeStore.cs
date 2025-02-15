using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class StreamBTreeStore<TOphValue>
    : IBTreeStore<TOphValue> where TOphValue : struct
{
    private const int NoRootId = -1;
    private const int AllocateNodes = 10000;
    private readonly byte[] _allocateBuf;
    private readonly byte[] _nodeBuf;
    private readonly int _nodeSize;
    private readonly IOphReadWriter<TOphValue> _ophReadWriter;

    private readonly Stream _stream;

    private long _numNodes;
    private long? _rootId;

    public StreamBTreeStore(
        Stream stream,
        BTreeOrder order,
        IOphReadWriter<TOphValue> ophReadWriter)
    {
        _stream = stream;
        _ophReadWriter = ophReadWriter;

        _nodeSize =
            sizeof(long) + // ParentId
            sizeof(int) + // NumIndices
            sizeof(int) + // NumChildren
            (ophReadWriter.Size + sizeof(long) + sizeof(int)) * order.MaxIndices + // Indices
            sizeof(long) * order.MaxChildren; // Children;

        _nodeBuf = new byte[_nodeSize];
        _allocateBuf = new byte[_nodeSize * AllocateNodes];
    }

    public long AllocateId()
    {
        var newNodePosition = _numNodes * _nodeSize;

        // SaveNode(new BTreeNode<TOphValue>(
        //     newNodePosition,
        //     null,
        //     new PositioningItems<long>([]),
        //     new PositioningItems<BTreeIndex<TOphValue>>([])));

        if (_numNodes % AllocateNodes == 0)
        {
            // allocation required
            _stream.Position = newNodePosition;
            _stream.Write(_allocateBuf);
            _stream.Flush();
        }

        _numNodes++;
        return newNodePosition;
    }

    public BTreeNode<TOphValue> GetNode(long id)
    {
        _stream.Position = id;
        _stream.ReadExactly(_nodeBuf);
        var position = 0;
        (var parentId, position) = BinaryReadWriter.ReadLong(_nodeBuf, position);
        (var numIndices, position) = BinaryReadWriter.ReadInt(_nodeBuf, position);
        (var numChildren, position) = BinaryReadWriter.ReadInt(_nodeBuf, position);
        (var indices, position) = ReadIndices(numIndices, _nodeBuf, position);
        var children = ReadChildren(numChildren, _nodeBuf, position);

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

    public long? GetRoot()
    {
        return _rootId;
    }

    public void SetRoot(long id)
    {
        _rootId = id;
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
