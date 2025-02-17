namespace SortTask.Domain.BTree;

public class BTreeIndexTraverser<TOphValue>(IBTreeStore<TOphValue> store) : IBTreeIndexTraverser<TOphValue>
    where TOphValue : struct
{
    public IEnumerable<BTreeIndex<TOphValue>> IterateOverIndex()
    {
        var rootId = store.GetRoot();
        if (rootId == null) yield break;

        var root = store.GetNode(rootId.Value);

        foreach (var index in IterateOverIndex(root))
            yield return index;
    }

    private IEnumerable<BTreeIndex<TOphValue>> IterateOverIndex(BTreeNode<TOphValue> node)
    {
        for (var i = 0; i < node.Indices.Length + 1; i++) // +1 - for children traverse
        {
            // 1. Return children of this index (left) or the rightmost for the last index
            if (node.Children.Length > 0)
            {
                var childNode = store.GetNode(node.Children[i]);
                foreach (var childIndex in IterateOverIndex(childNode))
                    yield return childIndex;
            }

            // 2. Return index itself
            if (i >= node.Indices.Length) continue;

            var index = node.Indices[i];
            yield return index;
        }
    }
}
