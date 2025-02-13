namespace SortTask.Domain;

public interface IIndexer
{
    Task Index(Row row, long offset, int length, CancellationToken cancellationToken);
}
