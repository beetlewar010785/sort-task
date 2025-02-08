namespace SortTask.Domain.BTree;

public class BTreeIndexValidator<TNode, TIndex, TNodeId>(
    IBTreeStore<TNode, TIndex, TNodeId> store,
    IRowLookup<TIndex> rowLookup,
    IComparer<ReadRow> rowComparer
)
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IIndex
{
    public async Task Validate(CancellationToken cancellationToken)
    {
        var root = await store.GetRoot(cancellationToken);
        if (root == null)
        {
            return;
        }

        await CheckMinMax(root, cancellationToken);
    }

    private async Task CheckMinMax(TNode node, CancellationToken cancellationToken)
    {
        await CheckSiblings(node, cancellationToken);

        if (node.Children.Count == 0)
        {
            return;
        }

        for (var i = 0; i < node.Indices.Count; i++)
        {
            var left = await store.GetNode(node.Children[i], cancellationToken);
            var right = await store.GetNode(node.Children[i + 1], cancellationToken);
            await CheckMinMax(node.Indices[i], left, right, cancellationToken);
        }

        foreach (var childId in node.Children)
        {
            var child = await store.GetNode(childId, cancellationToken);
            await CheckMinMax(child, cancellationToken);
        }
    }

    private async Task CheckMinMax(
        TIndex currentIndex,
        TNode left,
        TNode right,
        CancellationToken cancellationToken)
    {
        var currentRow = await rowLookup.FindRow(currentIndex, cancellationToken);

        var leftMaxIndex = await GetRight(left, cancellationToken);
        var leftMaxRow = await rowLookup.FindRow(leftMaxIndex, cancellationToken);
        var isLeftGreater = rowComparer.Compare(leftMaxRow, currentRow);
        if (isLeftGreater > 0)
        {
            throw new Exception($"Left row {leftMaxRow} is greater than current {currentRow}.");
        }

        var rightMinIndex = await GetLeft(right, cancellationToken);
        var rightMinRow = await rowLookup.FindRow(rightMinIndex, cancellationToken);
        var isRightLess = rowComparer.Compare(rightMinRow, currentRow);
        if (isRightLess < 0)
        {
            throw new Exception($"Right row {rightMinRow} is less than current {currentRow}.");
        }
    }

    private async Task CheckSiblings(TNode node, CancellationToken cancellationToken)
    {
        for (var i = 1; i < node.Indices.Count; i++)
        {
            var prev = node.Indices[i - 1];
            var curr = node.Indices[i];
            var prevRow = await rowLookup.FindRow(prev, cancellationToken);
            var currRow = await rowLookup.FindRow(curr, cancellationToken);
            var result = rowComparer.Compare(prevRow, currRow);
            if (result > 0)
            {
                throw new Exception($"Sibling index ${prev} is greater than ${curr}");
            }
        }
    }

    private async Task<TIndex> GetLeft(TNode node, CancellationToken cancellationToken)
    {
        if (node.Children.Count == 0)
        {
            return node.Indices[0];
        }

        var left = await store.GetNode(node.Children[0], cancellationToken);
        return await GetLeft(left, cancellationToken);
    }

    private async Task<TIndex> GetRight(TNode node, CancellationToken cancellationToken)
    {
        if (node.Children.Count == 0)
        {
            return node.Indices[^1];
        }

        var right = await store.GetNode(node.Children[^1], cancellationToken);
        return await GetRight(right, cancellationToken);
    }
}