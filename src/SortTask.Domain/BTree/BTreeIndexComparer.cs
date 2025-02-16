namespace SortTask.Domain.BTree;

public class BTreeIndexComparer<TOphValue>(
    IComparer<TOphValue> ophComparer,
    IComparer<Row> rowComparer,
    IRowLookup rowLookup) : IBTreeIndexComparer<TOphValue> where TOphValue : struct
{
    public int Compare(Row xRow, BTreeIndex<TOphValue> x, BTreeIndex<TOphValue> y)
    {
        var compareResult = ophComparer.Compare(x.OphValue, y.OphValue);
        if (compareResult != 0) return compareResult;

        var yRow = rowLookup.FindRow(y.Offset, y.Length);
        return rowComparer.Compare(xRow, yRow);
    }
}
