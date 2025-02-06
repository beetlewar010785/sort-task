namespace SortTask.Application;

public interface ISortRowsCommand
{
    Task Execute(CancellationToken cancellationToken);
}