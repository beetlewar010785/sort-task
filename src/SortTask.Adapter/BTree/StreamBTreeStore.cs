using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public class StreamBTreeStore<TOphValue>
    : IBTreeStore<TOphValue>, IDisposable where TOphValue : struct
{
    private const int NoRootId = -1;
    private const int AllocateBytes = 100 * 1024 * 1024;
    private const int CacheSizeInBytes = 100 * 1024 * 1024;

    private readonly IOphReadWriter<TOphValue> _ophReadWriter;
    private readonly Stream _stream;

    private readonly int _nodeCacheLimit;
    private readonly byte[] _nodeBuf;
    private readonly int _nodeSize;
    private long _numNodes;
    private long? _rootId;

    private readonly Lazy<byte[]> _allocationBuf = new(() => new byte[AllocateBytes]);
    private readonly Dictionary<long, BTreeNode<TOphValue>> _nodesCache = new();

    public StreamBTreeStore(
        Stream stream,
        BTreeOrder order,
        IOphReadWriter<TOphValue> ophReadWriter)
    {
        _stream = stream;
        _ophReadWriter = ophReadWriter;

        var indexSize =
            ophReadWriter.Size + // oph
            sizeof(long) + // offset
            sizeof(int); // length

        _nodeSize =
            sizeof(long) + // ParentId
            sizeof(short) + // NumIndices
            sizeof(short) + // NumChildren
            indexSize * order.MaxIndices + // Indices
            sizeof(long) * order.MaxChildren; // Children;

        _nodeBuf = new byte[_nodeSize];
        _nodeCacheLimit = CacheSizeInBytes / _nodeSize;
    }

    public long AllocateId()
    {
        var newNodePosition = _numNodes * _nodeSize;
        var endOfNodeOffset = newNodePosition + _nodeSize;

        if (endOfNodeOffset >= _stream.Length)
        {
            // allocation required
            _stream.Position = newNodePosition;
            _stream.Write(_allocationBuf.Value);
            _stream.Flush();
        }

        _numNodes++;
        return newNodePosition;
    }

    public BTreeNode<TOphValue> GetNode(long id)
    {
        return _nodesCache.TryGetValue(id, out var node) ? node : ReadNode(id);
    }

    public void SaveNode(BTreeNode<TOphValue> node)
    {
        _nodesCache[node.Id] = node;
        FlushIfRequired();
    }

    public long? GetRoot()
    {
        return _rootId;
    }

    public void SetRoot(long id)
    {
        _rootId = id;
    }

    public void Dispose()
    {
        Flush();
        GC.SuppressFinalize(this);
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

    private void FlushIfRequired()
    {
        if (_nodesCache.Count < _nodeCacheLimit)
        {
            return;
        }

        Flush();
    }

    private void Flush()
    {
        foreach (var nodeCache in _nodesCache)
        {
            WriteNode(nodeCache.Value);
        }

        _nodesCache.Clear();
    }

    private void WriteNode(BTreeNode<TOphValue> node)
    {
        var position = BinaryReadWriter.WriteLong(node.ParentId ?? NoRootId, _nodeBuf, 0);
        position = BinaryReadWriter.WriteShort(checked((short)node.Indices.Length), _nodeBuf, position);
        position = BinaryReadWriter.WriteShort(checked((short)node.Children.Length), _nodeBuf, position);
        position = WriteIndices(node.Indices.Values, _nodeBuf, position);
        _ = WriteChildren(node.Children.Values, _nodeBuf, position);

        _stream.Position = node.Id;
        _stream.Write(_nodeBuf);
        _stream.Flush();
    }

    private BTreeNode<TOphValue> ReadNode(long id)
    {
        _stream.Position = id;
        _stream.ReadExactly(_nodeBuf);
        var position = 0;
        (var parentId, position) = BinaryReadWriter.ReadLong(_nodeBuf, position);
        (var numIndices, position) = BinaryReadWriter.ReadShort(_nodeBuf, position);
        (var numChildren, position) = BinaryReadWriter.ReadShort(_nodeBuf, position);
        (var indices, position) = ReadIndices(numIndices, _nodeBuf, position);
        var children = ReadChildren(numChildren, _nodeBuf, position);

        return new BTreeNode<TOphValue>(
            id,
            parentId == NoRootId ? null : parentId,
            children,
            indices
        );
    }
}
