namespace SortTask.Domain.BTree;

public class BTreeIndexComparer(
    IComparer<OphULong> sentenceComparer,
    IComparer<Row> rowComparer,
    IRowLookup rowLookup) : IBTreeIndexComparer
{
    public async Task<int> Compare(BTreeIndex x, BTreeIndex y, CancellationToken cancellationToken)
    {
        var compareResult = sentenceComparer.Compare(x.SentenceOph, y.SentenceOph);
        if (compareResult != 0)
        {
            return compareResult;
        }

        var xRow = await rowLookup.FindRow(x.Offset, x.Length, cancellationToken);
        var yRow = await rowLookup.FindRow(y.Offset, y.Length, cancellationToken);
        return await Task.FromResult(rowComparer.Compare(xRow, yRow));
    }
}