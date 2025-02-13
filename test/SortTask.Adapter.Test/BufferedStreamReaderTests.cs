using System.Text;

namespace SortTask.Adapter.Test;

public class BufferedStreamReaderTests
{
    [TestCaseSource(nameof(TestCases))]
    public async Task<IEnumerable<BufferedStreamReader.ReadLineResult>> ShouldReadLines(
        string input,
        Encoding encoding)
    {
        var bytes = encoding.GetBytes(input);
        using var ms = new MemoryStream(bytes);
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

        yield return new TestCaseData("a\r\nб\r\n@", Encoding.UTF8)
            .Returns(new[]
            {
                new BufferedStreamReader.ReadLineResult("a", 0, 1),
                new BufferedStreamReader.ReadLineResult("б", 3, 2),
                new BufferedStreamReader.ReadLineResult("@", 7, 1)
            });

        yield return new TestCaseData("a", Encoding.UTF32)
            .Returns(new[]
            {
                new BufferedStreamReader.ReadLineResult("a", 0, 4)
            });

        yield return new TestCaseData("a\r\nb\r\nc", Encoding.UTF32)
            .Returns(new[]
            {
                new BufferedStreamReader.ReadLineResult("a", 0, 4),
                new BufferedStreamReader.ReadLineResult("b", 12, 4),
                new BufferedStreamReader.ReadLineResult("c", 24, 4)
            });

        var longLine1 = new string('a', 10000);
        var longLine2 = new string('b', 2000);
        yield return new TestCaseData($"{longLine1}\n{longLine2}", Encoding.UTF8)
            .Returns(new[]
            {
                new BufferedStreamReader.ReadLineResult(longLine1, 0, longLine1.Length),
                new BufferedStreamReader.ReadLineResult(longLine2, longLine1.Length + 1, longLine2.Length)
            })
            .SetName("Long lines");
    }
}
