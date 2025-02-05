namespace SortTask.Domain;

public class RowComparer : IComparer<IRow>
{
    public int Compare(IRow? x, IRow? y)
    {
        switch (x)
        {
            case null when y == null:
                return 0;
            case null:
                return -1;
        }

        if (y == null)
        {
            return 1;
        }

        var cmp = string.Compare(x.Sentence, y.Sentence, StringComparison.Ordinal);
        return cmp != 0 ? cmp : x.Number.CompareTo(y.Number);
    }
}