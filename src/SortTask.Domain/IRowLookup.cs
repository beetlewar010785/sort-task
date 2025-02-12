namespace SortTask.Domain;

public interface IRowLookup
{
    Task<Row> FindRow(long offset, int length, CancellationToken cancellationToken);
}