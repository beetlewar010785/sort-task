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
            new MemoryBTreeRow(10, "good news"),
            new MemoryBTreeRow(11, "bad news"),
            new MemoryBTreeRow(5, "alcohol is bad"),
            new MemoryBTreeRow(72, "collaboration is good"),
            new MemoryBTreeRow(12, "collaboration is good")
        };

        var sut = new BTreeRowIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId, MemoryBTreeRow>(
            readWriter,
            new MemoryBTreeNodeFactory(),
            new MemoryBTreeIndexFactory(),
            new BTreeOrder(1)
        );
        foreach (var row in rows)
        {
            await sut.IndexRow(row);
        }
    }
}