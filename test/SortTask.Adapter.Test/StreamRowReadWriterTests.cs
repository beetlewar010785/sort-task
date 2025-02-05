using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter.Test;

public class StreamRowReadWriterTests
{
    [Test]
    public async Task Should_Read_All_Written_Rows()
    {
        using var ms = new MemoryStream();

        var initialRows = new[]
        {
            new Row(1, "some string 1"),
            new Row(2, "some string 2"),
            new Row(3, "some another string")
        };

        var sut = new StreamRowReadWriter(ms, Encoding.UTF8);
        foreach (var row in initialRows)
        {
            await sut.Write(row);
        }

        await sut.Flush(CancellationToken.None);
        ms.Position = 0;

        var actualRows = await sut.ReadAsAsyncEnumerable().ToArrayAsync();
        Assert.That(actualRows, Is.EqualTo(initialRows));
    }
}