using System.Text;
using SortTask.Domain;

namespace SortTask.Adapter;

public class StringOph(Encoding encoding) : IStringOph
{
    public OphULong Hash(string value)
    {
        var bytes = encoding.GetBytes(value);

        switch (bytes.Length)
        {
            case < 8:
                Array.Resize(ref bytes, 8);
                break;
            case > 8:
                bytes = bytes[..8];
                break;
        }

        ulong hash = 0;
        for (var i = 0; i < 8; i++)
        {
            hash = (hash << 8) | bytes[i];
        }

        return new OphULong(hash);
    }
}