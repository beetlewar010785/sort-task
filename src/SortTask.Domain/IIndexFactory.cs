namespace SortTask.Domain;

public interface IIndexFactory<out TIndex>
    where TIndex : IIndex
{
    public TIndex CreateIndexFromRow(Row row, long rowOffset, long rowLength);
}