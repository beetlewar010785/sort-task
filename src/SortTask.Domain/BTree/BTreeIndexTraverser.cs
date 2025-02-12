using System.Runtime.CompilerServices;

namespace SortTask.Domain.BTree;

public class BTreeIndexTraverser(IBTreeStore store) : IBTreeIndexTraverser
{
    public async IAsyncEnumerable<BTreeIndex> IterateAsAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var rootId = await store.GetRoot(cancellationToken);
        if (rootId == null) yield break;

        var root = await store.GetNode(rootId.Value, cancellationToken);

        await foreach (var index in IterateAsAsyncEnumerable(root, cancellationToken))
        {
            yield return index;
        }
    }

    private async IAsyncEnumerable<BTreeIndex> IterateAsAsyncEnumerable(
        BTreeNode node,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < node.Indices.Count + 1; i++) // +1 - for children traverse
        {
            // 1. Return children of this index (left) or the rightmost for the last index
            if (node.Children.Count > 0)
            {
                var childNode = await store.GetNode(node.Children[i], cancellationToken);
                await foreach (var childIndex in IterateAsAsyncEnumerable(childNode, cancellationToken))
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