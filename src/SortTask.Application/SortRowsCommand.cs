using SortTask.Domain;

namespace SortTask.Application;

public class SortRowsCommand<TIndex>(
    IRowReader rowReader,
    IIndexer<TIndex> indexer,
    IIndexFactory<TIndex> indexFactory,
    IIndexTraverser<TIndex> indexTraverser,
    IRowLookup<TIndex> rowLookup,
    IRowWriter rowWriter,
    IProgressRenderer progressRenderer,
    Stream sourceStream,
    Stream targetStream // todo move out
)
    where TIndex : IIndex
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        try
        {
            const int flushPeriod = 10000;

            await foreach (var row in rowReader.ReadAsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                var index = indexFactory.CreateIndexFromRow(row);
                await indexer.Index(index);

                var progress = Math.Min(100 * sourceStream.Position / sourceStream.Length, 100);
                progressRenderer.Render((int)progress, "Indexing Data");
            }

            progressRenderer.Clear();

            var writtenRows = 0;
            await foreach (var index in indexTraverser.Traverse().WithCancellation(cancellationToken))
            {
                var row = await rowLookup.FindRow(index);
                await rowWriter.Write(row);

                writtenRows++;
                if (writtenRows % flushPeriod != 0) continue;

                await rowWriter.Flush(cancellationToken);

                var progress = Math.Min(100 * targetStream.Position / sourceStream.Length, 100);
                progressRenderer.Render((int)progress, "Writing Sorted Output");
            }

            await rowWriter.Flush(cancellationToken);
        }
        finally
        {
            progressRenderer.Clear();
        }
    }
}