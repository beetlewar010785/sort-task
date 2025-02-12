namespace SortTask.Domain;

/// <summary>
/// Order-Preserving Hashing
/// </summary>
public interface IOph<out TOphValue>
    where TOphValue : struct
{
    public TOphValue Hash(ReadOnlyMemory<byte> bytes);
}

public class OphCollisionDetector<TOphValue>(IComparer<TOphValue> inner) : Comparer<TOphValue>
    where TOphValue : struct
{
    public long CollisionCount { get; private set; }
    public long ComparisonCount { get; private set; }

    public override int Compare(TOphValue x, TOphValue y)
    {
        var result = inner.Compare(x, y);
        ComparisonCount++;
        if (result == 0)
        {
            CollisionCount++;
        }

        return result;
    }
}

// public class OphUlongComparer : IComparer<OphUlongValue>
// {
//     public int Compare(OphUlongValue x, OphUlongValue y)
//     {
//         return x.Value.CompareTo(y.Value);
//     }
// }