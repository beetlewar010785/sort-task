using System.Runtime.CompilerServices;

namespace SortTask.Domain.BTree;

public class BTreeIndexTraverser<TOphValue>(IBTreeStore<TOphValue> store) : IBTreeIndexTraverser<TOphValue>
    where TOphValue : struct
{
    public async IAsyncEnumerable<BTreeIndex<TOphValue>> IterateAsAsyncEnumerable(
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

    private async IAsyncEnumerable<BTreeIndex<TOphValue>> IterateAsAsyncEnumerable(
        BTreeNode<TOphValue> node,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (var i = 0; i < node.Indices.Length + 1; i++) // +1 - for children traverse
        {
            // 1. Return children of this index (left) or the rightmost for the last index
            if (node.Children.Length > 0)
            {
                var childNode = await store.GetNode(node.Children[i], cancellationToken);
                await foreach (var childIndex in IterateAsAsyncEnumerable(childNode, cancellationToken))
                {
                    yield return childIndex;
                }
            }

            // 2. Return index itself
            if (i >= node.Indices.Length) continue;

            var index = node.Indices[i];
            yield return index;
        }
    }
}