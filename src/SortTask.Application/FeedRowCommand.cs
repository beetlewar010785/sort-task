using System.Runtime.CompilerServices;
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
    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Generating Test Data...";

        while (true)
        {
            var rows = rowGenerator.Generate();
            foreach (var row in rows)
            {
                await rowWriter.Write(row, cancellationToken);
                yield return new CommandIteration<Result>(
                    null,
                    operationName);
            }

            if (targetStream.Position >= estimatedSize) break;
        }

        await rowWriter.Flush(cancellationToken);
    }

    public abstract record Result;
}
