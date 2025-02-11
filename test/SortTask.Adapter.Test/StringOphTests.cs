using System.Text;
using Bogus;
using SortTask.Domain;

namespace SortTask.Adapter.Test;

public class StringOphTests
{
    [TestCaseSource(nameof(TestCases))]
    public void Should_Preserve_Order(Encoding encoding, int numStrings)
    {
        var sut = new StringOph(encoding);

        Faker faker = new();
        var strings = Enumerable.Range(0, numStrings)
            .Select(_ => faker.Random.Words(1))
            .ToList();
        strings.Sort(string.CompareOrdinal);
        var hashes = strings.Select(sut.Hash);

        Assert.That(hashes, Is.Ordered.Using(new BigEndianStringOphComparer()), string.Join(";", strings));
    }

    private static IEnumerable<TestCaseData> TestCases() =>
    [
        new(Encoding.ASCII, 1000),
        new(Encoding.UTF8, 1000),
        new(Encoding.Unicode, 1000),
        new(Encoding.UTF8, 1000),
    ];
}