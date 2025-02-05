namespace SortTask.Domain;

public interface IRowReadWriter
{
    Task Write(Row row);
    Task Flush(CancellationToken cancellationToken);
}