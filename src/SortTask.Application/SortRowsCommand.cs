using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Application;

public class SortRowsCommand<TIndex>(
    IIndexTraverser<TIndex> indexTraverser,
    IRowLookup<TIndex> rowLookup,
    IRowReadWriter outputRowReadWriter
) : ICommand<SortRowsCommand<TIndex>.Param, SortRowsCommand<TIndex>.Result>
    where TIndex : IIndex
{
    public record Param;

    public abstract record Result;

    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        Param param,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Sorting...";

        var writtenRows = 0;
        await foreach (var index in indexTraverser.IterateAsAsyncEnumerable(cancellationToken))
        {
            var row = await rowLookup.FindRow(index, cancellationToken);
            await outputRowReadWriter.Write(row, cancellationToken);

            writtenRows++;
            if (writtenRows % AppConst.FlushPeriod == 0)
            {
                await outputRowReadWriter.Flush(cancellationToken);
            }

            yield return new CommandIteration<Result>(null, operationName);
        }

        await outputRowReadWriter.Flush(cancellationToken);
    }
}