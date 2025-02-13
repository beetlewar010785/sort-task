using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Application;

public class BuildIndexCommand(
    IRowIterator rowIterator,
    IIndexer indexer
) : ICommand<BuildIndexCommand.Result>
{
    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Building Index...";

        await foreach (var rowIteration in rowIterator.ReadAsAsyncEnumerable(cancellationToken))
        {
            await indexer.Index(rowIteration.Row, rowIteration.Offset, rowIteration.Length, cancellationToken);
            yield return new CommandIteration<Result>(null, operationName);
        }
    }

    public abstract record Result;
}
