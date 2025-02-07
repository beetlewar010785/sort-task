namespace SortTask.Domain.RowGeneration;

public interface IRowGenerator
{
    IEnumerable<WriteRow> Generate();
}