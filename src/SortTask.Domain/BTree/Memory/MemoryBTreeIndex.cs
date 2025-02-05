namespace SortTask.Domain.BTree.Memory;

public class MemoryBTreeIndex(MemoryBTreeRow bTreeRow) : IBTreeIndex
{
    public override string ToString()
    {
        return bTreeRow.ToString();
    }
}