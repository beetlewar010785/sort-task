namespace SortTask.Domain.BTree;

public interface IBTreeIndexTraverser<TOphValue>
    where TOphValue : struct
{
    IAsyncEnumerable<BTreeIndex<TOphValue>> IterateAsAsyncEnumerable(CancellationToken cancellationToken);
}
