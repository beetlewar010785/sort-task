using System.Runtime.CompilerServices;
using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Application;

public class SortRowsCommand(
    IBTreeIndexTraverser ibTreeIndexTraverser,
    IRowLookup rowLookup,
    IRowWriter outputRowWriter
) : ICommand<SortRowsCommand.Param, SortRowsCommand.Result>
{
    public record Param;

    public abstract record Result;

    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        Param param,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Sorting...";

        var writtenRows = 0;
        await foreach (var index in ibTreeIndexTraverser.IterateAsAsyncEnumerable(cancellationToken))
        {
            var row = await rowLookup.FindRow(index.Offset, index.Length, cancellationToken);
            await outputRowWriter.Write(row, cancellationToken);

            writtenRows++;
            if (writtenRows % AppConst.FlushPeriod == 0)
            {
                await outputRowWriter.Flush(cancellationToken);
            }

            yield return new CommandIteration<Result>(null, operationName);
        }

        await outputRowWriter.Flush(cancellationToken);
    }
}