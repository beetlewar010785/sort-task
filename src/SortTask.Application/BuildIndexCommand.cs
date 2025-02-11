using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Application;

public class BuildIndexCommand(
    IRowIterator rowIterator,
    IIndexer indexer
) : ICommand<BuildIndexCommand.Param, BuildIndexCommand.Result>
{
    public record Param;

    public abstract record Result;

    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        Param param,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Building Index...";

        await foreach (var rowIteration in rowIterator.ReadAsAsyncEnumerable(cancellationToken))
        {
            await indexer.Index(rowIteration.SentenceOph, rowIteration.Offset, rowIteration.Length, cancellationToken);
            yield return new CommandIteration<Result>(null, operationName);
        }
    }
}