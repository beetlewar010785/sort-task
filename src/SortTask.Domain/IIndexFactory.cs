namespace SortTask.Domain;

public interface IIndexFactory<out TIndex, in TRow>
    where TIndex : IIndex
    where TRow : IRow
{
    public TIndex CreateIndexFromRow(TRow row);
}