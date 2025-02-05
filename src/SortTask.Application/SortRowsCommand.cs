using SortTask.Domain;

namespace SortTask.Application;

public class SortRowsCommand<TRow>(IRowReader<TRow> rowReadWriter, IRowIndexer<TRow> rowIndexer)
    where TRow : IRow
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        await foreach (var row in rowReadWriter.ReadAsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            await rowIndexer.IndexRow(row);
        }
    }
}