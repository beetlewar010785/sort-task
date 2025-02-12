namespace SortTask.Domain.BTree;

public readonly struct BTreeIndex<TOphValue>(TOphValue ophValue, long offset, int length)
    where TOphValue : struct
{
    public TOphValue OphValue { get; } = ophValue;
    public long Offset { get; } = offset;
    public int Length { get; } = length;
}