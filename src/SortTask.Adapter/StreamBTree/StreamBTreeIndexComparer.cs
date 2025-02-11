using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeIndexComparer(
    IComparer<Row> rowComparer,
    IRowLookup<StreamBTreeIndex> rowLookup)
    : IBTreeIndexComparer<StreamBTreeIndex>
{
    public async Task<int> Compare(StreamBTreeIndex x, StreamBTreeIndex y, CancellationToken cancellationToken)
    {
        // todo improve
        var xRow = await rowLookup.FindRow(x, cancellationToken);
        var yRow = await rowLookup.FindRow(y, cancellationToken);
        return await Task.FromResult(rowComparer.Compare(xRow, yRow));
    }
}