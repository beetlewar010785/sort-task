namespace SortTask.Domain.BTree;

public interface IBTreeIndexTraverser<TOphValue>
    where TOphValue : struct
{
    IEnumerable<BTreeIndex<TOphValue>> IterateOverIndex();
}
