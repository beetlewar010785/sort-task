using BitFaster.Caching.Lru;
using SortTask.Domain;

namespace SortTask.Adapter;

public class RowLookupCache(IRowLookup inner, int capacity = 10000) : IRowLookup
{
    private readonly ConcurrentLru<long, Row> _lru = new(
        1,
        capacity,
        EqualityComparer<long>.Default);

    public long FindRowSkipCount { get; private set; }
    public long FindRowExecuteCount { get; private set; }

    public Row FindRow(long offset, int length)
    {
        if (_lru.TryGet(offset, out var row))
        {
            FindRowSkipCount++;
            return row;
        }

        row = inner.FindRow(offset, length);
        FindRowExecuteCount++;

        _lru.AddOrUpdate(offset, row);

        return row;
    }
}
