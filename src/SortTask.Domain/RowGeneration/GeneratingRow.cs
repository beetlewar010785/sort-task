namespace SortTask.Domain.RowGeneration;

public record GeneratingRow(int Number, string Sentence) : IRow
{
}