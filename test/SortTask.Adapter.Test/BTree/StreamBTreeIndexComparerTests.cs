using SortTask.Adapter.StreamBTree;
using SortTask.Domain;

namespace SortTask.Adapter.Test.BTree;

public class StreamBTreeIndexComparerTests
{
    [TestCaseSource(nameof(GetTestCases))]
    public async Task<int> Should_Compare_Indices(
        StreamBTreeIndex index1,
        StreamBTreeIndex index2,
        IEnumerable<ReadRow> rows,
        IComparer<ReadRow> comparer)
    {
        var rowLookup = new RowLookupMock();
        foreach (var row in rows)
        {
            rowLookup.AddRow(row);
        }

        var sut = new StreamBTreeIndexComparer(comparer, rowLookup);
        return await sut.Compare(index1, index2, CancellationToken.None);
    }

    private static IEnumerable<TestCaseData> GetTestCases()
    {
        var row1 = new ReadRow(123, "a", 1);
        var row2 = new ReadRow(234, "b", 2);

        yield return new TestCaseData(
                new StreamBTreeIndex(1),
                new StreamBTreeIndex(2),
                new List<ReadRow>([row1, row2]),
                new RowComparerMock().AddResult(row1, row2, 1)).Returns(1)
            .SetName("1");

        yield return new TestCaseData(
                new StreamBTreeIndex(1),
                new StreamBTreeIndex(2),
                new List<ReadRow>([row1, row2]),
                new RowComparerMock().AddResult(row1, row2, -1)).Returns(-1)
            .SetName("-1");

        yield return new TestCaseData(
                new StreamBTreeIndex(1),
                new StreamBTreeIndex(2),
                new List<ReadRow>([row1, row2]),
                new RowComparerMock().AddResult(row1, row2, -0)).Returns(0)
            .SetName("0");
    }

    private class RowLookupMock : IRowLookup<StreamBTreeIndex>
    {
        private readonly Dictionary<long, ReadRow> _rows = new();

        public void AddRow(ReadRow row)
        {
            _rows.Add(row.Position, row);
        }

        public Task<ReadRow> FindRow(StreamBTreeIndex index, CancellationToken cancellationToken)
        {
            return Task.FromResult(_rows[index.RowPosition]);
        }
    }

    private class RowComparerMock : IComparer<ReadRow>
    {
        private readonly Dictionary<ValueTuple<ReadRow, ReadRow>, int> _compareResults = new();

        public RowComparerMock AddResult(ReadRow x, ReadRow y, int result)
        {
            _compareResults.Add(ValueTuple.Create(x, y), result);
            return this;
        }

        public int Compare(ReadRow x, ReadRow y)
        {
            var t = ValueTuple.Create(x, y);
            if (!_compareResults.TryGetValue(t, out var result))
            {
                throw new Exception("Not found");
            }

            return result;
        }
    }
}