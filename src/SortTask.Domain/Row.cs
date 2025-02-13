namespace SortTask.Domain;

public record Row(int Number, string Sentence)
{
    public override string ToString()
    {
        return $"{Number}. {Sentence}";
    }
}

public class RowComparer : IComparer<Row>
{
    public int Compare(Row? x, Row? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        var cmp = string.CompareOrdinal(x.Sentence, y.Sentence);
        return cmp != 0 ? cmp : x.Number.CompareTo(y.Number);
    }
}
