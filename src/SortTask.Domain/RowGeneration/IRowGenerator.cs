namespace SortTask.Domain;

public interface IRowGenerator
{
    IEnumerable<Row> Generate();
}