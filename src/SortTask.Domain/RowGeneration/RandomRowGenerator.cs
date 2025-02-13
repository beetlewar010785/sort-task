using Bogus;

namespace SortTask.Domain.RowGeneration;

public class RandomRowGenerator(
    Random rnd,
    int minNumber = 1,
    int maxNumber = 10000,
    int minWordsInSentence = 1,
    int maxWordsInSentence = 10
) : IRowGenerator
{
    private readonly Faker _faker = new();

    public IEnumerable<Row> Generate()
    {
        var rowNumber = rnd.Next(minNumber, maxNumber);
        var numWords = rnd.Next(minWordsInSentence, maxWordsInSentence);

        var sentence = string.Join(" ", _faker.Hacker.Random.WordsArray(numWords));
        yield return new Row(rowNumber, sentence);
    }
}
