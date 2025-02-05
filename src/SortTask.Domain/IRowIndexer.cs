namespace SortTask.Domain;

public interface IRowIndexer
{
    Task IndexRow(Row row);
}