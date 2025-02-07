using SortTask.Domain;
using SortTask.Domain.RowGeneration;

namespace SortTask.Application;

public class FeedRowCommand(
    Stream targetStream,
    IRowWriter rowWriter,
    IRowGenerator rowGenerator,
    IProgressRenderer progressRenderer
)
{
    public async Task Execute(long estimatedSize, CancellationToken cancellationToken)
    {
        const int flushPeriod = 10000;

        try
        {
            var writtenRows = 0;
            while (true)
            {
                var rows = rowGenerator.Generate();
                foreach (var row in rows)
                {
                    await rowWriter.Write(row);

                    writtenRows++;
                    if (writtenRows % flushPeriod != 0) continue;

                    await rowWriter.Flush(cancellationToken);

                    var progress = Math.Min(100 * targetStream.Position / (double)estimatedSize, 100);
                    progressRenderer.Render((int)progress, "Generating Test Data");
                }

                if (targetStream.Position >= estimatedSize)
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