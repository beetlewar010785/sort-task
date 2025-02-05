namespace SortTask.Domain.BTree;

public class BTreeIndexer<TNode, TIndex, TNodeId>(
    IBTreeReadWriter<TNode, TIndex, TNodeId> readWriter,
    IBTreeNodeFactory<TNode, TIndex, TNodeId> nodeFactory,
    IBTreeIndexComparer<TIndex> indexComparer,
    BTreeOrder order
) : IIndexer<TIndex>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IIndex
{
    public async Task Index(TIndex index)
    {
        var root = await readWriter.GetRoot();
        if (root == null)
        {
            var rootId = await readWriter.AllocateId();
            root = nodeFactory.Create(rootId,
                default,
                BTreeNodeCollection<TNodeId>.Empty,
                BTreeIndexCollection<TIndex>.Empty
            );
            await readWriter.SaveNode(root);
            await readWriter.SetRoot(root.Id);
        }

        var targetResult = await FindTarget(index, root);
        await InsertIndex(targetResult.Target, targetResult.Position, index);
    }

    private async Task InsertIndex(TNode targetNode, int position, TIndex index)
    {
        targetNode = nodeFactory.Create(
            targetNode.Id,
            targetNode.ParentId,
            targetNode.Children,
            targetNode.Indices.InsertAt(position, index)
        );
        await readWriter.SaveNode(targetNode);
        await CheckOverflow(targetNode);
    }

    private async Task CheckOverflow(TNode node)
    {
        if (!IsOverflowed(node))
        {
            return;
        }

        var splitResult = await SplitNode(node);
        TNode? parent = default;
        if (node.ParentId != null)
        {
            parent = await readWriter.GetNode(node.ParentId);
        }

        if (parent == null)
        {
            // if the node does not have parent, it means it is root
            // then we need to assign new root
            var parentId = await readWriter.AllocateId();
            parent = nodeFactory.Create(
                parentId,
                default,
                new BTreeNodeCollection<TNodeId>([splitResult.Left, splitResult.Right]),
                new BTreeIndexCollection<TIndex>([splitResult.PopupIndex])
            );
            await readWriter.SaveNode(parent);
            await readWriter.SetRoot(parent.Id);
        }
        else
        {
            // when we have a parent, we add an index to the parent and created child node
            parent = nodeFactory.Create(
                parent.Id,
                parent.ParentId,
                parent.Children.InsertAfter(inserting: splitResult.Right, after: node.Id),
                parent.Indices.InsertAt(0, splitResult.PopupIndex) // todo
            );
            await readWriter.SaveNode(parent);
        }

        // do not forget to save updated node and created one
        var leftNode = nodeFactory.Create(
            splitResult.Left,
            parent.Id,
            splitResult.LeftChildren,
            splitResult.LeftIndices
        );
        await readWriter.SaveNode(leftNode);

        var rightNode = nodeFactory.Create(
            splitResult.Right,
            parent.Id,
            splitResult.RightChildren,
            splitResult.RightIndices
        );
        await readWriter.SaveNode(rightNode);

        // repeat recursively until we reach the root or find a node that is not overflowed
        await CheckOverflow(parent);
    }

    private bool IsOverflowed(TNode node)
    {
        return node.Indices.Count > order.Value * 2;
    }

    private async Task<FindTargetResult> FindTarget(TIndex index, TNode searchInNode)
    {
        if (searchInNode.Children.Count == 0)
        {
            // this is a leaf node, no need to search further
            for (var i = 0; i < searchInNode.Indices.Count; i++)
            {
                var otherIndex = searchInNode.Indices[i];
                var indexCompareResult = await indexComparer.Compare(index, otherIndex);
                if (indexCompareResult <= 0)
                {
                    // our index is less than or equal to the other index, we found right place
                    return new FindTargetResult(searchInNode, i);
                }
            }

            // insert at the end
            return new FindTargetResult(searchInNode, searchInNode.Indices.Count);
        }

        for (var i = 0; i < searchInNode.Indices.Count; i++)
        {
            var otherIndex = searchInNode.Indices[i];
            var indexCompareResult = await indexComparer.Compare(index, otherIndex);
            if (indexCompareResult <= 0)
            {
                var nodeId = searchInNode.Children[i];
                var node = await readWriter.GetNode(nodeId);
                return await FindTarget(index, node);
            }
        }

        var latestNodeId = searchInNode.Children[^1];
        var latestNode = await readWriter.GetNode(latestNodeId);
        return await FindTarget(index, latestNode);
    }

    private async Task<SplitNodeResult> SplitNode(TNode node)
    {
        var midIndex = node.Indices.Count / 2;
        var halfChildren = node.Children.Count / 2;
        var popupIndex = node.Indices[midIndex];
        var rightNodeId = await readWriter.AllocateId();
        return new SplitNodeResult(
            node.Id,
            new BTreeNodeCollection<TNodeId>(node.Children.Take(halfChildren).ToList()),
            new BTreeIndexCollection<TIndex>(node.Indices.Take(midIndex).ToList()),
            rightNodeId,
            new BTreeNodeCollection<TNodeId>(node.Children.Skip(halfChildren).ToList()),
            new BTreeIndexCollection<TIndex>(node.Indices.Skip(midIndex + 1).ToList()),
            popupIndex
        );
    }

    private record FindTargetResult(TNode Target, int Position);

    // we cannot return TNode because we do not have parent id yet
    // parent may be created based on the result of this method
    private record SplitNodeResult(
        TNodeId Left,
        BTreeNodeCollection<TNodeId> LeftChildren,
        BTreeIndexCollection<TIndex> LeftIndices,
        TNodeId Right,
        BTreeNodeCollection<TNodeId> RightChildren,
        BTreeIndexCollection<TIndex> RightIndices,
        TIndex PopupIndex
    );
}