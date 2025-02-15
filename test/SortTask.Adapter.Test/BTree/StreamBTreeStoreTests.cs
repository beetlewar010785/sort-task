using System.Text.Json;
using SortTask.Adapter.BTree;
using SortTask.Domain.BTree;

namespace SortTask.Adapter.Test.BTree;

public class StreamBTreeStoreTests
{
    [Test]
    public void Should_Get_Saved_Node()
    {
        var stream = new MemoryStream();
        var sut = new StreamBTreeStore<OphValue>(stream, new BTreeOrder(2), new OphReadWriter(2));

        const long id = 0L;
        var initialNode = new BTreeNode<OphValue>(
            id,
            456,
            new PositioningItems<long>([789, 890]),
            new PositioningItems<BTreeIndex<OphValue>>([
                new BTreeIndex<OphValue>(new OphValue([123, 234]), 123, 456),
                new BTreeIndex<OphValue>(new OphValue([234, 345]), 234, 567)
            ])
        );

        sut.SaveNode(initialNode);

        var actualNode = sut.GetNode(id);

        var initialJson = JsonSerializer.Serialize(initialNode);
        var actualJson = JsonSerializer.Serialize(actualNode);
        Assert.That(actualJson, Is.EqualTo(initialJson));
    }

    [TestCase(1_000)]
    [TestCase(10_000)]
    [TestCase(100_000)]
    [TestCase(1_000_000)]
    public void Should_Save_And_Get_Enormous_Number_Of_Nodes(int count)
    {
        using var stream = new MemoryStream();
        var sut = new StreamBTreeStore<OphValue>(stream, new BTreeOrder(2), new OphReadWriter(2));

        var ids = new List<long>();
        for (var i = 0; i < count; i++)
        {
            var id = sut.AllocateId();
            ids.Add(id);
            sut.SaveNode(new BTreeNode<OphValue>(id, null,
                new PositioningItems<long>([]),
                new PositioningItems<BTreeIndex<OphValue>>([])));
        }

        foreach (var id in ids) Assert.DoesNotThrow(() => sut.GetNode(id));
    }
}
