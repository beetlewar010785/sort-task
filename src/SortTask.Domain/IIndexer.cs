namespace SortTask.Domain;

public interface IIndexer
{
    Task Index(OphULong oph, long offset, int length, CancellationToken cancellationToken);
}