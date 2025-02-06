namespace SortTask.Domain.BTree;

public class BTreeIndexTraverser<TNode, TIndex, TNodeId>(
    IBTreeStore<TNode, TIndex, TNodeId> store
) : IIndexTraverser<TIndex>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IIndex
{
    public async IAsyncEnumerable<TIndex> Traverse()
    {
        var root = await store.GetRoot();
        if (root == null) yield break;

        await foreach (var index in Traverse(root))
        {
            yield return index;
        }
    }

    private async IAsyncEnumerable<TIndex> Traverse(TNode node)
    {
        for (var i = 0; i < node.Indices.Count + 1; i++) // +1 - for children traverse
        {
            // 1. Return children of this index (left) or the rightmost for the last index
            if (node.Children.Count > 0)
            {
                var childNode = await store.GetNode(node.Children[i]);
                await foreach (var childIndex in Traverse(childNode))
                {
                    yield return childIndex;
                }
            }

            // 2. Return index itself
            if (i >= node.Indices.Count) continue;

            var index = node.Indices[i];
            yield return index;
        }
    }
}