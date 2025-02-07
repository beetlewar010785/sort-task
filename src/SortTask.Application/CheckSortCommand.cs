using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Application;

public class CheckSortCommand(
    IRowReader rowReader,
    IComparer<ReadRow> rowComparer
) : ICommand<CheckSortCommand.Param, CheckSortCommand.Result>
{
    public record Param;

    public abstract class Result
    {
        public class ResultOk : Result;

        public class ResultFailure(ReadRow precedingRow, ReadRow failedRow) : Result
        {
            public ReadRow PrecedingRow => precedingRow;
            public ReadRow FailedRow => failedRow;
        }

        public static ResultOk Ok() => new();

        public static Result Failure(ReadRow previousRow, ReadRow nextRow) =>
            new ResultFailure(previousRow, nextRow);
    }

    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        Param param,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Checking Sort...";

        ReadRow? previousRow = null;
        await foreach (var row in rowReader.ReadAsAsyncEnumerable(cancellationToken))
        {
            if (previousRow.HasValue)
            {
                if (rowComparer.Compare(row, previousRow.Value) < 0)
                {
                    yield return new CommandIteration<Result>(
                        Result.Failure(previousRow.Value, row), operationName);
                    yield break;
                }

                yield return new CommandIteration<Result>(Result.Ok(), operationName);
            }

            previousRow = row;
        }

        yield return new CommandIteration<Result>(Result.Ok(), operationName);
    }
}