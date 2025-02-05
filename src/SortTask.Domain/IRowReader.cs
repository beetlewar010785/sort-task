namespace SortTask.Domain;

public interface IRowReader<out TRow>
    where TRow : IRow
{
    IAsyncEnumerable<TRow> ReadAsAsyncEnumerable();
}