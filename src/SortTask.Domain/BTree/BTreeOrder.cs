namespace SortTask.Domain.BTree;

public record BTreeOrder(int Value)
{
    public bool IsOverflowed(int numIndices)
    {
        var numExceededIndices = Value * 2 + 1;
        if (numIndices > numExceededIndices)
        {
            throw new InvalidOperationException(
                $"Maximum allowed number of indices exceeded. Allowed: ${numExceededIndices}, got: {numIndices}");
        }

        return numIndices == numExceededIndices;
    }
}