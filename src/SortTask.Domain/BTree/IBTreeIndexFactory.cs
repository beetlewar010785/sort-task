namespace SortTask.Domain.BTree;

public interface IBTreeIndexFactory<out TIndex, in TRow>
    where TIndex : IBTreeIndex
    where TRow : IRow
{
    public TIndex CreateIndexFromRow(TRow row);
}