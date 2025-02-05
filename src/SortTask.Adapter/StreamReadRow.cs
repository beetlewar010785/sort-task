using SortTask.Domain;

namespace SortTask.Adapter;

public record StreamReadRow(int Number, string Sentence) : IRow
{
}