namespace SortTask.Domain;

public interface IRowWriter
{
    Task Write(Row row);
    Task Flush(CancellationToken cancellationToken);
}