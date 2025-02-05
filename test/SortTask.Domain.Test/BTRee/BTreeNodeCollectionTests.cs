using SortTask.Domain.BTree;

namespace SortTask.Domain.Test.BTRee;

public class BTreeNodeCollectionTests
{
    [TestCaseSource(nameof(SortTestCases))]
    public BTreeNodeCollection<int> Should_Insert_After_Specified_Node(
        BTreeNodeCollection<int> input,
        int insertingValue,
        int insertAfter)
    {
        return input.InsertAfter(inserting: insertingValue, after: insertAfter);
    }

    private static IEnumerable<TestCaseData> SortTestCases()
    {
        yield return new TestCaseData(new BTreeNodeCollection<int>([1, 2, 3, 4]), 5, 2)
            .Returns(new BTreeNodeCollection<int>([1, 2, 5, 3, 4]));

        yield return new TestCaseData(new BTreeNodeCollection<int>([1, 2]), 3, 2)
            .Returns(new BTreeNodeCollection<int>([1, 2, 3]));
    }
}