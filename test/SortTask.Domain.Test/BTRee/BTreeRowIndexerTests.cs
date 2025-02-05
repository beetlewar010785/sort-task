using SortTask.Domain.BTree;
using SortTask.Domain.BTree.Memory;

namespace SortTask.Domain.Test.BTRee;

public class BTreeRowIndexerTests
{
    [Test]
    public async Task Should_Build_Index()
    {
        var readWriter = new MemoryBTreeReadWriter();
        var rows = new[]
        {
            new Row(10, "good news"),
            new Row(11, "bad news"),
            new Row(5, "alcohol is bad"),
            new Row(72, "collaboration is good"),
            new Row(12, "collaboration is good")
        };

        var expectedRows = new List<Row>(rows);
        expectedRows.Sort(new RowComparer());

        var sut = new BTreeRowIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            readWriter,
            new MemoryBTreeNodeFactory(),
            new MemoryBTreeIndexFactory(),
            new BTreeOrder(1),
            new RowIndexKeyComparer()
        );
        foreach (var row in rows)
        {
            await sut.IndexRow(row);
        }

        var a = 3;
    }

    // private static IEnumerable<BTreeIndex> CollectIndexes(BTreeNode node)
    // {
    //     for (var i = 0; i < node.Indexes.Count; i++)
    //     {
    //     }
    // }
}