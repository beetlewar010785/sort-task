namespace SortTask.Domain;

public interface IRowLookup<in TIndex>
    where TIndex : IIndex
{
    Task<Row> FindRow(TIndex index, CancellationToken cancellationToken);
}

public class RowLookupCounter<TIndex>(IRowLookup<TIndex> inner) : IRowLookup<TIndex> where TIndex : IIndex
{
    public long Count { get; private set; }

    public Task<Row> FindRow(TIndex index, CancellationToken cancellationToken)
    {
        Count++;
        return inner.FindRow(index, cancellationToken);
    }
}