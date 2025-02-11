using SortTask.Domain;

namespace SortTask.Adapter.StreamBTree;

public record StreamBTreeIndex(long RowOffset, long RowLength) : IIndex;