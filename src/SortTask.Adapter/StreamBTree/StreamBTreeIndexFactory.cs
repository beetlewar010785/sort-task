using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeIndexFactory : IIndexFactory<StreamBTreeIndex>
{
    public StreamBTreeIndex CreateIndexFromRow(ReadRow row) => new(row.Position);
}