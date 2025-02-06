using SortTask.Domain.BTree;
using SortTask.Domain.BTree.Memory;

namespace SortTask.Domain.Test.BTRee;

public class BTreeIndexerTests
{
    [TestCaseSource(nameof(SortingCases))]
    public async Task Should_Build_Sorted_Index(IList<Row> rows)
    {
        var store = new MemoryBTreeStore();
        var rowComparer = new RowComparer();
        var indexComparer = new MemoryBTreeIndexComparer(rowComparer);

        var sut = new BTreeIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            store,
            new MemoryBTreeNodeFactory(),
            indexComparer,
            new BTreeOrder(1)
        );

        foreach (var row in rows)
        {
            await sut.Index(new MemoryBTreeIndex(row));
        }

        var traverser = new BTreeIndexTraverser<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(store);
        var sortedNodes = await traverser.Traverse().ToListAsync(CancellationToken.None);
        var sortedRows = sortedNodes.Select(n => n.Row).ToList();

        var expectedSortedRows = rows.OrderBy(r => r, rowComparer).ToList();
        Assert.That(sortedRows, Is.EqualTo(expectedSortedRows));
    }

    [TestCaseSource(nameof(SortingCases))]
    public async Task Store_Should_Be_Consistent(IList<Row> rows)
    {
        var store = new MemoryBTreeStore();
        var rowComparer = new RowComparer();
        var indexComparer = new MemoryBTreeIndexComparer(rowComparer);

        var indexer = new BTreeIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            store,
            new MemoryBTreeNodeFactory(),
            indexComparer,
            new BTreeOrder(1)
        );

        foreach (var row in rows)
        {
            await indexer.Index(new MemoryBTreeIndex(row));
            var a = 3;
        }

        var sut = new BTreeIndexValidator<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
            store,
            new MemoryBTreeRowLookup(),
            rowComparer
        );

        Assert.DoesNotThrowAsync(sut.Validate);
    }

    private static IEnumerable<TestCaseData> SortingCases()
    {
        // yield return new TestCaseData(new List<Row>())
        //     .SetName("NoRows");
        yield return new TestCaseData(new List<Row>([
                new Row(23990, "Frozen Buckinghamshire Trail"),
                new Row(38680, "Djibouti Franc Money Market Account"),
                new Row(79758, "Idaho Guinea-Bissau East Caribbean Dollar"),
                new Row(58832, "overriding Alabama withdrawal"),
                new Row(6815, "online"),
                new Row(21539, "Forges Networked HDD Tennessee"),
                new Row(7557, "bypass"),
                new Row(13762, "Horizontal Avon Avon"),
                new Row(45205, "Island Turnpike"),
                new Row(49535, "Park Web Small Wooden Mouse"),
                // new Row(56546, "Intelligent Rubber Salad Sports & Toys mobile Jewelery, Grocery & Toys"),
                // new Row(41868, "Metal"),
                // new Row(86638, "mobile Technician"),
                // new Row(98549, "Security markets Tasty Soft Sausages programming"),
                // new Row(78055, "monitor Key Mountains Cotton"),
                // new Row(8992, "upward-trending moderator Avon Technician"),
                // new Row(42332, "Solutions partnerships"),
                // new Row(16054, "Personal Loan Account Practical Rubber Towels"),
                // new Row(58160, "frame Quality-focused"),
                // new Row(53448, "Coordinator pink"),
                // new Row(28512, "Bedfordshire invoice Avon Small Rubber Shoes"),
                // new Row(62574, "Buckinghamshire Forward"),
                // new Row(51466, "Division Principal Personal Loan Account"),
                // new Row(44270, "Plaza Home Loan Account Concrete structure"),
                // new Row(59546, "calculating optical Handmade Wooden Soap Handcrafted Soft Chicken"),
                // new Row(95987, "Wooden Street"),
                // new Row(18708, "New Leu"),
                // new Row(88794, "Incredible Plastic Salad Zimbabwe one-to-one")
            ]))
            .SetName("PredefinedRows");


        // var rowGenerator = new RandomRowGenerator(new Random(), 1000, 3);
        // yield return new TestCaseData(
        //         Enumerable
        //             .Range(0, 1000)
        //             .SelectMany(_ => rowGenerator.Generate())
        //             .ToList()
        //     )
        //     .SetName("RandomRows");
    }
}