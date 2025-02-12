namespace SortTask.Domain.RowGeneration;

public interface IRowGenerator
{
    IEnumerable<Row> Generate();
}