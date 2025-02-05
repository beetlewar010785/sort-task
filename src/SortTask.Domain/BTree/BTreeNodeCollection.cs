using System.Collections;

namespace SortTask.Domain.BTree;

public class BTreeNodeCollection<TNodeId>(List<TNodeId> nodes) : IEnumerable<TNodeId>
{
    public int Count => nodes.Count;

    public static BTreeNodeCollection<TNodeId> Empty => new([]);

    public BTreeNodeCollection<TNodeId> InsertAfter(TNodeId inserting, TNodeId after)
    {
        var index = nodes.IndexOf(after);
        var newNodes = new List<TNodeId>(nodes);
        newNodes.Insert(index, inserting);
        return new BTreeNodeCollection<TNodeId>(newNodes);
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