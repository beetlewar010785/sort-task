using SortTask.Domain;

namespace SortTask.Application;

public class SortRowsCommand(IRowReadWriter rowReadWriter, IRowIndexer rowIndexer)
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        await foreach (var row in rowReadWriter.ReadAsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            await rowIndexer.IndexRow(row);
        }
    }
}