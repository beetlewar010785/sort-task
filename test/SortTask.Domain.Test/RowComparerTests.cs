namespace SortTask.Domain.Test;

public class RowComparerTests
{
    [TestCaseSource(nameof(GetRowCases))]
    public int Should_Compare_Rows(Row row1, Row row2)
    {
        var sut = new RowComparer();
        return sut.Compare(row1, row2);
    }

    private static IEnumerable<TestCaseData> GetRowCases()
    {
        yield return new TestCaseData(
                new Row(1, "a"),
                new Row(1, "b"))
            .Returns(-1);

        yield return new TestCaseData(
                new Row(1, "b"),
                new Row(1, "a"))
            .Returns(1);

        yield return new TestCaseData(
                new Row(1, "a"),
                new Row(2, "a"))
            .Returns(-1);

        yield return new TestCaseData(
                new Row(2, "a"),
                new Row(1, "a"))
            .Returns(1);

        yield return new TestCaseData(
                new Row(1, "a"),
                new Row(1, "a"))
            .Returns(0);
    }
}