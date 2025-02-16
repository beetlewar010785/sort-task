using System.Text;
using SortTask.Adapter.BTree;
using SortTask.Domain;
using SortTask.Domain.BTree;
using SortTask.Domain.RowGeneration;

namespace SortTask.Adapter.Test.BTree;

public class BTreeIndexerTests
{
    [TestCaseSource(nameof(SortingCases))]
    public void ShouldBuildSortedIndex(TestCase testCase)
    {
        const int ophWords = 1;

        // Prepare incoming rows - move them from array to the stream.
        using var unsortedRowStream = new MemoryStream();
        var oph = new Oph(ophWords);
        var unsortedStreamRowReadWriter = new StreamRowStore(unsortedRowStream, testCase.Encoding);
        foreach (var row in testCase.Rows)
        {
            unsortedStreamRowReadWriter.Write(row);
            unsortedStreamRowReadWriter.Flush();
        }

        // Indexing rows
        using var indexStream = new MemoryStream();
        var rowComparer = new RowComparer();
        var ophComparer = new OphComparer();
        var indexComparer = new BTreeIndexComparer<OphValue>(ophComparer, rowComparer, unsortedStreamRowReadWriter);
        var ophReadWriter = new OphReadWriter(ophWords);
        using var store = new StreamBTreeStore<OphValue>(indexStream, testCase.Order, ophReadWriter);

        var sut = new BTreeIndexer<OphValue>(
            store,
            indexComparer,
            testCase.Order,
            oph,
            testCase.Encoding
        );

        using var iterationStream = new MemoryStream(unsortedRowStream.ToArray());
        var iterationRowReadWriter = new StreamRowStore(iterationStream, testCase.Encoding);
        foreach (var row in iterationRowReadWriter.IterateOverRows()) sut.Index(row.Row, row.Offset, row.Length);

        var traverser = new BTreeIndexTraverser<OphValue>(store);
        var sortedRows = traverser
            .IterateOverIndex()
            .Select(x => unsortedStreamRowReadWriter.FindRow(x.Offset, x.Length));

        var expectedSortedRows = testCase.Rows.OrderBy(r => r, rowComparer).ToList();
        Assert.That(sortedRows, Is.EqualTo(expectedSortedRows));
    }

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


        var rowGenerator = new RandomRowGenerator(new Random());
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

    public record TestCase(IList<Row> Rows, BTreeOrder Order, Encoding Encoding);
}
