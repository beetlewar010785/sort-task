namespace SortTask.Domain;

public record RowIteration(Row Row, OphULong SentenceOph, long Offset, int Length);

public interface IRowIterator
{
    IAsyncEnumerable<RowIteration> ReadAsAsyncEnumerable(CancellationToken cancellationToken);
}