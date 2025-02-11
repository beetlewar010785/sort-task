namespace SortTask.Domain;

public interface IInitializer
{
    Task Initialize(CancellationToken token);
}