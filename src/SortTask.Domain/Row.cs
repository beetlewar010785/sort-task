namespace SortTask.Domain;

public readonly struct Row(int number, string sentence)
{
    public int Number => number;
    public string Sentence => sentence;

    public override string ToString()
    {
        return $"{Number}. {Sentence}";
    }
}