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
            await CreateNewRoot(index, []);
            return;
        }

        var targetResult = await FindTarget(index, root);
        await InsertIndex(targetResult.Target, targetResult.Position, index);
    }

    private async Task InsertIndex(TNode targetNode, int position, TIndex index)
    {
        var newIndices = targetNode.Indices.ToList();
        newIndices.Insert(position, index);
        targetNode = nodeFactory.Create(
            targetNode.Id,
            targetNode.ParentId,
            targetNode.Children,
            newIndices
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

        var splitResult = SplitNode(node);
        var rightNodeId = await store.AllocateId();

        TNode? parent = default;
        if (node.ParentId != null)
        {
            parent = await store.GetNode(node.ParentId);
        }

        if (parent == null)
        {
            // if the node does not have parent, it means it is root
            // then we need to assign new root
            parent = await CreateNewRoot(
                splitResult.PopupIndex,
                [node.Id, rightNodeId]
            );
        }
        else
        {
            // when we have a parent, we add an index to the parent and created child node
            parent = await AddIndexAndNodeToParent(
                parent: parent,
                insertingNode: rightNodeId,
                insertNodeAfter: node.Id,
                insertingIndex: splitResult.PopupIndex);
        }

        await CreateOrUpdateNode(
            nodeId: node.Id,
            newParentId: parent.Id,
            newChildren: splitResult.LeftChildren,
            newIndices: splitResult.LeftIndices,
            childrenParentIdChanged: false
        );

        await CreateOrUpdateNode(
            nodeId: rightNodeId,
            newParentId: parent.Id,
            newChildren: splitResult.RightChildren,
            newIndices: splitResult.RightIndices,
            childrenParentIdChanged: true
        );

        // repeat recursively until we reach the root or find a node that is not overflowed
        await CheckOverflow(parent);
    }

    private SplitNodeResult SplitNode(TNode node)
    {
        var midIndex = node.Indices.Count / 2;
        var midChild = node.Children.Count / 2;
        var popupIndex = node.Indices[midIndex];
        return new SplitNodeResult(
            node.Children.Take(midChild).ToList(),
            node.Indices.Take(midIndex).ToList(),
            node.Children.Skip(midChild).ToList(),
            node.Indices.Skip(midIndex + 1).ToList(),
            popupIndex
        );
    }

    private async Task<TNode> AddIndexAndNodeToParent(
        TNode parent,
        TIndex insertingIndex,
        TNodeId insertingNode,
        TNodeId insertNodeAfter
    )
    {
        var target = await FindTargetIn(insertingIndex, parent);

        var newChildren = parent.Children.ToList();
        var insertingNodePosition = newChildren.IndexOf(insertNodeAfter) + 1;
        newChildren.Insert(insertingNodePosition, insertingNode);

        var newIndices = parent.Indices.ToList();
        newIndices.Insert(target.Position, insertingIndex);

        parent = nodeFactory.Create(
            parent.Id,
            parent.ParentId,
            newChildren,
            newIndices
        );
        await store.SaveNode(parent);

        return parent;
    }

    private async Task<TNode> CreateNewRoot(TIndex index, IReadOnlyList<TNodeId> children)
    {
        var parentId = await store.AllocateId();
        var parent = nodeFactory.Create(
            parentId,
            default,
            children,
            [index]
        );
        await store.SaveNode(parent);
        await store.SetRoot(parent.Id);
        return parent;
    }

    private async Task CreateOrUpdateNode(
        TNodeId nodeId,
        TNodeId newParentId,
        IReadOnlyList<TNodeId> newChildren,
        IReadOnlyList<TIndex> newIndices,
        bool childrenParentIdChanged
    )
    {
        var node = nodeFactory.Create(
            nodeId,
            newParentId,
            newChildren,
            newIndices
        );
        await store.SaveNode(node);

        if (childrenParentIdChanged)
        {
            // change parent id of the children     to this node's id
            foreach (var childId in newChildren)
            {
                var child = await store.GetNode(childId);
                child = nodeFactory.Create(
                    child.Id,
                    nodeId,
                    child.Children,
                    child.Indices
                );
                await store.SaveNode(child);
            }
        }
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

    private record FindTargetResult(TNode Target, int Position);

    // we cannot return TNode because we do not have parent id yet
    // parent may be created based on the result of this method
    private record SplitNodeResult(
        IReadOnlyList<TNodeId> LeftChildren,
        IReadOnlyList<TIndex> LeftIndices,
        IReadOnlyList<TNodeId> RightChildren,
        IReadOnlyList<TIndex> RightIndices,
        TIndex PopupIndex
    );
}