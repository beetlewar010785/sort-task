using SortTask.Domain.BTree;
using SortTask.Domain.BTree.Memory;

namespace SortTask.Domain.Test.BTRee;

public class BTreeIndexerTests
{
    [Test]
    public async Task Should_Build_Index()
    {
        var readWriter = new MemoryBTreeReadWriter();
        var rows = new[]
        {
            new MemoryBTreeRow(10, "good news"),
            new MemoryBTreeRow(11, "bad news"),
            new MemoryBTreeRow(5, "alcohol is bad"),
            new MemoryBTreeRow(72, "collaboration is good"),
            new MemoryBTreeRow(12, "collaboration is good")
        };

        var rowComparer = new RowComparer();
        var indexComparer = new MemoryBTreeIndexComparer(rowComparer);

        var sut = new BTreeIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            readWriter,
            new MemoryBTreeNodeFactory(),
            indexComparer,
            new BTreeOrder(1)
        );

        foreach (var row in rows)
        {
            await sut.Index(new MemoryBTreeIndex(row));
        }
    }
}