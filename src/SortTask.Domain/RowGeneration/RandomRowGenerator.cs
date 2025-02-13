using Bogus;

namespace SortTask.Domain.RowGeneration;

public class RandomRowGenerator(
    Random rnd,
    int minNumber = 1,
    int maxNumber = 1000000,
    int minWordsInSentence = 2,
    int maxWordsInSentence = 42
) : IRowGenerator
{
    private readonly Faker _faker = new();

    public IEnumerable<Row> Generate()
    {
        var rowNumber = rnd.Next(minNumber, maxNumber);
        var numWords = rnd.Next(minWordsInSentence, maxWordsInSentence);

        var sentence = string.Join("-", _faker.Hacker.Random.WordsArray(numWords).OrderBy(_ => rnd.Next()));
        yield return new Row(rowNumber, sentence);
    }
}
