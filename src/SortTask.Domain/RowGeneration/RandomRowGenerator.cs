using Bogus;

namespace SortTask.Domain.RowGeneration;

public class RandomRowGenerator(
    Random rnd,
    int maxRowNumber,
    int maxWordsInSentence
) : IRowGenerator
{
    private readonly Faker _faker = new();

    public IEnumerable<Row> Generate()
    {
        var rowNumber = rnd.Next(1, maxRowNumber);
        var numWords = rnd.Next(1, maxWordsInSentence);
        var sentence = string.Join(" ", _faker.Random.Words(numWords));
        yield return new Row(rowNumber, sentence);
    }
}