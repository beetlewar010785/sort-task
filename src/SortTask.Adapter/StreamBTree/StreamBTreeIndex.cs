using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public record StreamBTreeIndex(OphULong SentenceOph, long RowOffset, long RowLength) : IIndex;
