using System.Text;

namespace SortTask.Adapter.Test;

public class BufferedStreamReaderTests
{
    [TestCaseSource(nameof(TestCases))]
    public async Task<IEnumerable<BufferedStreamReader.ReadLineResult>> Should_Read_Lines(
        string input,
        Encoding encoding)
    {
        using var ms = new MemoryStream(encoding.GetBytes(input));
        var sut = new BufferedStreamReader(ms, encoding);
        var lines = new List<BufferedStreamReader.ReadLineResult>();
        while (true)
        {
            var line = await sut.ReadLine(CancellationToken.None);
            if (line == null) break;

            lines.Add(line);
        }

        return lines;
    }

    private static IEnumerable<TestCaseData> TestCases()
    {
        yield return new TestCaseData("a", Encoding.ASCII)
            .Returns(new[]
            {
                new BufferedStreamReader.ReadLineResult("a", 0, 1)
            });

        yield return new TestCaseData("a\nb", Encoding.UTF8)
            .Returns(
                new[]
                {
                    new BufferedStreamReader.ReadLineResult("a", 0, 1),
                    new BufferedStreamReader.ReadLineResult("b", 2, 1)
                });

        yield return new TestCaseData("a\r\nb\nc\r\n", Encoding.UTF8)
            .Returns(new[]
            {
                new BufferedStreamReader.ReadLineResult("a", 0, 1),
                new BufferedStreamReader.ReadLineResult("b", 3, 1),
                new BufferedStreamReader.ReadLineResult("c", 5, 1)
            });
    }
}