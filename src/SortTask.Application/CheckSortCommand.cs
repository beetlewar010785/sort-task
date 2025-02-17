using SortTask.Domain;

namespace SortTask.Application;

public class CheckSortCommand(
    IRowIterator rowIterator,
    IComparer<Row> rowComparer
) : ICommand<CheckSortCommand.Result>
{
    public IEnumerable<CommandIteration<Result>> Execute()
    {
        const string operationName = "Checking Sort...";

        Row? previousRow = null;
        foreach (var row in rowIterator.IterateOverRows())
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

    public abstract class Result
    {
        public static ResultOk Ok()
        {
            return new ResultOk();
        }

        public static Result Failure(Row previousRow, Row nextRow)
        {
            return new ResultFailure(previousRow, nextRow);
        }

        public class ResultOk : Result;

        public class ResultFailure(Row precedingRow, Row failedRow) : Result
        {
            public Row PrecedingRow => precedingRow;
            public Row FailedRow => failedRow;
        }
    }
}
