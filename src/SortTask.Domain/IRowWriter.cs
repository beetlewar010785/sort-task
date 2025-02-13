namespace SortTask.Domain;

public interface IRowWriter
{
    Task Write(Row row, CancellationToken cancellationToken);
    Task Flush(CancellationToken cancellationToken);
}
