namespace SortTask.Domain.BTree;

public interface IBTreeIndexComparer
{
    Task<int> Compare(BTreeIndex x, BTreeIndex y, CancellationToken cancellationToken);
}