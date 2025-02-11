namespace SortTask.Domain;

public interface IRowLookup
{
    Task<Row> FindRow(long offset, int length, CancellationToken cancellationToken);
}

public class RowLookupCounter(IRowLookup inner) : IRowLookup
{
    public long Count { get; private set; }

    public Task<Row> FindRow(long offset, int length, CancellationToken cancellationToken)
    {
        Count++;
        return inner.FindRow(offset, length, cancellationToken);
    }
}