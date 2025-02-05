using SortTask.Domain.BTree;

namespace SortTask.Domain.Test.BTRee;

public class BTreeNodeCollectionTests
{
    [Test]
    public void Should_Insert_After_Specified_Node()
    {
        var initial = new BTreeNodeCollection<int>([1, 2, 3, 4]);
        var actual = initial.InsertAfter(inserting: 5, after: 2);
        var expected = new BTreeNodeCollection<int>([1, 2, 5, 3, 4]);

        Assert.That(actual, Is.EqualTo(expected));
    }
}