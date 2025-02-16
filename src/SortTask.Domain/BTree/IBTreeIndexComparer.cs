namespace SortTask.Domain.BTree;

public interface IBTreeIndexComparer<TOphValue>
    where TOphValue : struct
{
    int Compare(Row xRow, BTreeIndex<TOphValue> x, BTreeIndex<TOphValue> y);
}
