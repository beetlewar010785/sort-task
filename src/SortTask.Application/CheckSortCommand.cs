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
                if (previousRow.HasValue)
                {
                    if (rowComparer.Compare(row, previousRow.Value) < 0)
                    {
                        return CheckSortResult.Failure(previousRow.Value, row);
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