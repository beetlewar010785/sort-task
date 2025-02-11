namespace SortTask.Domain;

public interface IRowLookup<in TIndex>
    where TIndex : IIndex
{
    Task<Row> FindRow(TIndex index, CancellationToken cancellationToken);
}