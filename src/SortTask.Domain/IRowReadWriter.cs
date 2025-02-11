namespace SortTask.Domain;

public interface IRowReadWriter
{
    Task Write(Row row, CancellationToken cancellationToken);
    Task Flush(CancellationToken cancellationToken);
    Task<Row> ReadAt(long offset, long length, CancellationToken cancellationToken);
}