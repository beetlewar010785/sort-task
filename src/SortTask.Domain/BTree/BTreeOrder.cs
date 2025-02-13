namespace SortTask.Domain.BTree;

public class BTreeOrder(int value)
{
    public int MaxIndices { get; } = value * 2 + 1;

    public int MaxChildren { get; } = value * 2 + 2;

    public bool IsOverflowed(int numIndices)
    {
        return numIndices > MaxIndices
            ? throw new InvalidOperationException(
                $"Maximum allowed number of indices exceeded. Allowed: ${MaxIndices}, got: {numIndices}")
            : numIndices == MaxIndices;
    }
}
