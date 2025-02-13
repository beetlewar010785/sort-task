namespace SortTask.Adapter.BTree;

public class OphReadWriter(int numWords) : IOphReadWriter<OphValue>
{
    public int Size => numWords * sizeof(ulong);

    public int Write(OphValue value, Span<byte> target, int position)
    {
        foreach (var word in value.Words) position = BinaryReadWriter.WriteUlong(word, target, position);

        return position;
    }

    public (OphValue, int) Read(ReadOnlySpan<byte> buf, int position)
    {
        var words = new ulong[numWords];
        for (var i = 0; i < numWords; i++)
        {
            (var word, position) = BinaryReadWriter.ReadUlong(buf, position);
            words[i] = word;
        }

        return ValueTuple.Create(new OphValue(words), position);
    }
}
