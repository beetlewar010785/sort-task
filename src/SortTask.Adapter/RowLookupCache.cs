using BitFaster.Caching.Lru;
using SortTask.Domain;

namespace SortTask.Adapter;

public class RowLookupCache(IRowLookup inner, int capacity) : IRowLookup
{
    private readonly ConcurrentLru<long, Row> _lru = new(1, capacity, new OffsetComparer());

    public async Task<Row> FindRow(long offset, int length, CancellationToken cancellationToken)
    {
        if (_lru.TryGet(offset, out var row))
        {
            return row;
        }

        row = await inner.FindRow(offset, length, cancellationToken);

        _lru.AddOrUpdate(offset, row);

        return row;
    }

    private class OffsetComparer : IEqualityComparer<long>
    {
        public bool Equals(long x, long y)
        {
            return x == y;
        }

        public int GetHashCode(long obj)
        {
            return obj.GetHashCode();
        }
    }
}