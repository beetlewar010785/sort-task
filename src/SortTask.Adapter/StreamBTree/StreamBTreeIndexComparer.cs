using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeIndexComparer(
    IComparer<OphULong> sentenceComparer,
    IComparer<Row> rowComparer,
    IRowLookup<StreamBTreeIndex> rowLookup)
    : IBTreeIndexComparer<StreamBTreeIndex>
{
    public async Task<int> Compare(StreamBTreeIndex x, StreamBTreeIndex y, CancellationToken cancellationToken)
    {
        var compareResult = sentenceComparer.Compare(x.SentenceOph, y.SentenceOph);
        if (compareResult != 0)
        {
            return compareResult;
        }

        var xRow = await rowLookup.FindRow(x, cancellationToken);
        var yRow = await rowLookup.FindRow(y, cancellationToken);
        return await Task.FromResult(rowComparer.Compare(xRow, yRow));
    }
}