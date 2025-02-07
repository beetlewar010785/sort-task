using SortTask.Domain;

namespace SortTask.Adapter.Test;

public class StreamRowReadWriterTests
{
    [Test]
    public async Task Should_Read_All_Written_Rows()
    {
        var initialRows = new[]
        {
            new WriteRow(1, "some string 1"),
            new WriteRow(2, "some string 2"),
            new WriteRow(3, "some another string")
        };

        using var ms = new MemoryStream();
        await using var streamWriter = new StreamWriter(ms, AdapterConst.Encoding, leaveOpen: true);
        var writer = new StreamRowWriter(streamWriter);
        foreach (var row in initialRows)
        {
            await writer.Write(row);
        }

        await writer.Flush(CancellationToken.None);
        ms.Position = 0;

        using var streamReader = new StreamReader(ms, AdapterConst.Encoding, leaveOpen: true);
        var reader = new StreamRowReader(streamReader);

        var actualRows = (await reader.ReadAsAsyncEnumerable(CancellationToken.None).ToArrayAsync())
            .Select(rr => rr.ToWriteRow())
            .ToList();
        Assert.That(actualRows, Is.EqualTo(initialRows));
    }
}