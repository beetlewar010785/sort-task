using System.Text;

namespace SortTask.Domain.BTree;

public class BTreeIndexer<TOphValue>(
    IBTreeStore<TOphValue> store,
    IBTreeIndexComparer<TOphValue> indexComparer,
    BTreeOrder order,
    IOph<TOphValue> oph,
    Encoding encoding
) : IIndexer
    where TOphValue : struct
{
    public void Index(Row row, long offset, int length)
    {
        var sentenceBytes = encoding.GetBytes(row.Sentence);
        var ophHash = oph.Hash(sentenceBytes);
        var index = new BTreeIndex<TOphValue>(ophHash, offset, length);

        var rootId = store.GetRoot();
        if (rootId == null)
        {
            _ = CreateNewRoot(index, new PositioningItems<long>([]));
            return;
        }

        var root = store.GetNode(rootId.Value);
        var targetResult = FindTarget(index, root);
        InsertBTreeIndex(targetResult.Target, targetResult.Position, index);
    }

    private void InsertBTreeIndex(
        BTreeNode<TOphValue> targetBTreeNode,
        int position,
        BTreeIndex<TOphValue> index)
    {
        var newIndices = targetBTreeNode.Indices.Insert(index, position);
        targetBTreeNode = new BTreeNode<TOphValue>(
            targetBTreeNode.Id,
            targetBTreeNode.ParentId,
            targetBTreeNode.Children,
            newIndices
        );
        store.SaveNode(targetBTreeNode);
        CheckOverflow(targetBTreeNode);
    }

    private void CheckOverflow(BTreeNode<TOphValue> node)
    {
        while (true)
        {
            if (!order.IsOverflowed(node.Indices.Length)) return;

            var splitResult = SplitNode(node);
            var rightId = store.AllocateId();

            BTreeNode<TOphValue>? parent = null;
            if (node.ParentId.HasValue) parent = store.GetNode(node.ParentId.Value);

            if (parent == null)
                // if the node does not have parent, it means it is root
                // then we need to assign new root
                parent = CreateNewRoot(
                    splitResult.PopupIndex,
                    new PositioningItems<long>([node.Id, rightId]));
            else
                // when we have a parent, we add an index to the parent and created child node
                parent = AddIndexAndNodeToParent(
                    parent.Value,
                    insertingNode: rightId,
                    insertAfterNode: node.Id,
                    insertingIndex: splitResult.PopupIndex);

            CreateOrUpdateNode(
                node.Id,
                parent.Value.Id,
                splitResult.LeftChildren,
                splitResult.LeftIndices,
                false);

            CreateOrUpdateNode(
                rightId,
                parent.Value.Id,
                splitResult.RightChildren,
                splitResult.RightIndices,
                true);

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

    private BTreeNode<TOphValue> AddIndexAndNodeToParent(
        BTreeNode<TOphValue> parent,
        BTreeIndex<TOphValue> insertingIndex,
        long insertingNode,
        long insertAfterNode)
    {
        var target = FindTargetIn(insertingIndex, parent);

        var insertingNodePosition = parent.Children.IndexOf(insertAfterNode) + 1;
        var newChildren = parent.Children.Insert(insertingNode, insertingNodePosition);

        var newIndices = parent.Indices.Insert(insertingIndex, target.Position);

        parent = new BTreeNode<TOphValue>(
            parent.Id,
            parent.ParentId,
            newChildren,
            newIndices
        );
        store.SaveNode(parent);

        return parent;
    }

    private BTreeNode<TOphValue> CreateNewRoot(
        BTreeIndex<TOphValue> index,
        PositioningItems<long> children)
    {
        var rootId = store.AllocateId();
        var root = new BTreeNode<TOphValue>(
            rootId,
            null,
            children,
            new PositioningItems<BTreeIndex<TOphValue>>([index])
        );
        store.SaveNode(root);
        store.SetRoot(root.Id);
        return root;
    }

    private void CreateOrUpdateNode(
        long nodeId,
        long newParentId,
        PositioningItems<long> newChildren,
        PositioningItems<BTreeIndex<TOphValue>> newIndices,
        bool childrenParentIdChanged
    )
    {
        var node = new BTreeNode<TOphValue>(
            nodeId,
            newParentId,
            newChildren,
            newIndices
        );
        store.SaveNode(node);

        if (childrenParentIdChanged)
            // change parent id of the children to this node's id
            foreach (var childId in newChildren.Values)
            {
                var child = store.GetNode(childId);
                child = new BTreeNode<TOphValue>(
                    child.Id,
                    nodeId,
                    child.Children,
                    child.Indices
                );
                store.SaveNode(child);
            }
    }

    private FindTargetResult FindTarget(
        BTreeIndex<TOphValue> insertingIndex,
        BTreeNode<TOphValue> searchInNode)
    {
        while (true)
        {
            if (searchInNode.Children.Length == 0)
                // this is a leaf node, no need to search further
                return FindTargetIn(insertingIndex, searchInNode);

            // search child where our index which is greater than our index to drill down
            for (var i = 0; i < searchInNode.Indices.Length; i++)
            {
                var existingIndex = searchInNode.Indices[i];
                var indexCompareResult = indexComparer.Compare(insertingIndex, existingIndex);
                if (indexCompareResult > 0) continue;

                var nodeId = searchInNode.Children[i];
                var node = store.GetNode(nodeId);
                return FindTarget(insertingIndex, node);
            }

            // our index is the greatest in the node, search in the rightmost child
            var latestNodeId = searchInNode.Children[^1];
            var latestNode = store.GetNode(latestNodeId);
            searchInNode = latestNode;
        }
    }

    private FindTargetResult FindTargetIn(BTreeIndex<TOphValue> index, BTreeNode<TOphValue> target)
    {
        for (var i = 0; i < target.Indices.Length; i++)
        {
            var existingIndex = target.Indices[i];
            var indexCompareResult = indexComparer.Compare(index, existingIndex);
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
