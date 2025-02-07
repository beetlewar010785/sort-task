namespace SortTask.Domain;

public readonly struct ReadRow(int number, string sentence, long position)
{
    public int Number => number;
    public string Sentence => sentence;
    public long Position => position;

    public override string ToString()
    {
        return $"{Number}. {Sentence}";
    }

    public WriteRow ToWriteRow() => new WriteRow(Number, Sentence);
}

public readonly struct WriteRow(int number, string sentence)
{
    public int Number => number;
    public string Sentence => sentence;

    public override string ToString()
    {
        return $"{Number}. {Sentence}";
    }

    public ReadRow ToReadRow(long position) => new(Number, Sentence, position);
}