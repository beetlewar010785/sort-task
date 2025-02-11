namespace SortTask.Domain;

public interface IIndexTraverser<out TIndex>
    where TIndex : IIndex
{
    IAsyncEnumerable<TIndex> IterateAsAsyncEnumerable(CancellationToken cancellationToken);
}