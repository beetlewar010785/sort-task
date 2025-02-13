using System.Runtime.CompilerServices;
using SortTask.Domain;
using SortTask.Domain.BTree;

namespace SortTask.Application;

public class SortRowsCommand<TOphValue>(
    IBTreeIndexTraverser<TOphValue> indexTraverser,
    IRowLookup rowLookup,
    IRowWriter outputRowWriter
) : ICommand<SortRowsCommand<TOphValue>.Result> where TOphValue : struct
{
    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
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

    public abstract record Result;
}
