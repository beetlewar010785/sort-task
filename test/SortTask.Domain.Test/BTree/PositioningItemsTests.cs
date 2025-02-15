using SortTask.Domain.BTree;

namespace SortTask.Domain.Test.BTree;

public class PositioningItemsTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public int Should_Search_For_The_Position(int[] sortedItems, int testedItem)
    {
        var sut = new PositioningItems<int>(sortedItems);
        return sut.SearchPosition(testedItem, (x, y) => x.CompareTo(y));
    }

    private static IEnumerable<TestCaseData> GetTestCases()
    {
        yield return new TestCaseData(new[] { 3, 5, 7 }, 0).Returns(0);
        yield return new TestCaseData(new[] { 3, 5, 7 }, 4).Returns(1);
        yield return new TestCaseData(new[] { 3, 5, 7 }, 6).Returns(2);
        yield return new TestCaseData(new[] { 3, 5, 7 }, 8).Returns(3);
    }
}
