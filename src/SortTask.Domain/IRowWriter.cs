namespace SortTask.Domain;

public interface IRowWriter<in TRow>
    where TRow : IRow
{
    Task Write(TRow row);
    Task Flush(CancellationToken cancellationToken);
}