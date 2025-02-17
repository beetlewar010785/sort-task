using SortTask.Domain;

namespace SortTask.Application;

public class BuildIndexCommand(
    IRowIterator rowIterator,
    IIndexer indexer
) : ICommand<BuildIndexCommand.Result>
{
    public IEnumerable<CommandIteration<Result>> Execute()
    {
        const string operationName = "Building Index...";

        foreach (var rowIteration in rowIterator.IterateOverRows())
        {
            indexer.Index(rowIteration.Row, rowIteration.Offset, rowIteration.Length);
            yield return new CommandIteration<Result>(null, operationName);
        }
    }

    public abstract record Result;
}
