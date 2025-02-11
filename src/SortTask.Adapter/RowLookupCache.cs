using BitFaster.Caching.Lru;
using SortTask.Domain;

namespace SortTask.Adapter;

public class RowLookupCache<TIndex>(
    IRowLookup<TIndex> inner,
    IEqualityComparer<TIndex> indexComparer,
    int capacity
)
    : IRowLookup<TIndex>
    where TIndex : IIndex
{
    private readonly ConcurrentLru<TIndex, Row> _lru = new(
        1,
        capacity,
        indexComparer);

    public async Task<Row> FindRow(TIndex index, CancellationToken cancellationToken)
    {
        if (_lru.TryGet(index, out var row))
        {
            return row;
        }

        row = await inner.FindRow(index, cancellationToken);

        _lru.AddOrUpdate(index, row);

        return row;
    }
}