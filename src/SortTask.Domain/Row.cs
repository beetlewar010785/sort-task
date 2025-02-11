namespace SortTask.Domain;

public record Row(int Number, string Sentence)
{
    public override string ToString()
    {
        return $"{Number}. {Sentence}";
    }
}

public record RowWithOffset(Row Row, long Offset, long Length);