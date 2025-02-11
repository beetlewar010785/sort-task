using System.Runtime.CompilerServices;
using SortTask.Domain;

namespace SortTask.Application;

public class CheckSortCommand(
    IRowIterator rowIterator,
    IComparer<Row> rowComparer
) : ICommand<CheckSortCommand.Param, CheckSortCommand.Result>
{
    public record Param;

    public abstract class Result
    {
        public class ResultOk : Result;

        public class ResultFailure(Row precedingRow, Row failedRow) : Result
        {
            public Row PrecedingRow => precedingRow;
            public Row FailedRow => failedRow;
        }

        public static ResultOk Ok() => new();

        public static Result Failure(Row previousRow, Row nextRow) =>
            new ResultFailure(previousRow, nextRow);
    }

    public async IAsyncEnumerable<CommandIteration<Result>> Execute(
        Param param,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string operationName = "Checking Sort...";

        Row? previousRow = null;
        await foreach (var row in rowIterator.ReadAsAsyncEnumerable(cancellationToken))
        {
            if (previousRow != null)
            {
                if (rowComparer.Compare(row.Row, previousRow) < 0)
                {
                    yield return new CommandIteration<Result>(
                        Result.Failure(previousRow, row.Row), operationName);
                    yield break;
                }

                yield return new CommandIteration<Result>(Result.Ok(), operationName);
            }

            previousRow = row.Row;
        }

        yield return new CommandIteration<Result>(Result.Ok(), operationName);
    }
}