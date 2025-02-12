using System.Text;

namespace SortTask.Domain.BTree;

public class Indexer(
    IBTreeStore store,
    IBTreeIndexComparer indexComparer,
    BTreeOrder order,
    IOph oph,
    Encoding encoding
) : IIndexer
{
    public async Task Index(Row row, long offset, int length, CancellationToken cancellationToken)
    {
        var sentenceBytes = encoding.GetBytes(row.Sentence);
        var ophHash = oph.Hash(sentenceBytes);
        var index = new BTreeIndex(ophHash, offset, length);

        var rootId = await store.GetRoot(cancellationToken);
        if (rootId == null)
        {
            await CreateNewRoot(index, new PositioningCollection<long>([]), cancellationToken);
            return;
        }

        var root = await store.GetNode(rootId.Value, cancellationToken);
        var targetResult = await FindTarget(index, root, cancellationToken);
        await InserBTreeIndex(targetResult.Target, targetResult.Position, index, cancellationToken);
    }

    private async Task InserBTreeIndex(
        BTreeNode targetBTreeNode,
        int position,
        BTreeIndex index,
        CancellationToken cancellationToken)
    {
        var newIndices = targetBTreeNode.Indices.Insert(index, position);
        targetBTreeNode = new BTreeNode(
            targetBTreeNode.Id,
            targetBTreeNode.ParentId,
            targetBTreeNode.Children,
            newIndices
        );
        await store.SaveNode(targetBTreeNode, cancellationToken);
        await CheckOverflow(targetBTreeNode, cancellationToken);
    }

    private async Task CheckOverflow(BTreeNode node, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (!order.IsOverflowed(node.Indices.Length))
            {
                return;
            }

            var splitResult = SplitNode(node);
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
                    new PositioningCollection<long>([node.Id, rightId]),
                    cancellationToken);
            }
            else
            {
                // when we have a parent, we add an index to the parent and created child node
                parent = await AddIndexAndNodeToParent(
                    parent: parent.Value,
                    insertingNode: rightId,
                    insertAfterNode: node.Id,
                    insertingIndex: splitResult.PopupIndex,
                    cancellationToken: cancellationToken);
            }

            await CreateOrUpdateNode(
                nodeId: node.Id,
                newParentId: parent.Value.Id,
                newChildren: splitResult.LeftChildren, 
                newIndices: splitResult.LeftIndices,
                childrenParentIdChanged: false, 
                cancellationToken);

            await CreateOrUpdateNode(
                nodeId: rightId,
                newParentId: parent.Value.Id,
                newChildren: splitResult.RightChildren,
                newIndices: splitResult.RightIndices,
                childrenParentIdChanged: true, 
                cancellationToken);

            // repeat recursively until we reach the root or find a node that is not overflowed
            node = parent.Value;
        }
    }

    private static SplitBTreeNodeResult SplitNode(BTreeNode node)
    {
        var midIndex = node.Indices.Length / 2;
        var midChild = node.Children.Length / 2;
        var popupIndex = node.Indices[midIndex];
        return new SplitBTreeNodeResult(
            new PositioningCollection<long>(node.Children.Values.Take(midChild).ToArray()),
            new PositioningCollection<BTreeIndex>(node.Indices.Values.Take(midIndex).ToArray()),
            new PositioningCollection<long>(node.Children.Values.Skip(midChild).ToArray()),
            new PositioningCollection<BTreeIndex>(node.Indices.Values.Skip(midIndex + 1).ToArray()),
            popupIndex
        );
    }

    private async Task<BTreeNode> AddIndexAndNodeToParent(
        BTreeNode parent,
        BTreeIndex insertingIndex,
        long insertingNode,
        long insertAfterNode,
        CancellationToken cancellationToken)
    {
        var target = await FindTargetIn(insertingIndex, parent, cancellationToken);

        var insertingNodePosition = parent.Children.IndexOf(insertAfterNode) + 1;
        var newChildren = parent.Children.Insert(insertingNode, insertingNodePosition);

        var newIndices = parent.Indices.Insert(insertingIndex, target.Position);

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
        PositioningCollection<long> children,
        CancellationToken cancellationToken)
    {
        var rootId = await store.AllocateId(cancellationToken);
        var root = new BTreeNode(
            rootId,
            null,
            children,
            new PositioningCollection<BTreeIndex>([index])
        );
        await store.SaveNode(root, cancellationToken);
        await store.SetRoot(root.Id, cancellationToken);
        return root;
    }

    private async Task CreateOrUpdateNode(
        long nodeId,
        long newParentId,
        PositioningCollection<long> newChildren,
        PositioningCollection<BTreeIndex> newIndices,
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
            // change parent id of the children to this node's id
            foreach (var childId in newChildren.Values)
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
            if (searchInNode.Children.Length == 0)
            {
                // this is a leaf node, no need to search further
                return await FindTargetIn(insertingIndex, searchInNode, cancellationToken);
            }

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

    private async Task<FindTargetResult> FindTargetIn(BTreeIndex index, BTreeNode target,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < target.Indices.Length; i++)
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
        return new FindTargetResult(target, target.Indices.Length);
    }

    private readonly struct FindTargetResult(BTreeNode target, int position)
    {
        public BTreeNode Target { get; } = target;
        public int Position { get; } = position;
    }

    // we cannot return BTreeNode because we do not have parent id yet
    // parent may be created based on the result of this method
    private readonly struct SplitBTreeNodeResult(
        PositioningCollection<long> leftChildren,
        PositioningCollection<BTreeIndex> leftIndices,
        PositioningCollection<long> rightChildren,
        PositioningCollection<BTreeIndex> rightIndices,
        BTreeIndex popupIndex
    )
    {
        public PositioningCollection<long> LeftChildren { get; } = leftChildren;
        public PositioningCollection<BTreeIndex> LeftIndices { get; } = leftIndices;
        public PositioningCollection<long> RightChildren { get; } = rightChildren;
        public PositioningCollection<BTreeIndex> RightIndices { get; } = rightIndices;
        public BTreeIndex PopupIndex { get; } = popupIndex;
    }
}