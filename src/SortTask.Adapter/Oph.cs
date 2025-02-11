using SortTask.Domain;

namespace SortTask.Adapter;

public class Oph : IOph
{
    public OphULong Hash(ReadOnlyMemory<byte> bytes)
    {
        Span<byte> b = stackalloc byte[8];
        var src = bytes.Span;

        var copyLength = Math.Min(src.Length, 8);
        src[..copyLength].CopyTo(b);

        ulong hash = 0;
        for (var i = 0; i < 8; i++)
        {
            hash = (hash << 8) | b[i];
        }

        return new OphULong(hash);
    }
}