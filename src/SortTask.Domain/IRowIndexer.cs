namespace SortTask.Domain;

public interface IRowIndexer<in TRow>
    where TRow : IRow
{
    Task IndexRow(TRow row);
}