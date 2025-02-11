using System.Text;
using SortTask.Adapter;
using SortTask.Adapter.StreamBTree;
using SortTask.Domain.BTree;
using SortTask.Domain.RowGeneration;

namespace SortTask.Domain.Test.BTRee;

public class BTreeIndexerTests
{
    [TestCaseSource(nameof(SortingCases))]
    public async Task Should_Build_Sorted_Index(TestCase testCase)
    {
        // Prepare incoming rows - move them from array to the stream.
        using var unsortedRowStream = new MemoryStream();
        var unsortedStreamRowReadWriter = new StreamRowReadWriter(unsortedRowStream, testCase.Encoding);
        foreach (var row in testCase.Rows)
        {
            await unsortedStreamRowReadWriter.Write(row, CancellationToken.None);
            await unsortedStreamRowReadWriter.Flush(CancellationToken.None);
        }

        // Indexing rows.
        using var indexStream = new MemoryStream();
        var rowComparer = new RowComparer();
        var rowLookup = new StreamBTreeRowLookup(unsortedStreamRowReadWriter);
        var indexComparer = new StreamBTreeIndexComparer(new BigEndianStringOphComparer(), rowComparer, rowLookup);
        var nodeFactory = new StreamBTreeNodeFactory();
        var stringOph = new StringOph(testCase.Encoding);
        var indexFactory = new StreamBTreeIndexFactory(stringOph);
        var bTreeNodeReadWriter = new StreamBTreeNodeReadWriter(indexStream, testCase.Order);
        var store = new StreamBTreeStore(bTreeNodeReadWriter);
        await store.Initialize(CancellationToken.None);

        var sut = new BTreeIndexer<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>(
            store,
            nodeFactory,
            indexComparer,
            testCase.Order
        );

        await using var iterationStream = new MemoryStream(unsortedRowStream.ToArray());
        var iterationRowReadWriter = new StreamRowReadWriter(iterationStream, testCase.Encoding);
        await iterationRowReadWriter
            .ReadAsAsyncEnumerable(CancellationToken.None)
            .ForEachAwaitAsync(async row => await sut.Index(
                indexFactory.CreateIndexFromRow(row.Row, row.Offset, row.Length),
                CancellationToken.None));

        var traverser = new BTreeIndexTraverser<StreamBTreeNode, StreamBTreeIndex, StreamBTreeNodeId>(store);
        var sortedRows = await traverser
            .IterateAsAsyncEnumerable(CancellationToken.None)
            .SelectAwait(async index => await rowLookup.FindRow(index, CancellationToken.None))
            .ToListAsync();

        var expectedSortedRows = testCase.Rows.OrderBy(r => r, rowComparer).ToList();
        Assert.That(sortedRows, Is.EqualTo(expectedSortedRows));
    }

    // [TestCaseSource(nameof(SortingCases))]
    // public async Task Store_Should_Be_Consistent(TestCase testCase)
    // {
    //     var store = new MemoryBTreeStore();
    //
    //     var rowComparer = new RowComparer();
    //     var indexComparer = new MemoryBTreeIndexComparer(rowComparer);
    //
    //     var indexer = new BTreeIndexer<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
    //         store,
    //         new MemoryBTreeNodeFactory(),
    //         indexComparer,
    //         testCase.Order
    //     );
    //
    //     var sut = new BTreeIndexValidator<MemoryBTreeNode, MemoryBTreeIndex, MemoryBTreeNodeId>(
    //         store,
    //         new MemoryBTreeRowLookup(),
    //         rowComparer
    //     );
    //
    //     long position = 0;
    //     foreach (var row in testCase.Rows)
    //     {
    //         await indexer.Index(new MemoryBTreeIndex(row.ToReadRow(position++)), CancellationToken.None);
    //     }
    //
    //     Assert.DoesNotThrowAsync(() => sut.Validate(CancellationToken.None));
    // }

    public record TestCase(IList<Row> Rows, BTreeOrder Order, Encoding Encoding);

    private static IEnumerable<TestCaseData> SortingCases()
    {
        yield return new TestCaseData(new TestCase([], new BTreeOrder(1), Encoding.UTF8))
            .SetName("No rows");

        yield return new TestCaseData(new TestCase(
                [
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
                    new Row(56546, "Intelligent Rubber Salad Sports & Toys mobile Jewelery, Grocery & Toys"),
                    new Row(41868, "Metal"),
                    new Row(86638, "mobile Technician"),
                    new Row(98549, "Security markets Tasty Soft Sausages programming"),
                    new Row(78055, "monitor Key Mountains Cotton"),
                    new Row(8992, "upward-trending moderator Avon Technician"),
                    new Row(42332, "Solutions partnerships"),
                    new Row(16054, "Personal Loan Account Practical Rubber Towels"),
                    new Row(58160, "frame Quality-focused"),
                    new Row(53448, "Coordinator pink"),
                    new Row(28512, "Bedfordshire invoice Avon Small Rubber Shoes"),
                    new Row(62574, "Buckinghamshire Forward"),
                    new Row(51466, "Division Principal Personal Loan Account"),
                    new Row(44270, "Plaza Home Loan Account Concrete structure"),
                    new Row(59546, "calculating optical Handmade Wooden Soap Handcrafted Soft Chicken"),
                    new Row(95987, "Wooden Street"),
                    new Row(18708, "New Leu"),
                    new Row(88794, "Incredible Plastic Salad Zimbabwe one-to-one"),
                    new Row(38680, "Djibouti Franc Money Market Account")
                ],
                new BTreeOrder(2),
                Encoding.UTF8))
            .SetName("Predefined rows");


        var rowGenerator = new RandomRowGenerator(new Random(), 1000, 3);
        yield return new TestCaseData(new TestCase(
                Enumerable
                    .Range(0, 1000)
                    .SelectMany(_ => rowGenerator.Generate())
                    .ToList(),
                new BTreeOrder(10),
                Encoding.UTF8)
            )
            .SetName("Random rows");
    }
}