namespace SortTask.Domain.BTree;

public interface IBTreeIndexTraverser
{
    IAsyncEnumerable<BTreeIndex> IterateAsAsyncEnumerable(CancellationToken cancellationToken);
}