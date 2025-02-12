using System.Runtime.CompilerServices;
using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Application;

public class SortRowsCommand<TOphValue>(
    IBTreeIndexTraverser<TOphValue> indexTraverser,
    IRowLookup rowLookup,
    IRowWriter outputRowWriter
) : ICommand<SortRowsCommand<TOphValue>.Param, SortRowsCommand<TOphValue>.Result> where TOphValue : struct
{
    public record Param;

    public abstract record Result;

    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        Param param,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Sorting...";

        await foreach (var index in indexTraverser.IterateAsAsyncEnumerable(cancellationToken))
        {
            var row = await rowLookup.FindRow(index.Offset, index.Length, cancellationToken);
            await outputRowWriter.Write(row, cancellationToken);
            yield return new CommandIteration<Result>(null, operationName);
        }

        await outputRowWriter.Flush(cancellationToken);
    }
}