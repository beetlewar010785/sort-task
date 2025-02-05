namespace SortTask.Domain;

// public record Row(int Number, string Sentence);
public interface IRow
{
    int Number { get; }
    string Sentence { get; }
}