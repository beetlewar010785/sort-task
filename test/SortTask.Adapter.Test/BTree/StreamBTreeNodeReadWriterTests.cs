using System.Text.Json;
using SortTask.Adapter.StreamBTree;
using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.Test.BTree;

public class StreamBTreeNodeReadWriterTests
{
    [Test]
    public async Task Should_Write_Read_Header()
    {
        await using var stream = new MemoryStream();
        var sut = new StreamBTreeNodeReadWriter(stream, new BTreeOrder(2));

        var initialHeader = new StreamBTreeHeader(10, new StreamBTreeNodeId(123));
        await sut.WriteHeader(initialHeader, CancellationToken.None);

        var actualHeader = await sut.ReadHeader(CancellationToken.None);

        var initialJson = JsonSerializer.Serialize(initialHeader);
        var actualJson = JsonSerializer.Serialize(actualHeader);
        Assert.That(actualJson, Is.EqualTo(initialJson));
    }
    
    [Test]
    public async Task Should_Write_Read_Node()
    {
        await using var stream = new MemoryStream();
        var sut = new StreamBTreeNodeReadWriter(stream, new BTreeOrder(2));

        var id = new StreamBTreeNodeId(0);
        var initialNode = new StreamBTreeNode(
            id,
            new StreamBTreeNodeId(456),
            [new StreamBTreeNodeId(789), new StreamBTreeNodeId(890)],
            [
                new StreamBTreeIndex(new OphULong(123), 123, 456),
                new StreamBTreeIndex(new OphULong(234), 234, 567)
            ]);

        await sut.WriteNode(initialNode, CancellationToken.None);

        var actualNode = await sut.ReadNode(id, CancellationToken.None);

        var initialJson = JsonSerializer.Serialize(initialNode);
        var actualJson = JsonSerializer.Serialize(actualNode);
        Assert.That(actualJson, Is.EqualTo(initialJson));
    }
}