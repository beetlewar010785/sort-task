namespace SortTask.Domain;

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