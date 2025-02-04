using System.Text;
using SortTask.Domain;

namespace SortTask.TestFileCreator;

public class FileCreator(string fileName, long fileSize)
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        const int maxRowNumber = 100_000;
        const int maxWordsInSentence = 5;
        const int repeatRowPeriod = 10;
        const int maxRepeatNumber = 1;
        const int refreshRepeatingRowsPeriod = 2;
        const int flushPeriod = 10000;

        await using var file = File.Create(fileName);

        var rowWriter = new RowReadWriter(file, Encoding.UTF8);
        var rnd = new Random();

        var rowsGenerator = new RandomlyRowsRepeater(
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

        var progressRenderer = new ProgressRenderer();
        try
        {
            var writtenRows = 0;
            while (true)
            {
                var rows = rowsGenerator.Generate();
                foreach (var row in rows)
                {
                    await rowWriter.Write(row);

                    writtenRows++;
                    if (writtenRows % flushPeriod != 0) continue;

                    await rowWriter.Flush(cancellationToken);

                    var progress = Math.Min(100 * file.Position / (double)fileSize, 100);
                    progressRenderer.Render((int)progress);
                }

                if (file.Position >= fileSize)
                {
                    break;
                }
            }

            await rowWriter.Flush(cancellationToken);
        }
        finally
        {
            progressRenderer.Clear();
        }
    }
}