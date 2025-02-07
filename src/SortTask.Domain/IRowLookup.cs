namespace SortTask.Domain;

public interface IRowLookup<in TIndex>
    where TIndex : IIndex
{
    Task<ReadRow> FindRow(TIndex index, CancellationToken cancellationToken);
}