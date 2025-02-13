namespace SortTask.Domain.BTree;

public interface IBTreeIndexComparer<TOphValue>
    where TOphValue : struct
{
    Task<int> Compare(BTreeIndex<TOphValue> x, BTreeIndex<TOphValue> y, CancellationToken cancellationToken);
}
