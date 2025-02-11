using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Application;

public class BuildIndexCommand<TIndex>(
    IIndexFactory<TIndex> indexFactory,
    IRowIterator rowIterator,
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

        await foreach (var row in rowIterator.ReadAsAsyncEnumerable(cancellationToken))
        {
            var index = indexFactory.CreateIndexFromRow(row.Row, row.Offset, row.Length);
            await indexer.Index(index, cancellationToken);
            yield return new CommandIteration<Result>(null, operationName);
        }
    }
}