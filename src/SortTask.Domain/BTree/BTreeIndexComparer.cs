namespace SortTask.Domain.BTree;

public class BTreeIndexComparer<TOphValue>(
    IComparer<TOphValue> ophComparer,
    IComparer<Row> rowComparer,
    IRowLookup rowLookup) : IBTreeIndexComparer<TOphValue> where TOphValue : struct
{
    public async Task<int> Compare(
        BTreeIndex<TOphValue> x,
        BTreeIndex<TOphValue> y,
        CancellationToken cancellationToken)
    {
        var compareResult = ophComparer.Compare(x.OphValue, y.OphValue);
        if (compareResult != 0) return compareResult;

        var xRow = await rowLookup.FindRow(x.Offset, x.Length, cancellationToken);
        var yRow = await rowLookup.FindRow(y.Offset, y.Length, cancellationToken);
        return await Task.FromResult(rowComparer.Compare(xRow, yRow));
    }
}
