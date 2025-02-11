namespace SortTask.Domain;

public interface IRowIterator
{
    IAsyncEnumerable<RowWithOffset> ReadAsAsyncEnumerable(CancellationToken cancellationToken);
}