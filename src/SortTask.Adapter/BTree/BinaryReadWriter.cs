namespace SortTask.Adapter.BTree;

public static class BinaryReadWriter
{
    public static int WriteLong(long value, Span<byte> target, int position)
    {
        return !BitConverter.TryWriteBytes(target[position..], value)
            ? throw new InvalidOperationException("Failed to write long")
            : position + sizeof(long);
    }

    public static (long, int) ReadLong(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToInt64(buf[position..]), position + sizeof(long));
    }

    public static int WriteUlong(ulong value, Span<byte> target, int position)
    {
        return !BitConverter.TryWriteBytes(target[position..], value)
            ? throw new InvalidOperationException("Failed to write ulong")
            : position + sizeof(ulong);
    }

    public static (ulong, int) ReadUlong(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToUInt64(buf[position..]), position + sizeof(ulong));
    }

    public static int WriteInt(int value, Span<byte> target, int position)
    {
        return !BitConverter.TryWriteBytes(target[position..], value)
            ? throw new InvalidOperationException("Failed to write int.")
            : position + sizeof(int);
    }

    public static (int, int) ReadInt(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToInt32(buf[position..]), position + sizeof(int));
    }

    public static int WriteShort(short value, Span<byte> target, int position)
    {
        return !BitConverter.TryWriteBytes(target[position..], value)
            ? throw new InvalidOperationException("Failed to write short.")
            : position + sizeof(short);
    }

    public static (short, int) ReadShort(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToInt16(buf[position..]), position + sizeof(short));
    }
}
