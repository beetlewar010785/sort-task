namespace SortTask.Domain;

public interface IIndexer
{
    void Index(Row row, long offset, int length);
}
