namespace SortTask.Domain.BTree;

public interface IBTreeIndexFactory<out TIndex> where TIndex : IBTreeIndex
{
    public TIndex CreateIndex(RowIndexKey key);
}