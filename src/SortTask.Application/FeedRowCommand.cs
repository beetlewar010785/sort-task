using System.Runtime.CompilerServices;
using SortTask.Domain;
using SortTask.Domain.RowGeneration;

namespace SortTask.Application;

public class FeedRowCommand(
    Stream targetStream,
    IRowReadWriter rowWriter,
    IRowGenerator rowGenerator,
    long estimatedSize
) : ICommand<FeedRowCommand.Param, FeedRowCommand.Result>
{
    public record Param;

    public abstract record Result;

    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        Param param,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Generating Test Data...";

        var writtenRows = 0;
        while (true)
        {
            var rows = rowGenerator.Generate();
            foreach (var row in rows)
            {
                await rowWriter.Write(row, cancellationToken);

                writtenRows++;
                if (writtenRows % AppConst.FlushPeriod == 0)
                {
                    await rowWriter.Flush(cancellationToken);
                }

                yield return new CommandIteration<Result>(
                    null,
                    operationName);
            }

            if (targetStream.Position >= estimatedSize)
            {
                break;
            }
        }

        await rowWriter.Flush(cancellationToken);
    }
}