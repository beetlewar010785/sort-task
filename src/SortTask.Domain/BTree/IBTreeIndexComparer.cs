namespace SortTask.Domain.BTree;

public interface IBTreeIndexComparer<in TIndex>
    where TIndex : IIndex
{
    Task<int> Compare(TIndex xIndex, TIndex yIndex);
}