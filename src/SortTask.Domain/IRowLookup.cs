namespace SortTask.Domain;

public interface IRowLookup
{
    Row FindRow(long offset, int length);
}
