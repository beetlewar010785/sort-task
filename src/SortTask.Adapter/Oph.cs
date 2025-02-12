using SortTask.Domain;

namespace SortTask.Adapter;

public class Oph(int numWords) : IOph<OphValue>
{
    public int NumWords => numWords;

    public OphValue Hash(ReadOnlySpan<byte> bytes)
    {
        var totalBytes = numWords * 8;
        Span<byte> b = stackalloc byte[totalBytes];

        var copyLength = Math.Min(bytes.Length, totalBytes);
        bytes[..copyLength].CopyTo(b);

        var values = new ulong[numWords];

        for (var i = 0; i < numWords; i++)
        {
            ulong value = 0;
            for (var j = 0; j < 8; j++)
            {
                value = (value << 8) | b[i * 8 + j];
            }

            values[i] = value;
        }

        return new OphValue(values);
    }
}

public readonly struct OphValue(ulong[] words)
{
    public ulong[] Words { get; } = words;
}

public class OphComparer : IComparer<OphValue>
{
    public int Compare(OphValue x, OphValue y)
    {
        if (x.Words.Length != y.Words.Length)
            throw new ArgumentException($"x words {x.Words.Length} != y words {y.Words.Length}");

        for (var i = 0; i < x.Words.Length; i++)
        {
            var result = x.Words[i].CompareTo(y.Words[i]);
            if (result != 0) return result;
        }

        return 0;
    }
}