using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public readonly struct StreamBTreeIndex(long rowPosition) : IIndex
{
    public long RowPosition => rowPosition;

    public override string ToString()
    {
        return rowPosition.ToString();
    }
}