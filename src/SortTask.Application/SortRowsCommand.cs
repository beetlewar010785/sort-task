using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Application;

public class SortRowsCommand<TIndex>(
    IIndexTraverser<TIndex> indexTraverser,
    IRowLookup<TIndex> rowLookup,
    IRowWriter rowWriter
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
        await foreach (var index in indexTraverser.Traverse(cancellationToken))
        {
            var row = await rowLookup.FindRow(index, cancellationToken);
            await rowWriter.Write(row.ToWriteRow());

            writtenRows++;
            if (writtenRows % AppConst.FlushPeriod == 0)
            {
                await rowWriter.Flush(cancellationToken);
            }

            yield return new CommandIteration<Result>(null, operationName);
        }

        await rowWriter.Flush(cancellationToken);
    }
}