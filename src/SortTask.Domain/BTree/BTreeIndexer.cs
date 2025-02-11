namespace SortTask.Domain.BTree;

public class Indexer(
    IBTreeStore store,
    IBTreeIndexComparer indexComparer,
    BTreeOrder order
) : IIndexer
{
    public async Task Index(OphULong oph, long offset, int length, CancellationToken cancellationToken)
    {
        var root = await store.GetRoot(cancellationToken);

        var index = new BTreeIndex(oph, offset, length);
        if (root == null)
        {
            await CreateNewRoot(index, [], cancellationToken);
            return;
        }

        var targetResult = await FindTarget(index, root, cancellationToken);
        await InserBTreeIndex(targetResult.Target, targetResult.Position, index, cancellationToken);
    }

    private async Task InserBTreeIndex(BTreeNode targeBTreeNode, int position, BTreeIndex index,
        CancellationToken cancellationToken)
    {
        var newIndices = targeBTreeNode.Indices.ToList();
        newIndices.Insert(position, index);
        targeBTreeNode = new BTreeNode(
            targeBTreeNode.Id,
            targeBTreeNode.ParentId,
            targeBTreeNode.Children,
            newIndices
        );
        await store.SaveNode(targeBTreeNode, cancellationToken);
        await CheckOverflow(targeBTreeNode, cancellationToken);
    }

    private async Task CheckOverflow(BTreeNode node, CancellationToken cancellationToken)
    {
        if (!order.IsOverflowed(node.Indices.Count))
        {
            return;
        }

        var splitResult = SpliBTreeNode(node);
        var rightId = await store.AllocateId(cancellationToken);

        BTreeNode? parent = null;
        if (node.ParentId.HasValue)
        {
            parent = await store.GetNode(node.ParentId.Value, cancellationToken);
        }

        if (parent == null)
        {
            // if the node does not have parent, it means it is root
            // then we need to assign new root
            parent = await CreateNewRoot(
                splitResult.PopupIndex,
                [node.Id, rightId],
                cancellationToken
            );
        }
        else
        {
            // when we have a parent, we add an index to the parent and created child node
            parent = await AddIndexAndNodeToParent(
                parent: parent,
                insertingNode: rightId,
                inserBTreeNodeAfter: node.Id,
                insertingIndex: splitResult.PopupIndex,
                cancellationToken: cancellationToken);
        }

        await CreateOrUpdateNode(
            nodeId: node.Id,
            newParentId: parent.Id,
            newChildren: splitResult.LeftChildren,
            newIndices: splitResult.LeftIndices,
            childrenParentIdChanged: false,
            cancellationToken
        );

        await CreateOrUpdateNode(
            nodeId: rightId,
            newParentId: parent.Id,
            newChildren: splitResult.RightChildren,
            newIndices: splitResult.RightIndices,
            childrenParentIdChanged: true,
            cancellationToken
        );

        // repeat recursively until we reach the root or find a node that is not overflowed
        await CheckOverflow(parent, cancellationToken);
    }

    private SpliBTreeNodeResult SpliBTreeNode(BTreeNode node)
    {
        var midIndex = node.Indices.Count / 2;
        var midChild = node.Children.Count / 2;
        var popupIndex = node.Indices[midIndex];
        return new SpliBTreeNodeResult(
            node.Children.Take(midChild).ToList(),
            node.Indices.Take(midIndex).ToList(),
            node.Children.Skip(midChild).ToList(),
            node.Indices.Skip(midIndex + 1).ToList(),
            popupIndex
        );
    }

    private async Task<BTreeNode> AddIndexAndNodeToParent(
        BTreeNode parent,
        BTreeIndex insertingIndex,
        long insertingNode,
        long inserBTreeNodeAfter,
        CancellationToken cancellationToken)
    {
        var target = await FindTargetIn(insertingIndex, parent, cancellationToken);

        var newChildren = parent.Children.ToList();
        var insertingNodePosition = newChildren.IndexOf(inserBTreeNodeAfter) + 1;
        newChildren.Insert(insertingNodePosition, insertingNode);

        var newIndices = parent.Indices.ToList();
        newIndices.Insert(target.Position, insertingIndex);

        parent = new BTreeNode(
            parent.Id,
            parent.ParentId,
            newChildren,
            newIndices
        );
        await store.SaveNode(parent, cancellationToken);

        return parent;
    }

    private async Task<BTreeNode> CreateNewRoot(
        BTreeIndex index,
        IReadOnlyList<long> children,
        CancellationToken cancellationToken)
    {
        var parentId = await store.AllocateId(cancellationToken);
        var parent = new BTreeNode(
            parentId,
            null,
            children,
            [index]
        );
        await store.SaveNode(parent, cancellationToken);
        await store.SetRoot(parent.Id, cancellationToken);
        return parent;
    }

    private async Task CreateOrUpdateNode(
        long nodeId,
        long newParentId,
        IReadOnlyList<long> newChildren,
        IReadOnlyList<BTreeIndex> newIndices,
        bool childrenParentIdChanged,
        CancellationToken cancellationToken
    )
    {
        var node = new BTreeNode(
            nodeId,
            newParentId,
            newChildren,
            newIndices
        );
        await store.SaveNode(node, cancellationToken);

        if (childrenParentIdChanged)
        {
            // change parent id of the children     to this node's id
            foreach (var childId in newChildren)
            {
                var child = await store.GetNode(childId, cancellationToken);
                child = new BTreeNode(
                    child.Id,
                    nodeId,
                    child.Children,
                    child.Indices
                );
                await store.SaveNode(child, cancellationToken);
            }
        }
    }

    private async Task<FindTargetResult> FindTarget(BTreeIndex insertingIndex, BTreeNode searchInNode,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            if (searchInNode.Children.Count == 0)
            {
                // this is a leaf node, no need to search further
                return await FindTargetIn(insertingIndex, searchInNode, cancellationToken);
            }

            // search child where our index which is greater than our index to drill down
            for (var i = 0; i < searchInNode.Indices.Count; i++)
            {
                var existingIndex = searchInNode.Indices[i];
                var indexCompareResult = await indexComparer.Compare(insertingIndex, existingIndex, cancellationToken);
                if (indexCompareResult > 0) continue;

                var nodeId = searchInNode.Children[i];
                var node = await store.GetNode(nodeId, cancellationToken);
                return await FindTarget(insertingIndex, node, cancellationToken);
            }

            // our index is the greatest in the node, search in the rightmost child 
            var latestNodeId = searchInNode.Children[^1];
            var latestNode = await store.GetNode(latestNodeId, cancellationToken);
            searchInNode = latestNode;
        }
    }

    private async Task<FindTargetResult> FindTargetIn(BTreeIndex index, BTreeNode target,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < target.Indices.Count; i++)
        {
            var existingIndex = target.Indices[i];
            var indexCompareResult = await indexComparer.Compare(index, existingIndex, cancellationToken);
            if (indexCompareResult <= 0)
            {
                // our index is less than or equal to the other index, we found right place
                return new FindTargetResult(target, i);
            }
        }

        // return the latest position
        return new FindTargetResult(target, target.Indices.Count);
    }

    private record FindTargetResult(BTreeNode Target, int Position);

    // we cannot return BTreeNode because we do not have parent id yet
    // parent may be created based on the result of this method
    private record SpliBTreeNodeResult(
        IReadOnlyList<long> LeftChildren,
        IReadOnlyList<BTreeIndex> LeftIndices,
        IReadOnlyList<long> RightChildren,
        IReadOnlyList<BTreeIndex> RightIndices,
        BTreeIndex PopupIndex
    );
}