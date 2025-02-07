using SortTask.Domain.BTree;

namespace SortTask.Domain;

public interface IIndexer<in TIndex>
    where TIndex : IIndex
{
    Task Index(TIndex index, CancellationToken cancellationToken);
}