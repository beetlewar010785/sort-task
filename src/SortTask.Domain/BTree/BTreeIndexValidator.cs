namespace SortTask.Domain.BTree;

public class BTreeIndexValidator<TNode, TIndex, TNodeId>(
    IBTreeStore<TNode, TIndex, TNodeId> store,
    IRowLookup<TIndex> rowLookup,
    IComparer<ReadRow> rowComparer
)
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IIndex
{
    public async Task Validate()
    {
        var root = await store.GetRoot();
        if (root == null)
        {
            return;
        }

        await CheckMinMax(root);
    }

    private async Task CheckMinMax(TNode node)
    {
        await CheckSiblings(node);

        if (node.Children.Count == 0)
        {
            return;
        }

        for (var i = 0; i < node.Indices.Count; i++)
        {
            var left = await store.GetNode(node.Children[i]);
            var right = await store.GetNode(node.Children[i + 1]);
            await CheckMinMax(node.Indices[i], left, right);
        }

        foreach (var childId in node.Children)
        {
            var child = await store.GetNode(childId);
            await CheckMinMax(child);
        }
    }

    private async Task CheckMinMax(
        TIndex currentIndex,
        TNode left,
        TNode right)
    {
        var currentRow = await rowLookup.FindRow(currentIndex);

        var leftMaxIndex = await GetRight(left);
        var leftMaxRow = await rowLookup.FindRow(leftMaxIndex);
        var isLeftGreater = rowComparer.Compare(leftMaxRow, currentRow);
        if (isLeftGreater > 0)
        {
            throw new Exception($"Left row {leftMaxRow} is greater than current {currentRow}.");
        }

        var rightMinIndex = await GetLeft(right);
        var rightMinRow = await rowLookup.FindRow(rightMinIndex);
        var isRightLess = rowComparer.Compare(rightMinRow, currentRow);
        if (isRightLess < 0)
        {
            throw new Exception($"Right row {rightMinRow} is less than current {currentRow}.");
        }
    }

    private async Task CheckSiblings(TNode node)
    {
        for (var i = 1; i < node.Indices.Count; i++)
        {
            var prev = node.Indices[i - 1];
            var curr = node.Indices[i];
            var prevRow = await rowLookup.FindRow(prev);
            var currRow = await rowLookup.FindRow(curr);
            var result = rowComparer.Compare(prevRow, currRow);
            if (result > 0)
            {
                throw new Exception($"Sibling index ${prev} is greater than ${curr}");
            }
        }
    }

    private async Task<TIndex> GetLeft(TNode node)
    {
        if (node.Children.Count == 0)
        {
            return node.Indices[0];
        }

        var left = await store.GetNode(node.Children[0]);
        return await GetLeft(left);
    }

    private async Task<TIndex> GetRight(TNode node)
    {
        if (node.Children.Count == 0)
        {
            return node.Indices[^1];
        }

        var right = await store.GetNode(node.Children[^1]);
        return await GetRight(right);
    }
}