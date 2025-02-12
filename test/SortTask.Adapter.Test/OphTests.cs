using System.Text;
using Bogus;

namespace SortTask.Adapter.Test;

public class UlongOphTests
{
    [TestCaseSource(nameof(TestCases))]
    public void Should_Preserve_Order(Encoding encoding, int numStrings)
    {
        Faker faker = new();
        var initialStrings = Enumerable.Range(0, numStrings)
            .Select(_ => faker.Random.Words(1))
            .ToList();
        initialStrings.Sort(string.CompareOrdinal);

        var comparer = new OphComparer();

        var oph = new Oph(2);
        var hashes = initialStrings
            .Select(s => oph.Hash(encoding.GetBytes(s)));

        Assert.That(hashes, Is.Ordered.Using(comparer), string.Join(";", initialStrings));
    }

    private static IEnumerable<TestCaseData> TestCases() =>
    [
        new(Encoding.ASCII, 1000),
        new(Encoding.UTF8, 2000),
        new(Encoding.Unicode, 3000),
        new(Encoding.UTF8, 4000)
    ];
}