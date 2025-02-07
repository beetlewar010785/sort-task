namespace SortTask.Domain;

public interface IRowReader
{
    IAsyncEnumerable<ReadRow> ReadAsAsyncEnumerable(CancellationToken cancellationToken);
}