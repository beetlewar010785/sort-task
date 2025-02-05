using System.Collections;

namespace SortTask.Domain.BTree;

public class BTreeIndexCollection<TIndex>(List<TIndex> indices) : IEnumerable<TIndex>
    where TIndex : IIndex
{
    public int Count => indices.Count;

    public static BTreeIndexCollection<TIndex> Empty => new([]);

    public BTreeIndexCollection<TIndex> InsertAt(int position, TIndex index)
    {
        indices.Insert(position, index);
        return this;
    }

    public TIndex this[int index] => indices[index];

    public IEnumerator<TIndex> GetEnumerator() => indices.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => indices.GetEnumerator();
}