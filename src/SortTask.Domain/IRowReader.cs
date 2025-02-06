namespace SortTask.Domain;

public interface IRowReader
{
    IAsyncEnumerable<Row> ReadAsAsyncEnumerable();
}