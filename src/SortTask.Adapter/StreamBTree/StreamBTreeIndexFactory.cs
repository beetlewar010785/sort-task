using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public class StreamBTreeIndexFactory(IStringOph stringOph) : IIndexFactory<StreamBTreeIndex>
{
    public StreamBTreeIndex CreateIndexFromRow(
        Row row,
        long rowOffset,
        long rowLength)
    {
        var sentenceOph = stringOph.Hash(row.Sentence);
        return new StreamBTreeIndex(sentenceOph, rowOffset, rowLength);
    }
}