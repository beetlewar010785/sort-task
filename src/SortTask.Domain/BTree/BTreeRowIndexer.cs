namespace SortTask.Domain.BTree;

public class BTreeRowIndexer<TNode, TIndex, TNodeId, TRow>(
    IBTreeReadWriter<TNode, TIndex, TNodeId> readWriter,
    IBTreeNodeFactory<TNode, TIndex, TNodeId> nodeFactory,
    IBTreeIndexFactory<TIndex, TRow> indexFactory,
    BTreeOrder order
) : IRowIndexer<TRow>
    where TNode : IBTreeNode<TNode, TIndex, TNodeId>
    where TIndex : IBTreeIndex
    where TRow : IRow
{
    public async Task IndexRow(TRow row)
    {
        var root = await readWriter.GetRoot();
        if (root == null)
        {
            var rootId = await readWriter.AllocateId();
            root = nodeFactory.Create(rootId, default, BTreeNodeCollection<TNodeId>.Empty, []);
            await readWriter.SaveNode(root);
            await readWriter.SetRoot(root.Id);
        }

        var index = indexFactory.CreateIndexFromRow(row);
        var targetNode = await FindTargetNode(index, root);
        await InsertIndex(index, targetNode);
    }

    private async Task InsertIndex(TIndex index, TNode targetNode)
    {
        targetNode = nodeFactory.Create(
            targetNode.Id,
            targetNode.ParentId,
            targetNode.Children,
            targetNode.Indexes.Append(index)
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
                [splitResult.PopupIndex]
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
                parent.Indexes.Append(splitResult.PopupIndex)
            );
            await readWriter.SaveNode(parent);
        }

        // do not forget to save updated node and created one
        var leftNode = nodeFactory.Create(
            splitResult.Left,
            parent.Id,
            splitResult.LeftChildren,
            splitResult.LeftIndexes
        );
        await readWriter.SaveNode(leftNode);

        var rightNode = nodeFactory.Create(
            splitResult.Right,
            parent.Id,
            splitResult.RightChildren,
            splitResult.RightIndexes
        );
        await readWriter.SaveNode(rightNode);

        // repeat recursively until we reach the root or find a node that is not overflowed
        await CheckOverflow(parent);
    }

    private bool IsOverflowed(TNode node)
    {
        return node.Indexes.Count > order.Value * 2;
    }

    private async Task<TNode> FindTargetNode(TIndex index, TNode searchInNode)
    {
        if (searchInNode.Children.Count == 0)
        {
            // this is a leaf node, no need to search further
            return searchInNode;
        }

        // for (var i = 0; i < searchInNode.Indexes.Count; i++)
        // {
        //     var otherIndex = searchInNode.Indexes[i];
        //     var compareResult = rowIndexKeyComparer.Compare(key, otherIndex.Key);
        // }

        return searchInNode;
    }

    private async Task<SplitNodeResult> SplitNode(TNode node)
    {
        var midIndex = node.Indexes.Count / 2;
        var halfChildren = node.Children.Count / 2;
        var popupIndex = node.Indexes[midIndex];
        var rightNodeId = await readWriter.AllocateId();
        return new SplitNodeResult(
            node.Id,
            new BTreeNodeCollection<TNodeId>(node.Children.Take(halfChildren).ToList()),
            node.Indexes.Take(midIndex),
            rightNodeId,
            new BTreeNodeCollection<TNodeId>(node.Children.Skip(halfChildren).ToList()),
            node.Indexes.Skip(midIndex + 1),
            popupIndex
        );
    }

    // we cannot return TNode because we do not have parent id yet
    // parent may be created based on the result of this method
    private record SplitNodeResult(
        TNodeId Left,
        BTreeNodeCollection<TNodeId> LeftChildren,
        IEnumerable<TIndex> LeftIndexes,
        TNodeId Right,
        BTreeNodeCollection<TNodeId> RightChildren,
        IEnumerable<TIndex> RightIndexes,
        TIndex PopupIndex
    );
}