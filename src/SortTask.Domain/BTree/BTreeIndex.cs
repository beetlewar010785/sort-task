namespace SortTask.Domain.BTree;

public readonly struct BTreeIndex(OphULong sentenceOph, long offset, int length)
{
    public OphULong SentenceOph { get; } = sentenceOph;
    public long Offset { get; } = offset;
    public int Length { get; } = length;
}