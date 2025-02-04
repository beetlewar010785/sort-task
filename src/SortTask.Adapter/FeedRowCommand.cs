using System.Text;
using SortTask.Application;
using SortTask.Domain;

namespace SortTask.Adapter;

public class FeedRowCommand(string fileName, long fileSize)
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        const int maxRowNumber = 100_000;
        const int maxWordsInSentence = 5;
        const int repeatRowPeriod = 10;
        const int maxRepeatNumber = 1;
        const int refreshRepeatingRowsPeriod = 2;

        await using var file = File.Create(fileName);

        var rowWriter = new RowReadWriter(file, Encoding.UTF8);
        var rnd = new Random();

        var rowsGenerator = new RowGenerationRepeater(
            new RandomRowGenerator(
                rnd,
                maxRowNumber: maxRowNumber,
                maxWordsInSentence: maxWordsInSentence
            ),
            rnd,
            repeatPeriod: repeatRowPeriod,
            maxRepeatNumber: maxRepeatNumber,
            refreshRepeatingRowsPeriod: refreshRepeatingRowsPeriod
        );

        var progressRenderer = new ConsoleProgressRenderer();
        var rowFeeder = new RowFeeder(
            file,
            fileSize,
            rowWriter,
            rowsGenerator,
            progressRenderer
        );

        await rowFeeder.Execute(cancellationToken);
    }
}