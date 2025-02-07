using System.Text;
using SortTask.Domain;
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
            new WriteRow(1, "some string 1"),
            new WriteRow(2, "some string 2"),
            new WriteRow(3, "some another string")
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
            .Select(rr => rr.ToWriteRow())
            .ToList();
        Assert.That(actualRows, Is.EqualTo(initialRows));
    }
}