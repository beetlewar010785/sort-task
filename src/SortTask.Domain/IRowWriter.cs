namespace SortTask.Domain;

public interface IRowWriter
{
    void Write(Row row);
    void Flush();
}
