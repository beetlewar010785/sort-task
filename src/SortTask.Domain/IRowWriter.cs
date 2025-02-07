namespace SortTask.Domain;

public interface IRowWriter
{
    Task Write(WriteRow row);
    Task Flush(CancellationToken cancellationToken);
}