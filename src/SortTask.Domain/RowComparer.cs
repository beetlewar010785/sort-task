namespace SortTask.Domain;

public class RowComparer : IComparer<ReadRow>, IComparer<WriteRow>
{
    public int Compare(WriteRow x, WriteRow y)
    {
        var cmp = string.Compare(x.Sentence, y.Sentence, StringComparison.Ordinal);
        return cmp != 0 ? cmp : x.Number.CompareTo(y.Number);
    }

    public int Compare(ReadRow x, ReadRow y)
    {
        return ((IComparer<WriteRow>)this).Compare(x.ToWriteRow(), y.ToWriteRow());
    }
}