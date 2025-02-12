namespace SortTask.Domain;

public readonly struct OphULong(ulong value)
{
    public ulong Value => value;

    public override string ToString()
    {
        return $"{value:D20}";
    }
}

/// <summary>
/// Order-Preserving Hashing
/// </summary>
public interface IOph
{
    public OphULong Hash(ReadOnlyMemory<byte> bytes);
}

public class OphCollisionDetector(IComparer<OphULong> inner) : Comparer<OphULong>
{
    public long CollisionCount { get; private set; }
    public long ComparisonCount { get; private set; }

    public override int Compare(OphULong x, OphULong y)
    {
        ComparisonCount++;
        if (x.Value.CompareTo(y.Value) == 0)
        {
            CollisionCount++;
        }

        return inner.Compare(x, y);
    }
}

public class OphComparer : IComparer<OphULong>
{
    public int Compare(OphULong x, OphULong y)
    {
        return x.Value.CompareTo(y.Value);
    }
}