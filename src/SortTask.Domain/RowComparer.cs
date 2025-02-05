namespace SortTask.Domain;

public class RowComparer : IComparer<IRow>
{
    public int Compare(IRow? x, IRow? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);

        var cmp = string.Compare(x.Sentence, y.Sentence, StringComparison.Ordinal);
        return cmp != 0 ? cmp : x.Number.CompareTo(y.Number);
    }
}