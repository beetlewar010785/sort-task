namespace SortTask.Domain;

public class RowComparer : IComparer<Row>
{
    public int Compare(Row? x, Row? y)
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