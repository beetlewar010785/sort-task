using SortTask.Adapter.MemoryBTree;
using SortTask.Domain.BTree;
using SortTask.Domain.RowGeneration;

namespace SortTask.Domain.Test.BTRee;

public class BTreeIndexerTests
{
    [TestCaseSource(nameof(SortingCases))]
    public async Task Should_Build_Sorted_Index(TestCase testCase)
    {
        var store = new MemoryBTreeStore();
        var rowComparer = new RowComparer();
        var indexComparer = new MemoryBTreeIndexComparer(rowComparer);

        var sut = new BTreeIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            store,
            new MemoryBTreeNodeFactory(),
            indexComparer,
            testCase.Order
        );

        long position = 0;
        await sut.Initialize(CancellationToken.None);
        foreach (var row in testCase.Rows)
        {
            await sut.Index(new MemoryBTreeIndex(row.ToReadRow(position++)), CancellationToken.None);
        }

        var traverser = new BTreeIndexTraverser<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(store);
        var sortedNodes = await traverser
            .Traverse(CancellationToken.None)
            .ToListAsync(CancellationToken.None);

        var sortedRows = sortedNodes.Select(n => n.Row.ToWriteRow()).ToList();

        var expectedSortedRows = testCase.Rows.OrderBy(r => r, rowComparer).ToList();
        Assert.That(sortedRows, Is.EqualTo(expectedSortedRows));
    }

    [TestCaseSource(nameof(SortingCases))]
    public async Task Store_Should_Be_Consistent(TestCase testCase)
    {
        var store = new MemoryBTreeStore();

        var rowComparer = new RowComparer();
        var indexComparer = new MemoryBTreeIndexComparer(rowComparer);

        var indexer = new BTreeIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            store,
            new MemoryBTreeNodeFactory(),
            indexComparer,
            testCase.Order
        );

        var sut = new BTreeIndexValidator<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            store,
            new MemoryBTreeRowLookup(),
            rowComparer
        );

        long position = 0;
        await indexer.Initialize(CancellationToken.None);
        foreach (var row in testCase.Rows)
        {
            await indexer.Index(new MemoryBTreeIndex(row.ToReadRow(position++)), CancellationToken.None);
        }

        Assert.DoesNotThrowAsync(() => sut.Validate(CancellationToken.None));
    }

    public record TestCase(IList<WriteRow> Rows, BTreeOrder Order);

    private static IEnumerable<TestCaseData> SortingCases()
    {
        yield return new TestCaseData(new TestCase([], new BTreeOrder(1)))
            .SetName("No rows");

        yield return new TestCaseData(new TestCase(
                [
                    new WriteRow(23990, "Frozen Buckinghamshire Trail"),
                    new WriteRow(38680, "Djibouti Franc Money Market Account"),
                    new WriteRow(79758, "Idaho Guinea-Bissau East Caribbean Dollar"),
                    new WriteRow(58832, "overriding Alabama withdrawal"),
                    new WriteRow(6815, "online"),
                    new WriteRow(21539, "Forges Networked HDD Tennessee"),
                    new WriteRow(7557, "bypass"),
                    new WriteRow(13762, "Horizontal Avon Avon"),
                    new WriteRow(45205, "Island Turnpike"),
                    new WriteRow(49535, "Park Web Small Wooden Mouse"),
                    new WriteRow(56546, "Intelligent Rubber Salad Sports & Toys mobile Jewelery, Grocery & Toys"),
                    new WriteRow(41868, "Metal"),
                    new WriteRow(86638, "mobile Technician"),
                    new WriteRow(98549, "Security markets Tasty Soft Sausages programming"),
                    new WriteRow(78055, "monitor Key Mountains Cotton"),
                    new WriteRow(8992, "upward-trending moderator Avon Technician"),
                    new WriteRow(42332, "Solutions partnerships"),
                    new WriteRow(16054, "Personal Loan Account Practical Rubber Towels"),
                    new WriteRow(58160, "frame Quality-focused"),
                    new WriteRow(53448, "Coordinator pink"),
                    new WriteRow(28512, "Bedfordshire invoice Avon Small Rubber Shoes"),
                    new WriteRow(62574, "Buckinghamshire Forward"),
                    new WriteRow(51466, "Division Principal Personal Loan Account"),
                    new WriteRow(44270, "Plaza Home Loan Account Concrete structure"),
                    new WriteRow(59546, "calculating optical Handmade Wooden Soap Handcrafted Soft Chicken"),
                    new WriteRow(95987, "Wooden Street"),
                    new WriteRow(18708, "New Leu"),
                    new WriteRow(88794, "Incredible Plastic Salad Zimbabwe one-to-one")
                ],
                new BTreeOrder(2)))
            .SetName("Predefined rows");


        var rowGenerator = new RandomRowGenerator(new Random(), 1000, 3);
        yield return new TestCaseData(new TestCase(
                Enumerable
                    .Range(0, 100000)
                    .SelectMany(_ => rowGenerator.Generate())
                    .ToList(),
                new BTreeOrder(10))
            )
            .SetName("Random rows");
    }
}