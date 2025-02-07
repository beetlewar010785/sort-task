using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Application;

public class BuildIndexCommand<TIndex>(
    IIndexFactory<TIndex> indexFactory,
    IRowReader rowReader,
    IIndexer<TIndex> indexer
) : ICommand<BuildIndexCommand<TIndex>.Param, BuildIndexCommand<TIndex>.Result>
    where TIndex : IIndex
{
    public record Param;

    public abstract record Result;

    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        Param param,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Building Index...";
        await foreach (var row in rowReader.ReadAsAsyncEnumerable(cancellationToken))
        {
            var index = indexFactory.CreateIndexFromRow(row);
            await indexer.Index(index, cancellationToken);
            yield return new CommandIteration<Result>(null, operationName);
        }
    }
}