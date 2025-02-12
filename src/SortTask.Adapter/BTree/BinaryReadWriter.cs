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

    public static (ulong, int) ReadUlong(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToUInt64(buf[position..]), position + sizeof(ulong));
    }

    public static int WriteInt(int value, Span<byte> target, int position)
    {
        if (!BitConverter.TryWriteBytes(target[position..], value))
        {
            throw new Exception("Failed to write int.");
        }

        return position + sizeof(int);
    }

    public static (int, int) ReadInt(ReadOnlySpan<byte> buf, int position)
    {
        return (BitConverter.ToInt32(buf[position..]), position + sizeof(int));
    }

    public static int WriteBytes(byte[] value, Span<byte> target, int position)
    {
        value.CopyTo(target.Slice(position, value.Length));
        return position + value.Length;
    }

    public static (byte[], int) ReadBytes(ReadOnlySpan<byte> buf, int count, int position)
    {
        var result = buf.Slice(position, count).ToArray();
        return (result, position + count);
    }
}