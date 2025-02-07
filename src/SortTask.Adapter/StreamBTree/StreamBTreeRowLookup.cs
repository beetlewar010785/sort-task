using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeRowLookup(Stream stream) : IRowLookup<StreamBTreeIndex>
{
    private readonly StreamReader _streamReader = new(stream, leaveOpen: true);

    public Task<ReadRow> FindRow(StreamBTreeIndex index, CancellationToken cancellationToken)
    {
        stream.Position = index.RowPosition;
        return _streamReader.DeserializeRow(cancellationToken);
    }
}