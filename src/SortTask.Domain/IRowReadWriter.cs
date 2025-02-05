namespace SortTask.Domain;

public interface IRowReader<out TRow>
    where TRow : IRow
{
    IAsyncEnumerable<TRow> ReadAsAsyncEnumerable();
}

public interface IRowWriter<in TRow>
    where TRow : IRow
{
    Task Write(TRow row);
    Task Flush(CancellationToken cancellationToken);
}