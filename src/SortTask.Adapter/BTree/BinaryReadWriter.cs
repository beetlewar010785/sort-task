using SortTask.Domain.BTree;

namespace SortTask.Adapter.BTree;

public static class BinaryReadWriter
{
    public static int WriteLong(long value, Span<byte> target, int position)
    {
        if (!BitConverter.TryWriteBytes(target[position..], value))
        {
            throw new Exception("Failed to write long.");
        }

        return position + sizeof(long);
    }

    public static (long, int) ReadLong(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToInt64(buf[position..]), position + sizeof(long));
    }

    public static int WriteUlong(ulong value, Span<byte> target, int position)
    {
        if (!BitConverter.TryWriteBytes(target[position..], value))
        {
            throw new Exception("Failed to write ulong.");
        }

        return position + sizeof(ulong);
    }

    public static (ulong, int) ReadUong(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToUInt64(buf[position..]), position + sizeof(ulong));
    }

    public static int WriteInt(int count, Span<byte> target, int position)
    {
        if (!BitConverter.TryWriteBytes(target[position..], count))
        {
            throw new Exception("Failed to write int.");
        }

        return position + sizeof(int);
    }

    public static (int, int) ReadInt(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToInt32(buf[position..]), position + sizeof(int));
    }
}