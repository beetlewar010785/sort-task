using SortTask.Domain;

namespace SortTask.Application;

public class SortRowsCommand<TIndex>(
    IRowReader rowReader,
    IIndexer<TIndex> indexer,
    IIndexFactory<TIndex> indexFactory,
    IIndexTraverser<TIndex> indexTraverser,
    IRowLookup<TIndex> rowLookup,
    IRowWriter rowWriter
) : ISortRowsCommand
    where TIndex : IIndex
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        await foreach (var row in rowReader.ReadAsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var index = indexFactory.CreateIndexFromRow(row);
            await indexer.Index(index);
        }

        await foreach (var index in indexTraverser.Traverse().WithCancellation(cancellationToken))
        {
            var row = await rowLookup.FindRow(index);
            await rowWriter.Write(row);
        }

        await rowWriter.Flush(cancellationToken);
    }
}