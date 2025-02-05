namespace SortTask.Domain.RowGeneration;

public interface IRowGenerator
{
    IEnumerable<GeneratingRow> Generate();
}