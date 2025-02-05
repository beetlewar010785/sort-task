using System.Collections;

namespace SortTask.Domain.BTree;

public class BTreeNodeCollection<TNodeId>(List<TNodeId> nodes) : IEnumerable<TNodeId>
{
    public int Count => nodes.Count;

    public TNodeId this[int index] => nodes[index];

    public static BTreeNodeCollection<TNodeId> Empty => new([]);

    public BTreeNodeCollection<TNodeId> InsertAfter(TNodeId inserting, TNodeId after)
    {
        var position = nodes.IndexOf(after);
        nodes.Insert(position + 1, inserting);
        return this;
    }

    public IEnumerator<TNodeId> GetEnumerator()
    {
        return nodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return nodes.GetEnumerator();
    }
}