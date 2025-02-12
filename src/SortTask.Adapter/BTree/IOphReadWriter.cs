namespace SortTask.Adapter.BTree;

public interface IOphReadWriter<TOphValue>
    where TOphValue : struct
{
    int Size { get; }

    int Write(TOphValue value, Span<byte> target, int position);

    (TOphValue, int) Read(ReadOnlySpan<byte> buf, int position);
}