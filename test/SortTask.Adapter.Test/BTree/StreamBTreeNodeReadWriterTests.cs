using System.Text.Json;
using SortTask.Adapter.BTree;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.Test.BTree;

public class StreamBTreeNodeReadWriterTests
{
    [Test]
    public async Task Should_Write_Read_Header()
    {
        await using var stream = new MemoryStream();
        var sut = new StreamBTreeNodeReadWriter<ulong>(stream, new BTreeOrder(2), new UlongOphReadWriter());

        var initialHeader = new StreamBTreeHeader(10, 123);
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
        var sut = new StreamBTreeNodeReadWriter<ulong>(stream, new BTreeOrder(2), new UlongOphReadWriter());

        var id = 0L;
        var initialNode = new BTreeNode<ulong>(
            id,
            456,
            new PositioningCollection<long>([789, 890]),
            new PositioningCollection<BTreeIndex<ulong>>([
                new BTreeIndex<ulong>(123, 123, 456),
                new BTreeIndex<ulong>(234, 234, 567)
            ])
        );

        await sut.WriteNode(initialNode, CancellationToken.None);

        var actualNode = await sut.ReadNode(id, CancellationToken.None);

        var initialJson = JsonSerializer.Serialize(initialNode);
        var actualJson = JsonSerializer.Serialize(actualNode);
        Assert.That(actualJson, Is.EqualTo(initialJson));
    }
}