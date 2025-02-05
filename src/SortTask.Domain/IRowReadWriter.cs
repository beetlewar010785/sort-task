namespace SortTask.Domain;

public interface IRowReadWriter
{
    IAsyncEnumerable<Row> ReadAsAsyncEnumerable();
    Task Write(Row row);
    Task Flush(CancellationToken cancellationToken);
}