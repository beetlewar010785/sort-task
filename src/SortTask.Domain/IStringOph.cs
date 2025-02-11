namespace SortTask.Domain;

public record OphULong(ulong Value)
{
    public override string ToString()
    {
        return $"{Value:D20}";
    }
}

/// <summary>
/// Order-Preserving Hashing
/// </summary>
public interface IStringOph
{
    public OphULong Hash(string value);
}

public class OphCollisionDetector(IComparer<OphULong> inner) : Comparer<OphULong>
{
    public int CollisionNumber { get; private set; }

    public override int Compare(OphULong? x, OphULong? y)
    {
        if (x != null && y != null && x.Value.CompareTo(y.Value) == 0)
        {
            CollisionNumber++;
        }

        return inner.Compare(x, y);
    }
}

public class BigEndianStringOphComparer : IComparer<OphULong>
{
    public int Compare(OphULong? x, OphULong? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        return x.Value.CompareTo(y.Value);
    }
}