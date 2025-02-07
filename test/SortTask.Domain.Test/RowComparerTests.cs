namespace SortTask.Domain.Test;

public class RowComparerTests
{
    [TestCaseSource(nameof(WriteRowCases))]
    public int Should_Compare_Write_Rows(WriteRow row1, WriteRow row2)
    {
        var sut = new RowComparer();
        return sut.Compare(row1, row2);
    }

    [TestCaseSource(nameof(ReadRowCases))]
    public int Should_Compare_Read_Rows(ReadRow row1, ReadRow row2)
    {
        var sut = new RowComparer();
        return sut.Compare(row1, row2);
    }

    private static IEnumerable<TestCaseData> WriteRowCases()
    {
        yield return new TestCaseData(
                new WriteRow(1, "a"),
                new WriteRow(1, "b"))
            .Returns(-1);

        yield return new TestCaseData(
                new WriteRow(1, "b"),
                new WriteRow(1, "a"))
            .Returns(1);

        yield return new TestCaseData(
                new WriteRow(1, "a"),
                new WriteRow(2, "a"))
            .Returns(-1);

        yield return new TestCaseData(
                new WriteRow(2, "a"),
                new WriteRow(1, "a"))
            .Returns(1);

        yield return new TestCaseData(
                new WriteRow(1, "a"),
                new WriteRow(1, "a"))
            .Returns(0);
    }

    private static IEnumerable<TestCaseData> ReadRowCases()
    {
        yield return new TestCaseData(
                new ReadRow(1, "a", 0),
                new ReadRow(1, "b", 1))
            .Returns(-1);

        yield return new TestCaseData(
                new ReadRow(1, "b", 0),
                new ReadRow(1, "a", 1))
            .Returns(1);

        yield return new TestCaseData(
                new ReadRow(1, "a", 0),
                new ReadRow(2, "a", 1))
            .Returns(-1);

        yield return new TestCaseData(
                new ReadRow(2, "a", 0),
                new ReadRow(1, "a", 1))
            .Returns(1);

        yield return new TestCaseData(
                new ReadRow(1, "a", 0),
                new ReadRow(1, "a", 1))
            .Returns(0);
    }
}