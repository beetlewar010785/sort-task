using SortTask.Domain;
using SortTask.Domain.RowGeneration;

namespace SortTask.Application;

public class FeedRowCommand(
    Stream targetStream,
    IRowWriter rowWriter,
    IRowGenerator rowGenerator,
    long estimatedSize
) : ICommand<FeedRowCommand.Result>
{
    public IEnumerable<CommandIteration<Result>> Execute()
    {
        const string operationName = "Generating Test Data...";

        while (true)
        {
            var rows = rowGenerator.Generate();
            foreach (var row in rows)
            {
                rowWriter.Write(row);
                yield return new CommandIteration<Result>(
                    null,
                    operationName);
            }

            if (targetStream.Position >= estimatedSize) break;
        }

        rowWriter.Flush();
    }

    public abstract record Result;
}
