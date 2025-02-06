namespace SortTask.Domain.BTree;

public class BTreeIndexer<TNode, TIndex, TNodeId>(
    IBTreeStore<TNode, TIndex, TNodeId> store,
    IBTreeNodeFactory<TNode, TIndex, TNodeId> nodeFactory,
    IBTreeIndexComparer<TIndex> indexComparer,
    BTreeOrder order
) : IIndexer<TIndex>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IIndex
{
    public async Task Index(TIndex index)
    {
        var root = await store.GetRoot();
        if (root == null)
        {
            var rootId = await store.AllocateId();
            root = nodeFactory.Create(
                rootId,
                default,
                BTreeNodeCollection<TNodeId>.Empty,
                new BTreeIndexCollection<TIndex>([index])
            );
            await store.SaveNode(root);
            await store.SetRoot(root.Id);
            return;
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
        await store.SaveNode(targetNode);
        await CheckOverflow(targetNode);
    }

    private async Task CheckOverflow(TNode node)
    {
        if (!order.IsOverflowed(node.Indices.Count))
        {
            return;
        }

        var splitResult = await SplitNode(node);
        TNode? parent = default;
        if (node.ParentId != null)
        {
            parent = await store.GetNode(node.ParentId);
        }

        if (parent == null)
        {
            // if the node does not have parent, it means it is root
            // then we need to assign new root
            var parentId = await store.AllocateId();
            parent = nodeFactory.Create(
                parentId,
                default,
                new BTreeNodeCollection<TNodeId>([splitResult.Left, splitResult.Right]),
                new BTreeIndexCollection<TIndex>([splitResult.PopupIndex])
            );
            await store.SaveNode(parent);
            await store.SetRoot(parent.Id);
        }
        else
        {
            // when we have a parent, we add an index to the parent and created child node
            var target = await FindTargetIn(splitResult.PopupIndex, parent);
            parent = nodeFactory.Create(
                parent.Id,
                parent.ParentId,
                parent.Children.InsertAfter(inserting: splitResult.Right, after: node.Id),
                parent.Indices.InsertAt(target.Position, splitResult.PopupIndex)
            );
            await store.SaveNode(parent);
        }

        // do not forget to save updated node and created one
        var leftNode = nodeFactory.Create(
            splitResult.Left,
            parent.Id,
            splitResult.LeftChildren,
            splitResult.LeftIndices
        );
        await store.SaveNode(leftNode);

        var rightNode = nodeFactory.Create(
            splitResult.Right,
            parent.Id,
            splitResult.RightChildren,
            splitResult.RightIndices
        );
        await store.SaveNode(rightNode);

        // repeat recursively until we reach the root or find a node that is not overflowed
        await CheckOverflow(parent);
    }

    private async Task<FindTargetResult> FindTarget(TIndex insertingIndex, TNode searchInNode)
    {
        if (searchInNode.Children.Count == 0)
        {
            // this is a leaf node, no need to search further
            return await FindTargetIn(insertingIndex, searchInNode);
        }

        // search child where our index which is greater than our index to drill down
        for (var i = 0; i < searchInNode.Indices.Count; i++)
        {
            var existingIndex = searchInNode.Indices[i];
            var indexCompareResult = await indexComparer.Compare(insertingIndex, existingIndex);
            if (indexCompareResult <= 0)
            {
                var nodeId = searchInNode.Children[i];
                var node = await store.GetNode(nodeId);
                return await FindTarget(insertingIndex, node);
            }
        }

        // our index is the greatest in the node, search in the rightmost child 
        var latestNodeId = searchInNode.Children[^1];
        var latestNode = await store.GetNode(latestNodeId);
        return await FindTarget(insertingIndex, latestNode);
    }

    private async Task<FindTargetResult> FindTargetIn(TIndex index, TNode target)
    {
        for (var i = 0; i < target.Indices.Count; i++)
        {
            var existingIndex = target.Indices[i];
            var indexCompareResult = await indexComparer.Compare(index, existingIndex);
            if (indexCompareResult <= 0)
            {
                // our index is less than or equal to the other index, we found right place
                return new FindTargetResult(target, i);
            }
        }

        // return the latest position
        return new FindTargetResult(target, target.Indices.Count);
    }

    private async Task<SplitNodeResult> SplitNode(TNode node)
    {
        var midIndex = node.Indices.Count / 2;
        var midChild = node.Children.Count / 2;
        var popupIndex = node.Indices[midIndex];
        var rightNodeId = await store.AllocateId();
        return new SplitNodeResult(
            node.Id,
            new BTreeNodeCollection<TNodeId>(node.Children.Take(midChild).ToList()),
            new BTreeIndexCollection<TIndex>(node.Indices.Take(midIndex).ToList()),
            rightNodeId,
            new BTreeNodeCollection<TNodeId>(node.Children.Skip(midChild).ToList()),
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