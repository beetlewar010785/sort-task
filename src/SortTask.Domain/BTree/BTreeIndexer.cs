using System.Text;

namespace SortTask.Domain.BTree;

public class Indexer<TOphValue>(
    IBTreeStore<TOphValue> store,
    IBTreeIndexComparer<TOphValue> indexComparer,
    BTreeOrder order,
    IOph<TOphValue> oph,
    Encoding encoding
) : IIndexer
    where TOphValue : struct
{
    public async Task Index(Row row, long offset, int length, CancellationToken cancellationToken)
    {
        var sentenceBytes = encoding.GetBytes(row.Sentence);
        var ophHash = oph.Hash(sentenceBytes);
        var index = new BTreeIndex<TOphValue>(ophHash, offset, length);

        var rootId = await store.GetRoot(cancellationToken);
        if (rootId == null)
        {
            _ = await CreateNewRoot(index, new PositioningItems<long>([]), cancellationToken);
            return;
        }

        var root = await store.GetNode(rootId.Value, cancellationToken);
        var targetResult = await FindTarget(index, root, cancellationToken);
        await InserBTreeIndex(targetResult.Target, targetResult.Position, index, cancellationToken);
    }

    private async Task InserBTreeIndex(
        BTreeNode<TOphValue> targetBTreeNode,
        int position,
        BTreeIndex<TOphValue> index,
        CancellationToken cancellationToken)
    {
        var newIndices = targetBTreeNode.Indices.Insert(index, position);
        targetBTreeNode = new BTreeNode<TOphValue>(
            targetBTreeNode.Id,
            targetBTreeNode.ParentId,
            targetBTreeNode.Children,
            newIndices
        );
        await store.SaveNode(targetBTreeNode, cancellationToken);
        await CheckOverflow(targetBTreeNode, cancellationToken);
    }

    private async Task CheckOverflow(BTreeNode<TOphValue> node, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (!order.IsOverflowed(node.Indices.Length)) return;

            var splitResult = SplitNode(node);
            var rightId = await store.AllocateId(cancellationToken);

            BTreeNode<TOphValue>? parent = null;
            if (node.ParentId.HasValue) parent = await store.GetNode(node.ParentId.Value, cancellationToken);

            if (parent == null)
                // if the node does not have parent, it means it is root
                // then we need to assign new root
                parent = await CreateNewRoot(
                    splitResult.PopupIndex,
                    new PositioningItems<long>([node.Id, rightId]),
                    cancellationToken);
            else
                // when we have a parent, we add an index to the parent and created child node
                parent = await AddIndexAndNodeToParent(
                    parent.Value,
                    insertingNode: rightId,
                    insertAfterNode: node.Id,
                    insertingIndex: splitResult.PopupIndex,
                    cancellationToken: cancellationToken);

            await CreateOrUpdateNode(
                node.Id,
                parent.Value.Id,
                splitResult.LeftChildren,
                splitResult.LeftIndices,
                false,
                cancellationToken);

            await CreateOrUpdateNode(
                rightId,
                parent.Value.Id,
                splitResult.RightChildren,
                splitResult.RightIndices,
                true,
                cancellationToken);

            // repeat recursively until we reach the root or find a node that is not overflowed
            node = parent.Value;
        }
    }

    private static SplitBTreeNodeResult SplitNode(BTreeNode<TOphValue> node)
    {
        var midIndex = node.Indices.Length / 2;
        var midChild = node.Children.Length / 2;
        var popupIndex = node.Indices[midIndex];
        return new SplitBTreeNodeResult(
            new PositioningItems<long>(node.Children.Values.Take(midChild).ToArray()),
            new PositioningItems<BTreeIndex<TOphValue>>(node.Indices.Values.Take(midIndex).ToArray()),
            new PositioningItems<long>(node.Children.Values.Skip(midChild).ToArray()),
            new PositioningItems<BTreeIndex<TOphValue>>(node.Indices.Values.Skip(midIndex + 1).ToArray()),
            popupIndex
        );
    }

    private async Task<BTreeNode<TOphValue>> AddIndexAndNodeToParent(
        BTreeNode<TOphValue> parent,
        BTreeIndex<TOphValue> insertingIndex,
        long insertingNode,
        long insertAfterNode,
        CancellationToken cancellationToken)
    {
        var target = await FindTargetIn(insertingIndex, parent, cancellationToken);

        var insertingNodePosition = parent.Children.IndexOf(insertAfterNode) + 1;
        var newChildren = parent.Children.Insert(insertingNode, insertingNodePosition);

        var newIndices = parent.Indices.Insert(insertingIndex, target.Position);

        parent = new BTreeNode<TOphValue>(
            parent.Id,
            parent.ParentId,
            newChildren,
            newIndices
        );
        await store.SaveNode(parent, cancellationToken);

        return parent;
    }

    private async Task<BTreeNode<TOphValue>> CreateNewRoot(
        BTreeIndex<TOphValue> index,
        PositioningItems<long> children,
        CancellationToken cancellationToken)
    {
        var rootId = await store.AllocateId(cancellationToken);
        var root = new BTreeNode<TOphValue>(
            rootId,
            null,
            children,
            new PositioningItems<BTreeIndex<TOphValue>>([index])
        );
        await store.SaveNode(root, cancellationToken);
        await store.SetRoot(root.Id, cancellationToken);
        return root;
    }

    private async Task CreateOrUpdateNode(
        long nodeId,
        long newParentId,
        PositioningItems<long> newChildren,
        PositioningItems<BTreeIndex<TOphValue>> newIndices,
        bool childrenParentIdChanged,
        CancellationToken cancellationToken
    )
    {
        var node = new BTreeNode<TOphValue>(
            nodeId,
            newParentId,
            newChildren,
            newIndices
        );
        await store.SaveNode(node, cancellationToken);

        if (childrenParentIdChanged)
            // change parent id of the children to this node's id
            foreach (var childId in newChildren.Values)
            {
                var child = await store.GetNode(childId, cancellationToken);
                child = new BTreeNode<TOphValue>(
                    child.Id,
                    nodeId,
                    child.Children,
                    child.Indices
                );
                await store.SaveNode(child, cancellationToken);
            }
    }

    private async Task<FindTargetResult> FindTarget(BTreeIndex<TOphValue> insertingIndex,
        BTreeNode<TOphValue> searchInNode,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            if (searchInNode.Children.Length == 0)
                // this is a leaf node, no need to search further
                return await FindTargetIn(insertingIndex, searchInNode, cancellationToken);

            // search child where our index which is greater than our index to drill down
            for (var i = 0; i < searchInNode.Indices.Length; i++)
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

    private async Task<FindTargetResult> FindTargetIn(BTreeIndex<TOphValue> index, BTreeNode<TOphValue> target,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < target.Indices.Length; i++)
        {
            var existingIndex = target.Indices[i];
            var indexCompareResult = await indexComparer.Compare(index, existingIndex, cancellationToken);
            if (indexCompareResult <= 0)
                // our index is less than or equal to the other index, we found right place
                return new FindTargetResult(target, i);
        }

        // return the latest position
        return new FindTargetResult(target, target.Indices.Length);
    }

    private readonly struct FindTargetResult(BTreeNode<TOphValue> target, int position)
    {
        public BTreeNode<TOphValue> Target { get; } = target;
        public int Position { get; } = position;
    }

    // we cannot return BTreeNode<TOphValue> because we do not have parent id yet
    // parent may be created based on the result of this method
    private readonly struct SplitBTreeNodeResult(
        PositioningItems<long> leftChildren,
        PositioningItems<BTreeIndex<TOphValue>> leftIndices,
        PositioningItems<long> rightChildren,
        PositioningItems<BTreeIndex<TOphValue>> rightIndices,
        BTreeIndex<TOphValue> popupIndex
    )
    {
        public PositioningItems<long> LeftChildren { get; } = leftChildren;
        public PositioningItems<BTreeIndex<TOphValue>> LeftIndices { get; } = leftIndices;
        public PositioningItems<long> RightChildren { get; } = rightChildren;
        public PositioningItems<BTreeIndex<TOphValue>> RightIndices { get; } = rightIndices;
        public BTreeIndex<TOphValue> PopupIndex { get; } = popupIndex;
    }
}
