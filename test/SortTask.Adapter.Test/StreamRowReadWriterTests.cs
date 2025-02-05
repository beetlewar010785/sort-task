using System.Text;
using SortTask.Domain.RowGeneration;

namespace SortTask.Adapter.Test;

public class StreamRowReadWriterTests
{
    [Test]
    public async Task Should_Read_All_Written_Rows()
    {
        using var ms = new MemoryStream();

        var initialRows = new[]
        {
            new GeneratingRow(1, "some string 1"),
            new GeneratingRow(2, "some string 2"),
            new GeneratingRow(3, "some another string")
        };

        var writer = new StreamRowWriter(ms, Encoding.UTF8);
        foreach (var row in initialRows)
        {
            await writer.Write(row);
        }

        await writer.Flush(CancellationToken.None);
        ms.Position = 0;

        var reader = new StreamRowReader(ms, Encoding.UTF8);

        var actualRows = (await reader.ReadAsAsyncEnumerable().ToArrayAsync())
            .Select(ReadRowToGeneratingRow)
            .ToArray();
        Assert.That(actualRows, Is.EqualTo(initialRows));
    }

    private static GeneratingRow ReadRowToGeneratingRow(StreamReadRow readRow)
    {
        return new GeneratingRow(readRow.Number, readRow.Sentence);
    }
}