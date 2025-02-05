using SortTask.Domain;

namespace SortTask.Application;

public class SortRowsCommand<TRow, TIndex>(
    IRowReader<TRow> rowReadWriter,
    IIndexer<TIndex> indexer,
    IIndexFactory<TIndex, TRow> indexFactory
)
    where TRow : IRow
    where TIndex : IIndex
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        await foreach (var row in rowReadWriter.ReadAsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var index = indexFactory.CreateIndexFromRow(row);
            await indexer.Index(index);
        }
    }
}