namespace SortTask.Application;

public interface IInitializer
{
    Task Initialize(CancellationToken token);
}