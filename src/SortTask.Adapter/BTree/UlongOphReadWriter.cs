namespace SortTask.Adapter.BTree;

public class UlongOphReadWriter : IOphReadWriter<ulong>
{
    public int Size => sizeof(ulong);

    public int Write(ulong value, Span<byte> target, int position)
    {
        return BinaryReadWriter.WriteUlong(value, target, position);
    }

    public (ulong, int) Read(ReadOnlySpan<byte> buf, int position)
    {
        return BinaryReadWriter.ReadUong(buf, position);
    }
}