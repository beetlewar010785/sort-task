using SortTask.Domain;

namespace SortTask.Application;

public class CheckSortCommand(
    Stream stream,
    IRowReader rowReader,
    IComparer<Row> rowComparer,
    IProgressRenderer progressRenderer
)
{
    public abstract class CheckSortResult
    {
        public class CheckSortResultOk : CheckSortResult
        {
        }

        public class CheckSortResultFailure(Row precedingRow, Row failedRow) : CheckSortResult
        {
            public Row PrecedingRow => precedingRow;
            public Row FailedRow => failedRow;
        }

        public static CheckSortResultOk Ok() => new CheckSortResultOk();

        public static CheckSortResult Failure(Row previousRow, Row nextRow) =>
            new CheckSortResultFailure(previousRow, nextRow);
    }

    public async Task<CheckSortResult> Execute(CancellationToken cancellationToken)
    {
        try
        {
            Row? previousRow = null;
            await foreach (var row in rowReader.ReadAsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                if (previousRow != null)
                {
                    if (rowComparer.Compare(row, previousRow) < 0)
                    {
                        return CheckSortResult.Failure(previousRow, row);
                        throw new Exception($"The row {row} is less than the preceding row {previousRow}.");
                    }
                }

                previousRow = row;

                var progress = Math.Min(100 * stream.Position / (double)stream.Length, 100);
                progressRenderer.Render((int)progress, "Sort Checking");
            }
        }
        finally
        {
            progressRenderer.Clear();
        }

        return CheckSortResult.Ok();
    }
}